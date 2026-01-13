using System.Collections;
using System.Collections.Generic;
using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using ProjectS.Gameplay.Combat;
using ProjectS.Gameplay.Skills.Behaviours;
using ProjectS.Gameplay.Stats;
using UnityEngine;

namespace ProjectS.Gameplay.Skills
{
    public sealed class PlayerSkillExecutor : MonoBehaviour, ISkillExecutor
    {
        [SerializeField] private Transform spawnOrigin;
        [SerializeField] private bool logSkillUsage = true;

        private readonly Dictionary<SkillSlot, SkillDefinition> skills = new Dictionary<SkillSlot, SkillDefinition>();
        private readonly Dictionary<SkillSlot, float> cooldownEndTimes = new Dictionary<SkillSlot, float>();
        private readonly Dictionary<SkillSlot, int> skillLevels = new Dictionary<SkillSlot, int>();
        private readonly Dictionary<SkillSlot, SkillUpgradeTrackBase> upgradeTracks = new Dictionary<SkillSlot, SkillUpgradeTrackBase>();
        private readonly Dictionary<SkillSlot, SkillUpgradeState> upgradeStates = new Dictionary<SkillSlot, SkillUpgradeState>();
        private PlayerStats playerStats;
        private CharacterController characterController;
        private IDamageReductionReceiver damageReductionReceiver;
        private int basicComboStep;
        private float lastBasicTime;
        private Coroutine channelRoutine;
        private SkillSlot channelSlot;
        private ChannelSkillBehaviour channelBehaviour;
        private bool channelActive;
        private WeaponHitbox weaponHitbox;
        private float channelMoveSpeedMultiplier = 1f;
        private float cooldownMultiplier = 1f;
        private float basicComboResetMultiplier = 1f;

        private void Awake()
        {
            skills.Clear();
            cooldownEndTimes.Clear();
            skillLevels.Clear();
            upgradeTracks.Clear();
            upgradeStates.Clear();

            if (spawnOrigin == null)
            {
                spawnOrigin = transform;
            }

            playerStats = GetComponent<PlayerStats>();
            characterController = GetComponent<CharacterController>();
            damageReductionReceiver = GetComponent<IDamageReductionReceiver>();
            weaponHitbox = GetComponentInChildren<WeaponHitbox>();
            if (weaponHitbox != null)
            {
                weaponHitbox.ConfigureOwner(gameObject);
            }
        }

        public void SetSkills(SkillDefinition basic, SkillDefinition skillQ, SkillDefinition skillE, SkillDefinition skillR)
        {
            SetSkill(SkillSlot.Basic, basic);
            SetSkill(SkillSlot.Q, skillQ);
            SetSkill(SkillSlot.E, skillE);
            SetSkill(SkillSlot.R, skillR);
        }

        public void SetUpgradeTrack(SkillSlot slot, SkillUpgradeTrackBase track)
        {
            upgradeTracks[slot] = track;
            UpdateUpgradeState(slot);
        }

        public void SetSkillLevel(SkillSlot slot, int level)
        {
            skillLevels[slot] = Mathf.Max(0, level);
            UpdateUpgradeState(slot);
        }

        public SkillUpgradeState GetUpgradeState(SkillSlot slot)
        {
            if (upgradeStates.TryGetValue(slot, out SkillUpgradeState state))
            {
                return state;
            }

            return SkillUpgradeState.CreateDefault();
        }

        public int GetSkillLevel(SkillSlot slot)
        {
            return skillLevels.TryGetValue(slot, out int level) ? level : 0;
        }

        public bool ShouldResetCooldownOnKill(SkillSlot slot)
        {
            return GetUpgradeState(slot).ResetCooldownOnKill;
        }

        public void ResetCooldown(SkillSlot slot)
        {
            cooldownEndTimes[slot] = Time.time;
        }

