using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Classes
{
    public static class WarriorUpgradeLibrary
    {
        private static SkillUpgradeTrack basicTrack;
        private static SkillUpgradeTrack qTrack;
        private static SkillUpgradeTrack eTrack;
        private static SkillUpgradeTrack rTrack;

        public static bool IsWarrior(ClassDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            return definition.id == "class_default" || definition.displayName == "Warrior";
        }

        public static SkillUpgradeTrack GetTrack(SkillSlot slot)
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

        private static SkillUpgradeTrack CreateBasicTrack()
        {
            SkillUpgradeTrack track = ScriptableObject.CreateInstance<SkillUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new SkillUpgradeStep { level = 1, damageBonusPercent = 0.10f },
                new SkillUpgradeStep { level = 3, prefabScaleBonusPercent = 0.30f },
                new SkillUpgradeStep { level = 4, damageBonusPercent = 0.20f }
            };
            return track;
        }

        private static SkillUpgradeTrack CreateQTrack()
        {
            SkillUpgradeTrack track = ScriptableObject.CreateInstance<SkillUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new SkillUpgradeStep { level = 1, cooldownDelta = -1f },
                new SkillUpgradeStep { level = 2, damageBonusPercent = 0.15f },
                new SkillUpgradeStep { level = 4, cooldownDelta = -2f },
                new SkillUpgradeStep { level = 5, resetCooldownOnKill = true }
            };
            return track;
        }

        private static SkillUpgradeTrack CreateETrack()
        {
            SkillUpgradeTrack track = ScriptableObject.CreateInstance<SkillUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new SkillUpgradeStep { level = 1, durationBonusSeconds = 1f },
                new SkillUpgradeStep { level = 2, cooldownDelta = -2f },
                new SkillUpgradeStep { level = 3, channelMoveSpeedMultiplier = 0.7f },
                new SkillUpgradeStep { level = 4, damageBonusPercent = 0.25f },
                new SkillUpgradeStep
                {
                    level = 5,
                    prefabScaleBonusPercent = 0.50f,
                    pullRadiusBonus = 3f,
                    pullStrengthBonus = 3f
                }
            };
            return track;
        }

        private static SkillUpgradeTrack CreateRTrack()
        {
            SkillUpgradeTrack track = ScriptableObject.CreateInstance<SkillUpgradeTrack>();
            track.hideFlags = HideFlags.DontSave;
            track.steps = new[]
            {
                new SkillUpgradeStep { level = 1, damageBonusPercent = 0.10f, forceAirborneCrit = true },
                new SkillUpgradeStep
                {
                    level = 3,
                    groundDotDuration = 3f,
                    groundDotTickInterval = 0.5f,
                    groundDotDamageMultiplier = 0.35f
                },
                new SkillUpgradeStep { level = 4, damageBonusPercent = 0.30f },
                new SkillUpgradeStep
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
