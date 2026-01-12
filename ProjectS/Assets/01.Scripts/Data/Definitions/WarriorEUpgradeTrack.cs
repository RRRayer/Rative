using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct WarriorEUpgradeStep
    {
        [Range(1, 5)] public int level;
        public float durationBonusSeconds;
        public float cooldownDelta;
        public float channelMoveSpeedMultiplier;
        [Range(0f, 2f)] public float damageBonusPercent;
        [Range(0f, 2f)] public float prefabScaleBonusPercent;
        public float pullRadiusBonus;
        public float pullStrengthBonus;
    }

    [CreateAssetMenu(menuName = "ProjectS/Definitions/Upgrade Tracks/Warrior/E")]
    public class WarriorEUpgradeTrack : SkillUpgradeTrackBase
    {
        public WarriorEUpgradeStep[] steps;

        public bool TryGetStep(int level, out WarriorEUpgradeStep step)
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

                state.DurationBonusSeconds += steps[i].durationBonusSeconds;
                state.CooldownDelta += steps[i].cooldownDelta;
                state.DamageBonusPercent += steps[i].damageBonusPercent;
                state.PrefabScaleBonusPercent += steps[i].prefabScaleBonusPercent;
                state.PullRadiusBonus += steps[i].pullRadiusBonus;
                state.PullStrengthBonus += steps[i].pullStrengthBonus;
                if (steps[i].channelMoveSpeedMultiplier > 0f)
                {
                    state.ChannelMoveSpeedMultiplier = steps[i].channelMoveSpeedMultiplier;
                }
            }

            return state;
        }
    }
}
