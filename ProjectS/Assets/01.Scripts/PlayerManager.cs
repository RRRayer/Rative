using Photon.Pun;
using StarterAssets;
using ProjectS.Classes;
using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using ProjectS.Gameplay.Skills;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.InputSystem; // Added

namespace PS.Manager
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        public float Health = 1f;
        public static GameObject LocalPlayerInstance;

        [SerializeField] private GameObject beams;
        [SerializeField] private ClassDefinition classDefinitionOverride;
        [SerializeField] private ClassCatalog classCatalogOverride;
        private bool isFiring;
        private bool isDead;
        private bool wasAttackHeld;
        private bool wasSkill1Held;
        private bool wasSkill2Held;
        private bool wasSkill3Held;
        private PlayerClassState classState;
        private PlayerSkillExecutor skillExecutor;
    #if ENABLE_INPUT_SYSTEM
        private StarterAssetsInputs input;
    #endif

        private void Awake()
        {
            if (photonView.IsMine)
            {
                LocalPlayerInstance = gameObject;
                AssignLocalCameraTarget();
            }
            else
            {
                // It's a remote player. Disable input, character controller, camera and audio listener.
                GetComponent<PlayerInput>().enabled = false;
                GetComponent<StarterAssets.FirstPersonController>().enabled = false;

                Camera camera = GetComponentInChildren<Camera>();
                if (camera != null) camera.enabled = false;

                AudioListener listener = GetComponentInChildren<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
            
            input = GetComponent<StarterAssetsInputs>();
            InitializeClassAndSkills();

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
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

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
                ProcessInput();
                // Health handling.

                {
                    isDead = true;
                    GameManager.Instance.LeaveRoom();
                }
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
            Health -= 0.1f;
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
            Health -= 0.1f * Time.deltaTime;
        }

        private void ProcessInput()
        {
            if (input.attack && !wasAttackHeld)
            {
                TryUseSkill(SkillSlot.Basic);
                Debug.Log("Basic attack triggered.");
            }

            if (input.attack)
            {
                if (!isFiring)
                {
                    Debug.Log("Firing started.");
                    isFiring = true;
                }
            }
            else if (!input.attack)
            {
                if (isFiring)
                {
                    isFiring = false;
                }
            }

            HandleSkillInput();
            wasAttackHeld = input.attack;
        }

        private void HandleSkillInput()
        {
            if (input == null)
            {
                return;
            }

            if (input.skill1 && !wasSkill1Held)
            {
                TryUseSkill(SkillSlot.Q);
            }

            if (input.skill2 && !wasSkill2Held)
            {
                TryUseSkill(SkillSlot.E);
            }

            if (input.skill3 && !wasSkill3Held)
            {
                TryUseSkill(SkillSlot.R);
            }

            wasSkill1Held = input.skill1;
            wasSkill2Held = input.skill2;
            wasSkill3Held = input.skill3;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Local player: send state to others.

                stream.SendNext(Health);
            }
            else
            {
                // Remote player: receive state.

                this.Health = (float)stream.ReceiveNext();
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
        }
    }
}
