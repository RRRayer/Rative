using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct WarriorQUpgradeStep
    {
        [Range(1, 5)] public int level;
        public float cooldownDelta;
        [Range(0f, 2f)] public float damageBonusPercent;
        public float dashDamageReductionMultiplier;
        public bool resetCooldownOnKill;
    }

    [CreateAssetMenu(menuName = "ProjectS/Definitions/Upgrade Tracks/Warrior/Q")]
    public class WarriorQUpgradeTrack : SkillUpgradeTrackBase
    {
        public WarriorQUpgradeStep[] steps;

        public bool TryGetStep(int level, out WarriorQUpgradeStep step)
        {
            if (steps != null)
            {
                for (int i = 0; i < steps.Length; i++)
                {
                    if (steps[i].level == level)
                    {
                        step = steps[i];
                        return true;
                    }
                }
            }

            step = default;
            return false;
        }

        public override SkillUpgradeState Evaluate(int level)
        {
            SkillUpgradeState state = SkillUpgradeState.CreateDefault();
            if (steps == null || steps.Length == 0)
            {
                return state;
            }

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].level > level)
                {
                    continue;
                }

                state.CooldownDelta += steps[i].cooldownDelta;
                state.DamageBonusPercent += steps[i].damageBonusPercent;
                if (steps[i].dashDamageReductionMultiplier > 0f)
                {
                    state.DashDamageReductionMultiplier = steps[i].dashDamageReductionMultiplier;
                }
                if (steps[i].resetCooldownOnKill)
                {
                    state.ResetCooldownOnKill = true;
                }
            }

            return state;
        }
    }
}
