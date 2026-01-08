using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Gameplay.Skills.Behaviours
{
    [CreateAssetMenu(menuName = "ProjectS/Skills/Channel")]
    public class ChannelSkillBehaviour : SkillBehaviour
    {
        public float baseDamage = 6f;
        public GameObject tickPrefab;
        public GameObject releasePrefab;
        public float tickInterval = 0.2f;
        public float maxDuration = 2.5f;

        public void ExecuteTick(SkillContext context, float damageMultiplier, float critChance, float critMultiplier)
        {
            SkillCombatUtility.SpawnDamagePrefab(
                tickPrefab,
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier);

            if (context.UpgradeState.PullRadiusBonus > 0f && context.UpgradeState.PullStrengthBonus > 0f)
            {
                SkillCombatUtility.ApplyPull(
                    context.Origin.position,
                    context.UpgradeState.PullRadiusBonus,
                    context.UpgradeState.PullStrengthBonus);
            }
        }

        public void ExecuteRelease(SkillContext context, float damageMultiplier, float critChance, float critMultiplier)
        {
            SkillCombatUtility.SpawnDamagePrefab(
                releasePrefab,
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier);
        }
    }
}
