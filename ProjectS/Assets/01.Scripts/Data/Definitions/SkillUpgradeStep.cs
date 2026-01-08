using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct SkillUpgradeStep
    {
        [Range(1, 5)] public int level;
        [Range(0f, 2f)] public float damageBonusPercent;
        public float cooldownDelta;
        [Range(0f, 2f)] public float rangeBonusPercent;
        public float durationBonusSeconds;
        public float tickIntervalMultiplier;
        public float pullRadiusBonus;
        public float pullStrengthBonus;
        [Range(0f, 2f)] public float prefabScaleBonusPercent;
        public float comboResetMultiplier;
        public float channelMoveSpeedMultiplier;
        public bool forceAirborneCrit;
        public bool enableDoubleHit;
        public float secondaryHitMultiplier;
        public bool resetCooldownOnKill;
        public float groundDotDuration;
        public float groundDotTickInterval;
        public float groundDotDamageMultiplier;
    }
}
