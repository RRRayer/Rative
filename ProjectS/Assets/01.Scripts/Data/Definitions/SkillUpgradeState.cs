namespace ProjectS.Data.Definitions
{
    public struct SkillUpgradeState
    {
        public float DamageBonusPercent;
        public float CooldownDelta;
        public float RangeBonusPercent;
        public float DurationBonusSeconds;
        public float TickIntervalMultiplier;
        public float PullRadiusBonus;
        public float PullStrengthBonus;
        public float PrefabScaleBonusPercent;
        public float ComboResetMultiplier;
        public float ChannelMoveSpeedMultiplier;
        public bool ForceAirborneCrit;
        public bool EnableDoubleHit;
        public float SecondaryHitMultiplier;
        public bool ResetCooldownOnKill;
        public float GroundDotDuration;
        public float GroundDotTickInterval;
        public float GroundDotDamageMultiplier;

        public static SkillUpgradeState CreateDefault()
        {
            return new SkillUpgradeState
            {
                TickIntervalMultiplier = 1f,
                ComboResetMultiplier = 1f,
                ChannelMoveSpeedMultiplier = 1f,
                SecondaryHitMultiplier = 0.5f
            };
        }

        public void Apply(SkillUpgradeStep step)
        {
            DamageBonusPercent += step.damageBonusPercent;
            CooldownDelta += step.cooldownDelta;
            RangeBonusPercent += step.rangeBonusPercent;
            DurationBonusSeconds += step.durationBonusSeconds;
            PullRadiusBonus += step.pullRadiusBonus;
            PullStrengthBonus += step.pullStrengthBonus;
            PrefabScaleBonusPercent += step.prefabScaleBonusPercent;
            ForceAirborneCrit |= step.forceAirborneCrit;
            EnableDoubleHit |= step.enableDoubleHit;
            ResetCooldownOnKill |= step.resetCooldownOnKill;

            if (step.tickIntervalMultiplier > 0f)
            {
                TickIntervalMultiplier *= step.tickIntervalMultiplier;
            }

            if (step.comboResetMultiplier > 0f)
            {
                ComboResetMultiplier *= step.comboResetMultiplier;
            }

            if (step.channelMoveSpeedMultiplier > 0f)
            {
                ChannelMoveSpeedMultiplier = step.channelMoveSpeedMultiplier;
            }

            if (step.secondaryHitMultiplier > 0f)
            {
                SecondaryHitMultiplier = step.secondaryHitMultiplier;
            }

            if (step.groundDotDuration > 0f)
            {
                GroundDotDuration = step.groundDotDuration;
            }

            if (step.groundDotTickInterval > 0f)
            {
                GroundDotTickInterval = step.groundDotTickInterval;
            }

            if (step.groundDotDamageMultiplier > 0f)
            {
                GroundDotDamageMultiplier = step.groundDotDamageMultiplier;
            }
        }
    }
}