        public void SetCooldownMultiplier(float multiplier)
        {
            cooldownMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public void SetBasicComboResetMultiplier(float multiplier)
        {
            basicComboResetMultiplier = Mathf.Max(0.1f, multiplier);
        }

        public bool IsChanneling => channelActive;

        public float CurrentChannelMoveSpeedMultiplier => channelActive ? channelMoveSpeedMultiplier : 1f;

        public bool TryExecuteSkill(SkillSlot slot)
        {
            if (!skills.TryGetValue(slot, out SkillDefinition skillDefinition) || skillDefinition == null)
            {
                return false;
            }

            if (skillDefinition.behaviour == null)
            {
                return false;
            }

            if (IsChannelSkill(slot))
            {
                BeginChannel(slot);
                return true;
            }

            float cooldownRemaining = GetCooldownRemaining(slot);
            if (cooldownRemaining > 0f)
            {
                return false;
            }

            cooldownEndTimes[slot] = Time.time + GetCooldownDuration(slot, skillDefinition);
            ExecuteSkill(skillDefinition, slot);
            return true;
        }

        public float GetCooldownRemaining(SkillSlot slot)
        {
            if (!cooldownEndTimes.TryGetValue(slot, out float endTime))
            {
                return 0f;
            }

            return Mathf.Max(0f, endTime - Time.time);
        }

        public float GetCooldownDuration(SkillSlot slot)
        {
            if (!skills.TryGetValue(slot, out SkillDefinition skillDefinition) || skillDefinition == null)
            {
                return 0f;
            }

            return GetCooldownDuration(slot, skillDefinition);
        }

        public bool IsChannelSkill(SkillSlot slot)
        {
            return skills.TryGetValue(slot, out SkillDefinition skillDefinition)
                && skillDefinition != null
                && skillDefinition.behaviour is ChannelSkillBehaviour;
        }

        public void BeginChannel(SkillSlot slot)
        {
            if (channelActive)
            {
                return;
            }

            if (!skills.TryGetValue(slot, out SkillDefinition skillDefinition) || skillDefinition == null)
            {
                return;
            }

            if (!(skillDefinition.behaviour is ChannelSkillBehaviour behaviour))
            {
                return;
            }

            float cooldownRemaining = GetCooldownRemaining(slot);
            if (cooldownRemaining > 0f)
            {
                return;
            }

            channelActive = true;
            channelSlot = slot;
            channelBehaviour = behaviour;
            channelMoveSpeedMultiplier = GetUpgradeState(slot).ChannelMoveSpeedMultiplier;
            channelRoutine = StartCoroutine(ChannelRoutine(skillDefinition, behaviour, slot));
        }

        public void EndChannel(SkillSlot slot)
        {
            if (!channelActive || channelSlot != slot)
            {
                return;
            }

            if (channelRoutine != null)
            {
                StopCoroutine(channelRoutine);
                channelRoutine = null;
            }

            if (skills.TryGetValue(slot, out SkillDefinition skillDefinition) && skillDefinition != null && channelBehaviour != null)
            {
                SkillContext context = BuildContext(skillDefinition, slot);
                float damageMultiplier = SkillCombatUtility.GetDamageMultiplier(context, slot);
                float critChance = SkillCombatUtility.GetCritChance(context);
                float critMultiplier = SkillCombatUtility.GetCritMultiplier(context);
                channelBehaviour.ExecuteRelease(context, damageMultiplier, critChance, critMultiplier);
                cooldownEndTimes[slot] = Time.time + GetCooldownDuration(slot, skillDefinition);
            }

            channelActive = false;
            channelBehaviour = null;
        }

        public void ReduceCooldown(SkillSlot slot, float seconds)
        {
            if (!cooldownEndTimes.TryGetValue(slot, out float endTime))
            {
                return;
            }

            cooldownEndTimes[slot] = Mathf.Max(Time.time, endTime - seconds);
        }

        public void StartGroundDot(
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier,
            float range,
            float duration,
            float tickInterval)
        {
            StartCoroutine(GroundDotRoutine(
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier,
                range,
                duration,
                tickInterval));
        }

        internal void ExecuteBasicCombo(BasicComboSkillBehaviour behaviour, SkillContext context)
        {
            float resetSeconds = behaviour.comboResetSeconds > 0f ? behaviour.comboResetSeconds : 0.9f;
            resetSeconds *= basicComboResetMultiplier;
            resetSeconds *= context.UpgradeState.ComboResetMultiplier;
            if (Time.time - lastBasicTime > resetSeconds)
            {
                basicComboStep = 0;
            }

            basicComboStep = (basicComboStep % 3) + 1;
            lastBasicTime = Time.time;

            float comboMultiplier = 1f;
            GameObject prefabToSpawn = behaviour.swingPrefab;
            if (basicComboStep == 3)
            {
                comboMultiplier = behaviour.finisherMultiplier > 0f ? behaviour.finisherMultiplier : 1.5f;
                if (behaviour.finisherPrefab != null)
                {
                    prefabToSpawn = behaviour.finisherPrefab;
                }
            }

            float damageMultiplier = SkillCombatUtility.GetDamageMultiplier(context, context.Slot) * comboMultiplier;
            float critChance = SkillCombatUtility.GetCritChance(context);
            float critMultiplier = SkillCombatUtility.GetCritMultiplier(context);

            if (basicComboStep == 3 && context.UpgradeState.FinisherFixedDamage)
            {
                damageMultiplier = comboMultiplier;
                critChance = 0f;
            }

            if (!TryExecuteWeaponSwing(behaviour, context, damageMultiplier, critChance, critMultiplier))
            {
                SkillCombatUtility.SpawnDamagePrefab(
                    prefabToSpawn,
                    context,
                    behaviour.baseDamage,
                    damageMultiplier,
                    critChance,
                    critMultiplier);
            }
        }

        internal void ExecuteDash(DashSkillBehaviour behaviour, SkillContext context)
        {
            StartCoroutine(DashRoutine(behaviour));

            float damageMultiplier = SkillCombatUtility.GetDamageMultiplier(context, context.Slot);
            float critChance = SkillCombatUtility.GetCritChance(context);
            float critMultiplier = SkillCombatUtility.GetCritMultiplier(context);

            SkillCombatUtility.SpawnDamagePrefab(
                behaviour.prefab,
                context,
                behaviour.baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier);
        }

        private void ExecuteSkill(SkillDefinition skillDefinition, SkillSlot slot)
        {
            SkillContext context = BuildContext(skillDefinition, slot);
            SkillBehaviour behaviour = skillDefinition.behaviour;

            if (behaviour is BasicComboSkillBehaviour basicCombo)
            {
                basicCombo.Execute(context);
            }
            else if (behaviour is DashSkillBehaviour dash)
            {
                dash.Execute(context);
            }
            else if (behaviour is ConeSkillBehaviour cone)
            {
                cone.Execute(context);
            }
            else
            {
                return;
            }

            if (logSkillUsage)
            {
                Debug.Log($"[Skill] {slot} executed: {skillDefinition.displayName}");
            }
        }

        private IEnumerator ChannelRoutine(SkillDefinition skillDefinition, ChannelSkillBehaviour behaviour, SkillSlot slot)
        {
            SkillUpgradeState upgrades = GetUpgradeState(slot);
            float tickInterval = behaviour.tickInterval > 0f ? behaviour.tickInterval : 0.2f;
            float maxDuration = behaviour.maxDuration > 0f ? behaviour.maxDuration : 1.5f;
            tickInterval *= upgrades.TickIntervalMultiplier;
            maxDuration += upgrades.DurationBonusSeconds;
            float endTime = Time.time + maxDuration;

            SkillContext context = BuildContext(skillDefinition, slot);
            float damageMultiplier = SkillCombatUtility.GetDamageMultiplier(context, slot);
            float critChance = SkillCombatUtility.GetCritChance(context);
            float critMultiplier = SkillCombatUtility.GetCritMultiplier(context);

            while (Time.time < endTime && channelActive)
            {
                behaviour.ExecuteTick(context, damageMultiplier, critChance, critMultiplier);
                yield return new WaitForSeconds(tickInterval);
            }

            if (channelActive)
            {
                EndChannel(slot);
            }
        }

        private IEnumerator DashRoutine(DashSkillBehaviour behaviour)
        {
            float distance = behaviour.dashDistance > 0f ? behaviour.dashDistance : 4f;
            float duration = behaviour.dashDuration > 0f ? behaviour.dashDuration : 0.15f;
            float elapsed = 0f;
            Vector3 direction = spawnOrigin.forward;
            float reductionMultiplier = 1f;
            if (skills.TryGetValue(SkillSlot.Q, out SkillDefinition skillDefinition)
                && skillDefinition != null)
            {
                reductionMultiplier = GetUpgradeState(SkillSlot.Q).DashDamageReductionMultiplier;
            }

            if (damageReductionReceiver != null && reductionMultiplier < 1f)
            {
                damageReductionReceiver.SetDamageReductionMultiplier(reductionMultiplier);
            }

            while (elapsed < duration)
            {
                float step = (distance / duration) * Time.deltaTime;
                if (characterController != null)
                {
                    characterController.Move(direction * step);
                }
                else
                {
                    transform.position += direction * step;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (damageReductionReceiver != null && reductionMultiplier < 1f)
            {
                damageReductionReceiver.SetDamageReductionMultiplier(1f);
            }

            if (behaviour.pullRadius > 0f)
            {
                SkillCombatUtility.ApplyPull(transform.position, behaviour.pullRadius, behaviour.pullStrength);
            }
        }

        private SkillContext BuildContext(SkillDefinition skillDefinition, SkillSlot slot)
        {
            return new SkillContext
            {
                Definition = skillDefinition,
                Slot = slot,
                SkillLevel = GetSkillLevel(slot),
                UpgradeState = GetUpgradeState(slot),
                Origin = spawnOrigin,
                Owner = gameObject,
                Stats = playerStats,
                CharacterController = characterController,
                Executor = this,
                SourceId = gameObject.GetInstanceID()
            };
        }

        private bool TryExecuteWeaponSwing(
            BasicComboSkillBehaviour behaviour,
            SkillContext context,
            float damageMultiplier,
            float critChance,
            float critMultiplier)
        {
            WeaponHitbox hitbox = GetWeaponHitbox();
            if (hitbox == null)
            {
                return false;
            }

            float duration = basicComboStep == 3 ? behaviour.finisherHitboxDuration : behaviour.swingHitboxDuration;
            float sizeBonus = Mathf.Max(context.UpgradeState.RangeBonusPercent, context.UpgradeState.PrefabScaleBonusPercent);
            hitbox.BeginSwing(
                duration,
                behaviour.baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier,
                context.SourceId,
                context.Slot,
                1f + sizeBonus);
            return true;
        }

        private WeaponHitbox GetWeaponHitbox()
        {
            if (weaponHitbox != null)
            {
                return weaponHitbox;
            }

            Transform origin = spawnOrigin != null ? spawnOrigin : transform;
            GameObject hitboxObject = new GameObject("WeaponHitbox");
            hitboxObject.transform.SetParent(origin, false);
            BoxCollider collider = hitboxObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.2f, 1f, 1.2f);
            weaponHitbox = hitboxObject.AddComponent<WeaponHitbox>();
            weaponHitbox.ConfigureOwner(gameObject);
            return weaponHitbox;
        }

        private void SetSkill(SkillSlot slot, SkillDefinition skillDefinition)
        {
            skills[slot] = skillDefinition;
            cooldownEndTimes[slot] = 0f;
            if (skillDefinition != null)
            {
                upgradeTracks[slot] = skillDefinition.upgradeTrack;
            }
            else
            {
                upgradeTracks[slot] = null;
            }

            if (!skillLevels.ContainsKey(slot))
            {
                skillLevels[slot] = 0;
            }

            UpdateUpgradeState(slot);
        }

        private void UpdateUpgradeState(SkillSlot slot)
        {
            upgradeTracks.TryGetValue(slot, out SkillUpgradeTrackBase track);
            int level = GetSkillLevel(slot);
            SkillUpgradeState state = track != null ? track.Evaluate(level) : SkillUpgradeState.CreateDefault();
            upgradeStates[slot] = state;
        }

        private float GetCooldownDuration(SkillSlot slot, SkillDefinition skillDefinition)
        {
            float baseCooldown = skillDefinition != null ? skillDefinition.cooldown : 0f;
            SkillUpgradeState upgrades = GetUpgradeState(slot);
            float finalCooldown = Mathf.Max(0f, baseCooldown + upgrades.CooldownDelta);
            return finalCooldown * cooldownMultiplier;
        }

        private IEnumerator GroundDotRoutine(
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier,
            float range,
            float duration,
            float tickInterval)
        {
            Transform dotOrigin = new GameObject("GroundDotOrigin").transform;
            dotOrigin.position = context.Origin.position;
            dotOrigin.rotation = context.Origin.rotation;

            SkillContext dotContext = context;
            dotContext.Origin = dotOrigin;

            float interval = tickInterval > 0f ? tickInterval : 0.5f;
            float endTime = Time.time + duration;
            while (Time.time < endTime)
            {
                SkillCombatUtility.ExecuteConeDamage(
                    dotContext,
                    baseDamage,
                    damageMultiplier,
                    critChance,
                    critMultiplier,
                    range,
                    360f,
                    1f,
                    false,
                    ~0);
                yield return new WaitForSeconds(interval);
            }

            if (dotOrigin != null)
            {
                Destroy(dotOrigin.gameObject);
            }
        }
    }
}
