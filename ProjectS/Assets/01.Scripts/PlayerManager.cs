using Photon.Pun;
using System.Collections.Generic;
using StarterAssets;
using PS.Classes;
using PS.Core.Combat;
using PS.Core.Skills;
using PS.Data.Definitions;
using PS.Progression.Leveling;
using PS.Gameplay.Stats;
using PS.Gameplay.Skills;
using PS.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.InputSystem; // Added

namespace PS.Manager
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable, IDamageReductionReceiver, IUpgradeProvider, ICooldownProvider, ICombatant, ITeamDamageProvider
    {
        private const int MaxUpgradeLevel = 5;
        public float Health = 1f;
        public static GameObject LocalPlayerInstance;
        private static readonly List<PlayerManager> Instances = new List<PlayerManager>();

        [SerializeField] private GameObject beams;
        [SerializeField] private ClassDefinition classDefinitionOverride;
        [SerializeField] private ClassCatalog classCatalogOverride;
#if ENABLE_INPUT_SYSTEM
        [SerializeField] private InputActionAsset inputActionsOverride;
#endif
        private bool isFiring;
        private bool isDead;
        private bool wasAttackHeld;
        private bool wasSkill1Held;
        private bool wasSkill2Held;
        private bool wasSkill3Held;
        private PlayerClassState classState;
        private PlayerSkillExecutor skillExecutor;
        private PlayerStats stats;
        private ProgressionManager progression;
        private WarriorPassiveState passiveState;
        private StarterAssets.FirstPersonController firstPersonController;
        private float baseMoveSpeed;
        private float baseSprintSpeed;
        private bool hasMovementBaseline;
        private float lastMoveSpeedMultiplier = -1f;
        private float lastCooldownMultiplier = -1f;
        private float lastComboResetMultiplier = -1f;
        private float damageReductionMultiplier = 1f;
        private int bloodFrenzyStacks;
        private float bloodFrenzyExpiresAt;
        private bool guardianAngelUsed;
        private float invulnerableUntil;
        private bool resolvingSoulLink;
        private Vector3 networkPosition;
        private Quaternion networkRotation;
        private bool hasNetworkState;
#if ENABLE_INPUT_SYSTEM
        private StarterAssetsInputs input;
        private PlayerInput playerInput;
        private InputAction attackAction;
        private InputAction skill1Action;
        private InputAction skill2Action;
        private InputAction skill3Action;
        private bool loggedMissingInputActions;
#endif

        private void Awake()
        {
            Instances.Add(this);
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                AssignLocalCameraTarget();
                PlayerInput localInput = GetComponent<PlayerInput>();
                if (localInput != null)
                {
#if ENABLE_INPUT_SYSTEM
                    if (inputActionsOverride != null && localInput.actions != inputActionsOverride)
                    {
                        localInput.actions = inputActionsOverride;
                    }
#endif
                    localInput.enabled = true;
                }
                StarterAssetsInputs localInputs = GetComponent<StarterAssetsInputs>();
                if (localInputs != null)
                {
                    localInputs.enabled = true;
                }
            }
            else
            {
                // It's a remote player. Disable input, character controller, camera and audio listener.
                GetComponent<PlayerInput>().enabled = false;
                GetComponent<StarterAssets.FirstPersonController>().enabled = false;
                StarterAssetsInputs localInputs = GetComponent<StarterAssetsInputs>();
                if (localInputs != null)
                {
                    localInputs.enabled = false;
                }

                Camera camera = GetComponentInChildren<Camera>();
                if (camera != null) camera.enabled = false;

                AudioListener listener = GetComponentInChildren<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
            
            input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            playerInput = GetComponent<PlayerInput>();
            CacheInputActions();
#endif
            stats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();
            progression = GetComponent<ProgressionManager>() ?? gameObject.AddComponent<ProgressionManager>();
            firstPersonController = GetComponent<StarterAssets.FirstPersonController>();
            passiveState = GetComponent<WarriorPassiveState>() ?? gameObject.AddComponent<WarriorPassiveState>();
            // Defer class/skill initialization until Start so dependent components are ready.

            if (beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                beams.SetActive(false);
            }
            // Keep the player manager across scene loads.

        }

        private void AssignLocalCameraTarget()
        {
            var controller = GetComponent<StarterAssets.FirstPersonController>();
            if (controller == null || controller.CinemachineCameraTarget == null)
            {
                return;
            }

            var virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            if (virtualCamera == null)
            {
                return;
            }

            virtualCamera.Follow = controller.CinemachineCameraTarget.transform;
            virtualCamera.LookAt = controller.CinemachineCameraTarget.transform;
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            DamageEvents.DamageApplied += OnDamageApplied;
            KillEvents.Killed += OnKilled;

            InitializeClassAndSkills();

            if (passiveState != null)
            {
                passiveState.Changed += OnPassiveChanged;
            }

            if (skillExecutor != null)
            {
                skillExecutor.SkillExecuted += OnSkillExecuted;
            }
        }

        private void OnDestroy()
        {
            Instances.Remove(this);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            DamageEvents.DamageApplied -= OnDamageApplied;
            KillEvents.Killed -= OnKilled;

            if (passiveState != null)
            {
                passiveState.Changed -= OnPassiveChanged;
            }

            if (skillExecutor != null)
            {
                skillExecutor.SkillExecuted -= OnSkillExecuted;
            }

            // If this was the local player instance, clean up the static reference
            if (photonView.IsMine && LocalPlayerInstance == gameObject)
            {
                LocalPlayerInstance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadingMode)
        {
            // After a scene load, check if the local player is in a valid position.
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0, 5, 0);
            }

            if (photonView.IsMine)
            {
                AssignLocalCameraTarget();
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                EnsureActionMap();
                ProcessInput();
                ApplyRuntimeModifiers();
                if (!isDead && Health <= 0f)
                {
                    isDead = true;
                    Debug.LogWarning($"[PlayerManager] Health <= 0. Leaving room. Health={Health}, InRoom={PhotonNetwork.InRoom}");
                    GameManager.Instance.LeaveRoom();
                }
            }
            else if (hasNetworkState)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Slerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }
            if (bloodFrenzyStacks > 0 && Time.time >= bloodFrenzyExpiresAt)
            {
                bloodFrenzyStacks = 0;
            }

            if (invulnerableUntil > 0f && Time.time >= invulnerableUntil)
            {
                invulnerableUntil = 0f;
            }
            // Keep beam active state in sync with firing.
            if (beams != null && isFiring != beams.activeInHierarchy)
            {
                beams.SetActive(isFiring);
            }
        }

        public void SetClass(ClassDefinition definition)
        {
            if (classState == null)
            {
                classState = GetComponent<PlayerClassState>() ?? gameObject.AddComponent<PlayerClassState>();
            }

            classState.SetClass(definition);
            ApplyClassSkills();
        }

        public bool TryUseSkill(SkillSlot slot)
        {
            if (!photonView.IsMine || skillExecutor == null)
            {
                Debug.LogWarning($"[PlayerManager] TryUseSkill blocked. IsMine={photonView.IsMine}, skillExecutor={(skillExecutor == null ? "null" : "ok")} slot={slot}");
                return false;
            }

            return skillExecutor.TryExecuteSkill(slot);
        }

        public float GetSkillCooldownRemaining(SkillSlot slot)
        {
            if (skillExecutor == null)
            {
                return 0f;
            }

            return skillExecutor.GetCooldownRemaining(slot);
        }

        public float GetSkillCooldownDuration(SkillSlot slot)
        {
            if (skillExecutor == null)
            {
                return 0f;
            }

            return skillExecutor.GetCooldownDuration(slot);
        }

        float ICooldownProvider.GetCooldownRemaining(SkillSlot slot)
        {
            return GetSkillCooldownRemaining(slot);
        }

        float ICooldownProvider.GetCooldownDuration(SkillSlot slot)
        {
            return GetSkillCooldownDuration(slot);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Health -= 0.1f * damageReductionMultiplier;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!photonView.IsMine)
            {
                return;
            }
            if (!other.name.Contains("Beam"))
            {
                return;
            }
            Debug.Log("Ouch!!");
            Health -= 0.1f * damageReductionMultiplier * Time.deltaTime;
        }

        public void SetDamageReductionMultiplier(float multiplier)
        {
            damageReductionMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);
        }

        float ICombatant.Health => Health;
        float ICombatant.MaxHealth => stats != null ? stats.MaxHealth : Health;
        bool ICombatant.IsAlive => Health > 0f;
        Vector3 ICombatant.Position => transform.position;

        public void ApplyDamage(DamageInfo info)
        {
            ApplyDamageInternal(info, false);
        }

        private void ApplyDamageInternal(DamageInfo info, bool ignoreSoulLink)
        {
            if (info.SourceId == gameObject.GetInstanceID())
            {
                return;
            }

            if (invulnerableUntil > Time.time)
            {
                return;
            }

            if (!ignoreSoulLink
                && TeamUpgradeManager.Instance != null
                && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.SoulLink)
                && !resolvingSoulLink)
            {
                int partyCount = Mathf.Max(1, Instances.Count);
                float sharedAmount = info.Amount / partyCount;
                resolvingSoulLink = true;
                for (int i = 0; i < Instances.Count; i++)
                {
                    if (Instances[i] == null)
                    {
                        continue;
                    }

                    DamageInfo sharedInfo = info;
                    sharedInfo.Amount = sharedAmount;
                    Instances[i].ApplyDamageInternal(sharedInfo, true);
                }
                resolvingSoulLink = false;
                return;
            }

            float reduction = stats != null ? stats.DamageReductionPercent : 0f;
            float finalAmount = info.Amount * damageReductionMultiplier;
            finalAmount *= 1f - Mathf.Clamp(reduction, 0f, 0.5f);

            if (TeamUpgradeManager.Instance != null
                && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.GuardianAngel)
                && !guardianAngelUsed
                && finalAmount >= Health)
            {
                guardianAngelUsed = true;
                invulnerableUntil = Time.time + 3f;
                float applied = Mathf.Max(0f, Health - 1f);
                Health = Mathf.Max(1f, Health - applied);

                DamageInfo resolved = info;
                resolved.Amount = applied;
                DamageEvents.Raise(resolved);
                return;
            }

            Health = Mathf.Max(0f, Health - finalAmount);

            DamageInfo resolvedInfo = info;
            resolvedInfo.Amount = finalAmount;
            DamageEvents.Raise(resolvedInfo);
        }

        public float GetTeamDamageMultiplier()
        {
            if (TeamUpgradeManager.Instance != null && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.BloodFrenzy))
            {
                return 1f + (bloodFrenzyStacks * 0.05f);
            }

            return 1f;
        }

        public void Heal(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            float maxHealth = stats != null ? stats.MaxHealth : Health;
            Health = Mathf.Min(maxHealth, Health + amount);
        }

        private void OnSkillExecuted(SkillSlot slot)
        {
            if (slot != SkillSlot.R)
            {
                return;
            }

            if (TeamUpgradeManager.Instance == null || !TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.Adrenaline))
            {
                return;
            }

            for (int i = 0; i < Instances.Count; i++)
            {
                PlayerManager player = Instances[i];
                if (player == null || player.skillExecutor == null)
                {
                    continue;
                }

                player.skillExecutor.ReduceCooldown(SkillSlot.Q, 5f);
                player.skillExecutor.ReduceCooldown(SkillSlot.E, 5f);
            }
        }

        private void ProcessInput()
        {
            bool attackPressed = input != null && input.attack;
            bool skill1Pressed = input != null && input.skill1;
            bool skill2Pressed = input != null && input.skill2;
            bool skill3Pressed = input != null && input.skill3;

#if ENABLE_INPUT_SYSTEM
            if (attackAction != null)
            {
                attackPressed = attackAction.IsPressed();
            }
            if (skill1Action != null)
            {
                skill1Pressed = skill1Action.IsPressed();
            }
            if (skill2Action != null)
            {
                skill2Pressed = skill2Action.IsPressed();
            }
            if (skill3Action != null)
            {
                skill3Pressed = skill3Action.IsPressed();
            }
#endif

            bool attackDown = attackPressed && !wasAttackHeld;
#if ENABLE_INPUT_SYSTEM
            if (attackAction != null)
            {
                attackDown = attackAction.WasPressedThisFrame();
            }
#endif

            if (attackDown)
            {
                TryUseSkill(SkillSlot.Basic);
                Debug.Log("Basic attack triggered.");
            }

            if (attackPressed)
            {
                if (!isFiring)
                {
                    Debug.Log("Firing started.");
                    isFiring = true;
                }
            }
            else if (!attackPressed)
            {
                if (isFiring)
                {
                    isFiring = false;
                }
            }

            HandleSkillInput(skill1Pressed, skill2Pressed, skill3Pressed);
            wasAttackHeld = attackPressed;
        }

        private void HandleSkillInput(bool skill1Pressed, bool skill2Pressed, bool skill3Pressed)
        {
            bool skill1Down = skill1Pressed && !wasSkill1Held;
            bool skill2Down = skill2Pressed && !wasSkill2Held;
            bool skill2Up = !skill2Pressed && wasSkill2Held;
            bool skill3Down = skill3Pressed && !wasSkill3Held;

#if ENABLE_INPUT_SYSTEM
            if (skill1Action != null)
            {
                skill1Down = skill1Action.WasPressedThisFrame();
            }
            if (skill2Action != null)
            {
                skill2Down = skill2Action.WasPressedThisFrame();
                skill2Up = skill2Action.WasReleasedThisFrame();
            }
            if (skill3Action != null)
            {
                skill3Down = skill3Action.WasPressedThisFrame();
            }
#endif

            if (skill1Down)
            {
                bool used = TryUseSkill(SkillSlot.Q);
                Debug.Log($"Skill Q pressed. Used={used}.");
            }

            if (skill2Down)
            {
                if (skillExecutor != null && skillExecutor.IsChannelSkill(SkillSlot.E))
                {
                    skillExecutor.BeginChannel(SkillSlot.E);
                }
                else
                {
                    bool used = TryUseSkill(SkillSlot.E);
                    Debug.Log($"Skill E pressed. Used={used}.");
                }
            }

            if (skill2Up)
            {
                if (skillExecutor != null && skillExecutor.IsChannelSkill(SkillSlot.E))
                {
                    skillExecutor.EndChannel(SkillSlot.E);
                }
            }

            if (skill3Down)
            {
                bool used = TryUseSkill(SkillSlot.R);
                Debug.Log($"Skill R pressed. Used={used}.");
            }

            wasSkill1Held = skill1Pressed;
            wasSkill2Held = skill2Pressed;
            wasSkill3Held = skill3Pressed;
        }

