using System.Collections.Generic;
using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Gameplay.Skills
{
    public sealed class PlayerSkillExecutor : MonoBehaviour, ISkillExecutor
    {
        [SerializeField] private Transform spawnOrigin;
        [SerializeField] private bool logSkillUsage = true;

        private readonly Dictionary<SkillSlot, SkillDefinition> skills = new Dictionary<SkillSlot, SkillDefinition>();
        private readonly Dictionary<SkillSlot, float> cooldownEndTimes = new Dictionary<SkillSlot, float>();

        private void Awake()
        {
            if (spawnOrigin == null)
            {
                spawnOrigin = transform;
            }
        }

        public void SetSkills(SkillDefinition skillQ, SkillDefinition skillE, SkillDefinition skillR)
        {
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

        private void SetSkill(SkillSlot slot, SkillDefinition skillDefinition)
        {
            skills[slot] = skillDefinition;
            cooldownEndTimes[slot] = 0f;
        }

        private void ExecuteSkill(SkillDefinition skillDefinition, SkillSlot slot)
        {
            if (skillDefinition.prefab != null)
            {
                Instantiate(skillDefinition.prefab, spawnOrigin.position, spawnOrigin.rotation);
            }

            if (logSkillUsage)
            {
                Debug.Log($"[Skill] {slot} executed: {skillDefinition.displayName}");
            }
        }
    }
}
