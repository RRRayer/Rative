using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Gameplay.Skills.Behaviours
{
    [CreateAssetMenu(menuName = "ProjectS/Skills/Cone")]
    public class ConeSkillBehaviour : SkillBehaviour
    {
        public float baseDamage = 45f;
        public float range = 6f;
        public float angle = 120f;
        public float airborneMultiplier = 1.5f;

        public void Execute(SkillContext context)
        {
            float damageMultiplier = SkillCombatUtility.GetDamageMultiplier(context, context.Slot);
            float critChance = SkillCombatUtility.GetCritChance(context);
            float critMultiplier = SkillCombatUtility.GetCritMultiplier(context);
            float rangeMultiplier = 1f + context.UpgradeState.RangeBonusPercent;
            float finalRange = range * rangeMultiplier;

            SkillCombatUtility.ExecuteConeDamage(
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier,
                finalRange,
                angle,
                airborneMultiplier,
                context.UpgradeState.ForceAirborneCrit);

            if (context.UpgradeState.EnableDoubleHit)
            {
                SkillCombatUtility.ExecuteConeDamage(
                    context,
                    baseDamage * context.UpgradeState.SecondaryHitMultiplier,
                    damageMultiplier,
                    critChance,
                    critMultiplier,
                    finalRange,
                    angle,
                    airborneMultiplier,
                    context.UpgradeState.ForceAirborneCrit);
            }

            if (context.Executor != null && context.UpgradeState.GroundDotDuration > 0f)
            {
                context.Executor.StartGroundDot(
                    context,
                    baseDamage * context.UpgradeState.GroundDotDamageMultiplier,
                    damageMultiplier,
                    critChance,
                    critMultiplier,
                    finalRange,
                    context.UpgradeState.GroundDotDuration,
                    context.UpgradeState.GroundDotTickInterval);
            }
        }
    }
}
