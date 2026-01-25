using PS.Core.Skills;

namespace PS.Core.Combat
{
    public interface IDamageConfigurable
    {
        void ConfigureDamage(
            float baseDamage,
            float multiplier,
            float critChance,
            float critMultiplier,
            int sourceId,
            SkillSlot slot);
    }
}