#if ENABLE_INPUT_SYSTEM
        private void CacheInputActions()
        {
            if (playerInput == null || playerInput.actions == null)
            {
                return;
            }

            attackAction = playerInput.actions["Attack"];
            skill1Action = playerInput.actions["Skill1"];
            skill2Action = playerInput.actions["Skill2"];
            skill3Action = playerInput.actions["Skill3"];

            if (!loggedMissingInputActions
                && (attackAction == null || skill1Action == null || skill2Action == null || skill3Action == null))
            {
                loggedMissingInputActions = true;
                Debug.LogWarning(
                    "[PlayerManager] Missing input actions. Check InputSystem_Actions: Attack/Skill1/Skill2/Skill3.");
            }
        }

        private void EnsureActionMap()
        {
            if (playerInput == null || playerInput.actions == null)
            {
                return;
            }

            if (!playerInput.enabled)
            {
                return;
            }

            if (!playerInput.actions.enabled)
            {
                playerInput.actions.Enable();
            }

            if (playerInput.currentActionMap == null || playerInput.currentActionMap.name != "Player")
            {
                playerInput.SwitchCurrentActionMap("Player");
            }
        }
#endif

        private void ApplyRuntimeModifiers()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            float statMultiplier = stats != null ? stats.GetMoveSpeedMultiplier() : 1f;
            float passiveMultiplier = passiveState != null ? passiveState.MoveSpeedMultiplier : 1f;
            float channelMultiplier = skillExecutor != null ? skillExecutor.CurrentChannelMoveSpeedMultiplier : 1f;
            float moveMultiplier = statMultiplier * passiveMultiplier * channelMultiplier;
            if (TeamUpgradeManager.Instance != null && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.TitaniumSkin))
            {
                moveMultiplier *= 0.9f;
            }

            if (firstPersonController != null && hasMovementBaseline
                && Mathf.Abs(moveMultiplier - lastMoveSpeedMultiplier) > 0.001f)
            {
                firstPersonController.MoveSpeed = baseMoveSpeed * moveMultiplier;
                firstPersonController.SprintSpeed = baseSprintSpeed * moveMultiplier;
                lastMoveSpeedMultiplier = moveMultiplier;
            }

            float cooldownMultiplier = passiveState != null ? passiveState.CooldownMultiplier : 1f;
            if (skillExecutor != null && Mathf.Abs(cooldownMultiplier - lastCooldownMultiplier) > 0.001f)
            {
                skillExecutor.SetCooldownMultiplier(cooldownMultiplier);
                lastCooldownMultiplier = cooldownMultiplier;
            }

            float comboResetMultiplier = passiveState != null ? passiveState.ComboResetMultiplier : 1f;
            if (skillExecutor != null && Mathf.Abs(comboResetMultiplier - lastComboResetMultiplier) > 0.001f)
            {
                skillExecutor.SetBasicComboResetMultiplier(comboResetMultiplier);
                lastComboResetMultiplier = comboResetMultiplier;
            }
        }

        private void OnKilled(KillInfo info)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (info.SourceId != gameObject.GetInstanceID())
            {
                return;
            }

            if (info.Slot == SkillSlot.Q && skillExecutor != null
                && skillExecutor.ShouldResetCooldownOnKill(SkillSlot.Q))
            {
                skillExecutor.ResetCooldown(SkillSlot.Q);
            }

            if (TeamUpgradeManager.Instance != null && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.BloodFrenzy))
            {
                bloodFrenzyStacks = Mathf.Clamp(bloodFrenzyStacks + 1, 0, 10);
                bloodFrenzyExpiresAt = Time.time + 5f;
            }
        }

        private void OnPassiveChanged()
        {
            ApplyRuntimeModifiers();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Local player: send state to others.

                stream.SendNext(Health);
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                // Remote player: receive state.

                this.Health = (float)stream.ReceiveNext();
                networkPosition = (Vector3)stream.ReceiveNext();
                networkRotation = (Quaternion)stream.ReceiveNext();
                hasNetworkState = true;
            }

        }

        private void OnDamageApplied(DamageInfo info)
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (info.SourceId != gameObject.GetInstanceID())
            {
                return;
            }

            if (info.Slot == SkillSlot.Basic)
            {
                skillExecutor?.ReduceCooldown(SkillSlot.Q, 1f);
                passiveState?.RegisterHit();
            }

            if (TeamUpgradeManager.Instance != null && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.VampiricOath))
            {
                Heal(info.Amount * 0.05f);
            }
        }

        private void InitializeClassAndSkills()
        {
            classState = GetComponent<PlayerClassState>() ?? gameObject.AddComponent<PlayerClassState>();
            skillExecutor = GetComponent<PlayerSkillExecutor>() ?? gameObject.AddComponent<PlayerSkillExecutor>();

            if (classDefinitionOverride != null)
            {
                classState.SetClass(classDefinitionOverride);
            }
            else if (classState.CurrentClass == null)
            {
                ClassDefinition resolved = ResolveDefaultClass();
                if (resolved != null)
                {
                    classState.SetClass(resolved);
                }
            }

            ApplyClassSkills();
            Debug.Log($"[PlayerManager] Class resolved: {(classState != null && classState.CurrentClass != null ? classState.CurrentClass.displayName : "null")}");
        }

        private ClassDefinition ResolveDefaultClass()
        {
            ClassCatalog catalog = classCatalogOverride;
            if (catalog == null)
            {
                catalog = Resources.Load<ClassCatalog>("ClassCatalog");
            }

            if (catalog != null)
            {
                if (catalog.defaultClass != null)
                {
                    return catalog.defaultClass;
                }

                if (catalog.classes != null && catalog.classes.Length > 0)
                {
                    return catalog.classes[0];
                }
            }

            return TestClassFactory.GetOrCreate();
        }

        private void ApplyClassSkills()
        {
            if (classState == null || skillExecutor == null || classState.CurrentClass == null)
            {
                return;
            }

            skillExecutor.SetSkills(
                classState.CurrentClass.basicAttack,
                classState.CurrentClass.skillQ,
                classState.CurrentClass.skillE,
                classState.CurrentClass.skillR);

            ConfigureUpgradeTracks(classState.CurrentClass);
            ConfigurePassiveTrack(classState.CurrentClass);

            ApplyClassStats();
        }

        private void ConfigureUpgradeTracks(ClassDefinition definition)
        {
            if (skillExecutor == null || definition == null)
            {
                return;
            }

            SkillUpgradeTrackBase basicTrack = ResolveUpgradeTrack(definition, SkillSlot.Basic);
            if (basicTrack != null)
            {
                skillExecutor.SetUpgradeTrack(SkillSlot.Basic, basicTrack);
            }

            SkillUpgradeTrackBase qTrack = ResolveUpgradeTrack(definition, SkillSlot.Q);
            if (qTrack != null)
            {
                skillExecutor.SetUpgradeTrack(SkillSlot.Q, qTrack);
            }

            SkillUpgradeTrackBase eTrack = ResolveUpgradeTrack(definition, SkillSlot.E);
            if (eTrack != null)
            {
                skillExecutor.SetUpgradeTrack(SkillSlot.E, eTrack);
            }

            SkillUpgradeTrackBase rTrack = ResolveUpgradeTrack(definition, SkillSlot.R);
            if (rTrack != null)
            {
                skillExecutor.SetUpgradeTrack(SkillSlot.R, rTrack);
            }
        }

        private SkillUpgradeTrackBase ResolveUpgradeTrack(ClassDefinition definition, SkillSlot slot)
        {
            SkillUpgradeTrackBase track = slot switch
            {
                SkillSlot.Basic => definition.basicUpgradeTrack,
                SkillSlot.Q => definition.skillQUpgradeTrack,
                SkillSlot.E => definition.skillEUpgradeTrack,
                SkillSlot.R => definition.skillRUpgradeTrack,
                _ => null
            };

            if (track != null)
            {
                return track;
            }

            if (WarriorUpgradeLibrary.IsWarrior(definition))
            {
                return WarriorUpgradeLibrary.GetTrack(slot);
            }

            return null;
        }

        public List<UpgradeOption> BuildUpgradeOptions(int count)
        {
            List<UpgradeOption> options = new List<UpgradeOption>();
            List<UpgradeType> candidates = new List<UpgradeType>();

            if (GetSkillLevel(SkillSlot.Basic) < MaxUpgradeLevel)
            {
                candidates.Add(UpgradeType.Basic);
            }
            if (GetSkillLevel(SkillSlot.Q) < MaxUpgradeLevel)
            {
                candidates.Add(UpgradeType.Q);
            }
            if (GetSkillLevel(SkillSlot.E) < MaxUpgradeLevel)
            {
                candidates.Add(UpgradeType.E);
            }
            if (GetSkillLevel(SkillSlot.R) < MaxUpgradeLevel)
            {
                candidates.Add(UpgradeType.R);
            }
            if (GetPassiveLevel() < MaxUpgradeLevel)
            {
                candidates.Add(UpgradeType.Passive);
            }

            int pickCount = Mathf.Min(count, candidates.Count);
            for (int i = 0; i < pickCount; i++)
            {
                int index = Random.Range(0, candidates.Count);
                UpgradeType type = candidates[index];
                candidates.RemoveAt(index);
                options.Add(BuildOption(type));
            }

            return options;
        }

        public void ApplyUpgradeChoice(UpgradeOption option)
        {
            switch (option.Type)
            {
                case UpgradeType.Basic:
                    SetSkillLevel(SkillSlot.Basic, GetSkillLevel(SkillSlot.Basic) + 1);
                    break;
                case UpgradeType.Q:
                    SetSkillLevel(SkillSlot.Q, GetSkillLevel(SkillSlot.Q) + 1);
                    break;
                case UpgradeType.E:
                    SetSkillLevel(SkillSlot.E, GetSkillLevel(SkillSlot.E) + 1);
                    break;
                case UpgradeType.R:
                    SetSkillLevel(SkillSlot.R, GetSkillLevel(SkillSlot.R) + 1);
                    break;
                case UpgradeType.Passive:
                    SetPassiveLevel(GetPassiveLevel() + 1);
                    break;
            }
        }

        private UpgradeOption BuildOption(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.Basic => BuildBasicOption(),
                UpgradeType.Q => BuildQOption(),
                UpgradeType.E => BuildEOption(),
                UpgradeType.R => BuildROption(),
                UpgradeType.Passive => BuildPassiveOption(),
                _ => default
            };
        }

        private UpgradeOption BuildBasicOption()
        {
            int nextLevel = Mathf.Clamp(GetSkillLevel(SkillSlot.Basic) + 1, 1, MaxUpgradeLevel);
            WarriorBasicUpgradeTrack track = classState?.CurrentClass?.basicUpgradeTrack;
            WarriorBasicUpgradeStep step = default;
            string description = "업그레이드";
            if (track != null && track.TryGetStep(nextLevel, out step))
            {
                description = DescribeBasicStep(step);
            }

            return new UpgradeOption
            {
                Type = UpgradeType.Basic,
                Title = $"거친 숨결 (LMB) Lv.{nextLevel}",
                Description = description
            };
        }

        private UpgradeOption BuildQOption()
        {
            int nextLevel = Mathf.Clamp(GetSkillLevel(SkillSlot.Q) + 1, 1, MaxUpgradeLevel);
            WarriorQUpgradeTrack track = classState?.CurrentClass?.skillQUpgradeTrack;
            WarriorQUpgradeStep step = default;
            string description = "업그레이드";
            if (track != null && track.TryGetStep(nextLevel, out step))
            {
                description = DescribeQStep(step);
            }

            return new UpgradeOption
            {
                Type = UpgradeType.Q,
                Title = $"윈드 터널 (Q) Lv.{nextLevel}",
                Description = description
            };
        }

        private UpgradeOption BuildEOption()
        {
            int nextLevel = Mathf.Clamp(GetSkillLevel(SkillSlot.E) + 1, 1, MaxUpgradeLevel);
            WarriorEUpgradeTrack track = classState?.CurrentClass?.skillEUpgradeTrack;
            WarriorEUpgradeStep step = default;
            string description = "업그레이드";
            if (track != null && track.TryGetStep(nextLevel, out step))
            {
                description = DescribeEStep(step);
            }

            return new UpgradeOption
            {
                Type = UpgradeType.E,
                Title = $"태풍의 눈 (E) Lv.{nextLevel}",
                Description = description
            };
        }

        private UpgradeOption BuildROption()
        {
            int nextLevel = Mathf.Clamp(GetSkillLevel(SkillSlot.R) + 1, 1, MaxUpgradeLevel);
            WarriorRUpgradeTrack track = classState?.CurrentClass?.skillRUpgradeTrack;
            WarriorRUpgradeStep step = default;
            string description = "업그레이드";
            if (track != null && track.TryGetStep(nextLevel, out step))
            {
                description = DescribeRStep(step);
            }

            return new UpgradeOption
            {
                Type = UpgradeType.R,
                Title = $"템페스트 엣지 (R) Lv.{nextLevel}",
                Description = description
            };
        }

        private UpgradeOption BuildPassiveOption()
        {
            int nextLevel = Mathf.Clamp(GetPassiveLevel() + 1, 1, MaxUpgradeLevel);
            PassiveUpgradeTrack track = classState?.CurrentClass?.passiveUpgradeTrack;
            PassiveUpgradeStep step = default;
            string description = "업그레이드";
            if (track != null && track.TryGetStep(nextLevel, out step))
            {
                description = DescribePassiveStep(step);
            }

            return new UpgradeOption
            {
                Type = UpgradeType.Passive,
                Title = $"폭풍의 기세 (P) Lv.{nextLevel}",
                Description = description
            };
        }

        private int GetSkillLevel(SkillSlot slot)
        {
            return skillExecutor != null ? skillExecutor.GetSkillLevel(slot) : 0;
        }

        private void SetSkillLevel(SkillSlot slot, int level)
        {
            if (skillExecutor == null)
            {
                return;
            }

            skillExecutor.SetSkillLevel(slot, Mathf.Clamp(level, 0, MaxUpgradeLevel));
        }

        private int GetPassiveLevel()
        {
            return passiveState != null ? passiveState.CurrentLevel : 0;
        }

        private void SetPassiveLevel(int level)
        {
            if (passiveState == null)
            {
                return;
            }

            passiveState.SetLevel(Mathf.Clamp(level, 0, MaxUpgradeLevel));
            ApplyRuntimeModifiers();
        }

        private string DescribeBasicStep(WarriorBasicUpgradeStep step)
        {
            if (step.damageBonusPercent > 0f)
            {
                return $"피해량 +{Mathf.RoundToInt(step.damageBonusPercent * 100f)}%";
            }
            if (step.comboResetMultiplier > 0f && step.comboResetMultiplier < 1f)
            {
                float reduction = (1f - step.comboResetMultiplier) * 100f;
                return $"후딜레이 -{Mathf.RoundToInt(reduction)}%";
            }
            if (step.prefabScaleBonusPercent > 0f)
            {
                return $"검풍 범위 {Mathf.RoundToInt(step.prefabScaleBonusPercent * 100f)}% 증가";
            }
            if (step.finisherFixedDamage)
            {
                return "진공 베기: 마지막 3타 고정 피해";
            }
            return "업그레이드";
        }

        private string DescribeQStep(WarriorQUpgradeStep step)
        {
            if (step.cooldownDelta < 0f)
            {
                return $"쿨타임 {step.cooldownDelta}초";
            }
            if (step.damageBonusPercent > 0f)
            {
                return $"피해량 +{Mathf.RoundToInt(step.damageBonusPercent * 100f)}%";
            }
            if (step.dashDamageReductionMultiplier > 0f && step.dashDamageReductionMultiplier < 1f)
            {
                float reduction = (1f - step.dashDamageReductionMultiplier) * 100f;
                return $"시전 중 딜감 {Mathf.RoundToInt(reduction)}%";
            }
            if (step.resetCooldownOnKill)
            {
                return "처치 시 쿨타임 초기화";
            }
            return "업그레이드";
        }

        private string DescribeEStep(WarriorEUpgradeStep step)
        {
            if (step.durationBonusSeconds > 0f)
            {
                return $"지속시간 +{step.durationBonusSeconds}초";
            }
            if (step.cooldownDelta < 0f)
            {
                return $"쿨타임 {step.cooldownDelta}초";
            }
            if (step.channelMoveSpeedMultiplier > 0f)
            {
                int percent = Mathf.RoundToInt(step.channelMoveSpeedMultiplier * 100f);
                return $"회전 중 이동 가능 ({percent}%)";
            }
            if (step.damageBonusPercent > 0f)
            {
                return $"피해량 +{Mathf.RoundToInt(step.damageBonusPercent * 100f)}%";
            }
            if (step.prefabScaleBonusPercent > 0f || step.pullRadiusBonus > 0f)
            {
                return "거대 태풍: 범위 증가 + 강한 흡입";
            }
            return "업그레이드";
        }

        private string DescribeRStep(WarriorRUpgradeStep step)
        {
            if (step.damageBonusPercent > 0f)
            {
                return $"피해량 +{Mathf.RoundToInt(step.damageBonusPercent * 100f)}%";
            }
            if (step.rangeBonusPercent > 0f)
            {
                return $"범위 +{Mathf.RoundToInt(step.rangeBonusPercent * 100f)}%";
            }
            if (step.groundDotDuration > 0f)
            {
                return "폭풍의 여파: 바닥 장판딜 생성";
            }
            if (step.enableDoubleHit)
            {
                return "쌍둥이 폭풍: 2회 발사";
            }
            return "업그레이드";
        }

        private string DescribePassiveStep(PassiveUpgradeStep step)
        {
            if (step.attackSpeedPerStackPercent > 0f && step.stackDurationSeconds <= 5f)
            {
                return $"공속 +{Mathf.RoundToInt(step.attackSpeedPerStackPercent * 100f)}%";
            }
            if (step.stackDurationSeconds > 5f)
            {
                return $"지속시간 {Mathf.RoundToInt(step.stackDurationSeconds)}초";
            }
            if (step.moveSpeedPerStackPercent > 0f)
            {
                return $"스택당 이동속도 +{Mathf.RoundToInt(step.moveSpeedPerStackPercent * 100f)}%";
            }
            if (step.maxStacks > 5)
            {
                return $"최대 중첩 {step.maxStacks}회";
            }
            if (step.cooldownReductionAtMaxPercent > 0f)
            {
                return $"풀스택시 쿨감 {Mathf.RoundToInt(step.cooldownReductionAtMaxPercent * 100f)}%";
            }
            return "업그레이드";
        }

        private void ConfigurePassiveTrack(ClassDefinition definition)
        {
            if (passiveState == null || definition == null)
            {
                return;
            }

            if (definition.passiveUpgradeTrack != null)
            {
                passiveState.SetUpgradeTrack(definition.passiveUpgradeTrack);
            }
        }

        private void ApplyClassStats()
        {
            if (stats == null || classState == null || classState.CurrentClass == null)
            {
                return;
            }

            stats.SetBaseStats(classState.CurrentClass.stats);
            Health = stats.MaxHealth;

            if (firstPersonController == null)
            {
                return;
            }

            if (!hasMovementBaseline)
            {
                baseMoveSpeed = firstPersonController.MoveSpeed;
                baseSprintSpeed = firstPersonController.SprintSpeed;
                hasMovementBaseline = true;
            }
            ApplyRuntimeModifiers();
        }
    }
}
