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
        public float DashDamageReductionMultiplier;
        public bool ForceAirborneCrit;
        public bool EnableDoubleHit;
        public float SecondaryHitMultiplier;
        public bool ResetCooldownOnKill;
        public bool FinisherFixedDamage;
        public float GroundDotDuration;
        public float GroundDotTickInterval;
        public float GroundDotDamageMultiplier;

        public static SkillUpgradeState CreateDefault()
        {
            return new SkillUpgradeState
            {
                TickIntervalMultiplier = 1f,
                ComboResetMultiplier = 1f,
                ChannelMoveSpeedMultiplier = 0f,
                DashDamageReductionMultiplier = 1f,
                SecondaryHitMultiplier = 0.5f
            };
        }

        public void ApplyDamageBonus(float bonus)
        {
            DamageBonusPercent += bonus;
        }
    }
}
