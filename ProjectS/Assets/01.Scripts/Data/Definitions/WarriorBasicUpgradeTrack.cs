using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct WarriorBasicUpgradeStep
    {
        [Range(1, 5)] public int level;
        [Range(0f, 2f)] public float damageBonusPercent;
        public float comboResetMultiplier;
        [Range(0f, 2f)] public float prefabScaleBonusPercent;
        public bool finisherFixedDamage;
    }

    [CreateAssetMenu(menuName = "ProjectS/Definitions/Upgrade Tracks/Warrior/Basic")]
    public class WarriorBasicUpgradeTrack : SkillUpgradeTrackBase
    {
        public WarriorBasicUpgradeStep[] steps;

        public bool TryGetStep(int level, out WarriorBasicUpgradeStep step)
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

                state.DamageBonusPercent += steps[i].damageBonusPercent;
                state.PrefabScaleBonusPercent += steps[i].prefabScaleBonusPercent;
                if (steps[i].comboResetMultiplier > 0f)
                {
                    state.ComboResetMultiplier *= steps[i].comboResetMultiplier;
                }
                if (steps[i].finisherFixedDamage)
                {
                    state.FinisherFixedDamage = true;
                }
            }

            return state;
        }
    }
}
