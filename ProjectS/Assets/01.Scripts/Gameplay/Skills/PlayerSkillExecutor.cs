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
        private PlayerStats playerStats;
        private CharacterController characterController;
        private int basicComboStep;
        private float lastBasicTime;
        private Coroutine channelRoutine;
        private SkillSlot channelSlot;
        private ChannelSkillBehaviour channelBehaviour;
        private bool channelActive;

        private void Awake()
        {
            if (spawnOrigin == null)
            {
                spawnOrigin = transform;
            }

            playerStats = GetComponent<PlayerStats>();
            characterController = GetComponent<CharacterController>();
        }

        public void SetSkills(SkillDefinition basic, SkillDefinition skillQ, SkillDefinition skillE, SkillDefinition skillR)
        {
            SetSkill(SkillSlot.Basic, basic);
            SetSkill(SkillSlot.Q, skillQ);
            SetSkill(SkillSlot.E, skillE);
            SetSkill(SkillSlot.R, skillR);
        }

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

            cooldownEndTimes[slot] = Time.time + skillDefinition.cooldown;
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
                cooldownEndTimes[slot] = Time.time + skillDefinition.cooldown;
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

        internal void ExecuteBasicCombo(BasicComboSkillBehaviour behaviour, SkillContext context)
        {
            float resetSeconds = behaviour.comboResetSeconds > 0f ? behaviour.comboResetSeconds : 0.9f;
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

            SkillCombatUtility.SpawnDamagePrefab(
                prefabToSpawn,
                context,
                behaviour.baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier);
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
            float tickInterval = behaviour.tickInterval > 0f ? behaviour.tickInterval : 0.2f;
            float maxDuration = behaviour.maxDuration > 0f ? behaviour.maxDuration : 1.5f;
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
                Origin = spawnOrigin,
                Owner = gameObject,
                Stats = playerStats,
                CharacterController = characterController,
                Executor = this,
                SourceId = gameObject.GetInstanceID()
            };
        }

        private void SetSkill(SkillSlot slot, SkillDefinition skillDefinition)
        {
            skills[slot] = skillDefinition;
            cooldownEndTimes[slot] = 0f;
        }
    }
}
