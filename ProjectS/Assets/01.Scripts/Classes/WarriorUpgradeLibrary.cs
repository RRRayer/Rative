using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Classes
{
    public static class WarriorUpgradeLibrary
    {
        private static WarriorBasicUpgradeTrack basicTrack;
        private static WarriorQUpgradeTrack qTrack;
        private static WarriorEUpgradeTrack eTrack;
        private static WarriorRUpgradeTrack rTrack;

        public static bool IsWarrior(ClassDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            return definition.id == "class_default" || definition.displayName == "Warrior";
        }

        public static SkillUpgradeTrackBase GetTrack(SkillSlot slot)
        {
            switch (slot)
            {
                case SkillSlot.Basic:
                    return basicTrack ?? (basicTrack = CreateBasicTrack());
                case SkillSlot.Q:
                    return qTrack ?? (qTrack = CreateQTrack());
                case SkillSlot.E:
                    return eTrack ?? (eTrack = CreateETrack());
                case SkillSlot.R:
                    return rTrack ?? (rTrack = CreateRTrack());
                default:
                    return null;
            }
        }

        private static WarriorBasicUpgradeTrack CreateBasicTrack()
        {
            WarriorBasicUpgradeTrack track = ScriptableObject.CreateInstance<WarriorBasicUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new WarriorBasicUpgradeStep { level = 1, damageBonusPercent = 0.10f },
                new WarriorBasicUpgradeStep { level = 2, comboResetMultiplier = 0.8f },
                new WarriorBasicUpgradeStep { level = 3, prefabScaleBonusPercent = 0.30f },
                new WarriorBasicUpgradeStep { level = 4, damageBonusPercent = 0.20f },
                new WarriorBasicUpgradeStep { level = 5, finisherFixedDamage = true }
            };
            return track;
        }

        private static WarriorQUpgradeTrack CreateQTrack()
        {
            WarriorQUpgradeTrack track = ScriptableObject.CreateInstance<WarriorQUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new WarriorQUpgradeStep { level = 1, cooldownDelta = -1f },
                new WarriorQUpgradeStep { level = 2, damageBonusPercent = 0.15f },
                new WarriorQUpgradeStep { level = 3, dashDamageReductionMultiplier = 0.5f },
                new WarriorQUpgradeStep { level = 4, cooldownDelta = -2f },
                new WarriorQUpgradeStep { level = 5, resetCooldownOnKill = true }
            };
            return track;
        }

        private static WarriorEUpgradeTrack CreateETrack()
        {
            WarriorEUpgradeTrack track = ScriptableObject.CreateInstance<WarriorEUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new WarriorEUpgradeStep { level = 1, durationBonusSeconds = 1f },
                new WarriorEUpgradeStep { level = 2, cooldownDelta = -2f },
                new WarriorEUpgradeStep { level = 3, channelMoveSpeedMultiplier = 0.7f },
                new WarriorEUpgradeStep { level = 4, damageBonusPercent = 0.25f },
                new WarriorEUpgradeStep
                {
                    level = 5,
                    prefabScaleBonusPercent = 0.50f,
                    pullRadiusBonus = 3f,
                    pullStrengthBonus = 3f
                }
            };
            return track;
        }

        private static WarriorRUpgradeTrack CreateRTrack()
        {
            WarriorRUpgradeTrack track = ScriptableObject.CreateInstance<WarriorRUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new WarriorRUpgradeStep { level = 1, damageBonusPercent = 0.10f, forceAirborneCrit = true },
                new WarriorRUpgradeStep { level = 2, rangeBonusPercent = 0.15f },
                new WarriorRUpgradeStep
                {
                    level = 3,
                    groundDotDuration = 3f,
                    groundDotTickInterval = 0.5f,
                    groundDotDamageMultiplier = 0.35f
                },
                new WarriorRUpgradeStep { level = 4, damageBonusPercent = 0.30f },
                new WarriorRUpgradeStep
                {
                    level = 5,
                    enableDoubleHit = true,
                    secondaryHitMultiplier = 0.5f
                }
            };
            return track;
        }
    }
}
