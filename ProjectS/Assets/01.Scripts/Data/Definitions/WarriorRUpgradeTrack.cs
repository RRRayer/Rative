using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct WarriorRUpgradeStep
    {
        [Range(1, 5)] public int level;
        [Range(0f, 2f)] public float damageBonusPercent;
        [Range(0f, 2f)] public float rangeBonusPercent;
        public bool forceAirborneCrit;
        public bool enableDoubleHit;
        public float secondaryHitMultiplier;
        public float groundDotDuration;
        public float groundDotTickInterval;
        public float groundDotDamageMultiplier;
    }

    [CreateAssetMenu(menuName = "ProjectS/Definitions/Upgrade Tracks/Warrior/R")]
    public class WarriorRUpgradeTrack : SkillUpgradeTrackBase
    {
        public WarriorRUpgradeStep[] steps;

        public bool TryGetStep(int level, out WarriorRUpgradeStep step)
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
                state.RangeBonusPercent += steps[i].rangeBonusPercent;
                if (steps[i].forceAirborneCrit)
                {
                    state.ForceAirborneCrit = true;
                }
                if (steps[i].enableDoubleHit)
                {
                    state.EnableDoubleHit = true;
                }
                if (steps[i].secondaryHitMultiplier > 0f)
                {
                    state.SecondaryHitMultiplier = steps[i].secondaryHitMultiplier;
                }
                if (steps[i].groundDotDuration > 0f)
                {
                    state.GroundDotDuration = steps[i].groundDotDuration;
                }
                if (steps[i].groundDotTickInterval > 0f)
                {
                    state.GroundDotTickInterval = steps[i].groundDotTickInterval;
                }
                if (steps[i].groundDotDamageMultiplier > 0f)
                {
                    state.GroundDotDamageMultiplier = steps[i].groundDotDamageMultiplier;
                }
            }

            return state;
        }
    }
}
