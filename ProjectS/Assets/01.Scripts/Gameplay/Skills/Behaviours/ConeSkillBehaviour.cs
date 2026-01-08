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

            SkillCombatUtility.ExecuteConeDamage(
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier,
                range,
                angle,
                airborneMultiplier);
        }
    }
}
