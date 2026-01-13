using ProjectS.Core.Skills;

namespace ProjectS.Core.Combat
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
