using System.Collections;
using ProjectS.Data.Definitions;
using ProjectS.Core.Combat;
using ProjectS.Gameplay.Combat;
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
        public GameObject effectPrefab;
        public float effectLifetime = 0.6f;
        public float effectScaleMultiplier = 1f;
        public bool effectScaleWithRange = true;
        public bool useProjectileDamage;
        public float secondaryProjectileDelay = 0.08f;

        public void Execute(SkillContext context)
        {
            float damageMultiplier = SkillCombatUtility.GetDamageMultiplier(context, context.Slot);
            float critChance = SkillCombatUtility.GetCritChance(context);
            float critMultiplier = SkillCombatUtility.GetCritMultiplier(context);
            float rangeMultiplier = 1f + context.UpgradeState.RangeBonusPercent;
            float finalRange = range * rangeMultiplier;

            if (effectPrefab != null)
            {
                if (useProjectileDamage)
                {
                    SpawnProjectile(
                        context,
                        baseDamage,
                        damageMultiplier,
                        critChance,
                        critMultiplier,
                        finalRange);

                    if (context.UpgradeState.EnableDoubleHit)
                    {
                        if (context.Executor != null)
                        {
                            context.Executor.StartCoroutine(DelayedProjectile(
                                context,
                                baseDamage * context.UpgradeState.SecondaryHitMultiplier,
                                damageMultiplier,
                                critChance,
                                critMultiplier,
                                finalRange));
                        }
                        else
                        {
                            SpawnProjectile(
                                context,
                                baseDamage * context.UpgradeState.SecondaryHitMultiplier,
                                damageMultiplier,
                                critChance,
                                critMultiplier,
                                finalRange);
                        }
                    }

                    return;
                }

                GameObject effect = Object.Instantiate(effectPrefab, context.Origin.position, context.Origin.rotation);
                float scale = effectScaleMultiplier;
                if (effectScaleWithRange)
                {
                    scale *= finalRange / Mathf.Max(0.1f, range);
                }
                if (scale > 0.01f && scale != 1f)
                {
                    effect.transform.localScale *= scale;
                }
                if (effectLifetime > 0f)
                {
                    Object.Destroy(effect, effectLifetime);
                }
            }

            SkillCombatUtility.ExecuteConeDamage(
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier,
                finalRange,
                angle,
                airborneMultiplier,
                context.UpgradeState.ForceAirborneCrit,
                ~0);

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
                    context.UpgradeState.ForceAirborneCrit,
                    ~0);
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

        private void ConfigureDamage(
            GameObject spawned,
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier)
        {
            IDamageConfigurable[] damageTargets = spawned.GetComponentsInChildren<IDamageConfigurable>();
            for (int i = 0; i < damageTargets.Length; i++)
            {
                damageTargets[i].ConfigureDamage(
                    baseDamage,
                    damageMultiplier,
                    critChance,
                critMultiplier,
                context.SourceId,
                context.Slot);
            }
        }

        private void SpawnProjectile(
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier,
            float finalRange)
        {
            GameObject effect = Object.Instantiate(effectPrefab, context.Origin.position, context.Origin.rotation);
            float scale = effectScaleMultiplier;
            if (effectScaleWithRange)
            {
                scale *= finalRange / Mathf.Max(0.1f, range);
            }
            if (scale > 0.01f && scale != 1f)
            {
                effect.transform.localScale *= scale;
            }

            ConfigureDamage(effect, context, baseDamage, damageMultiplier, critChance, critMultiplier);
            TempestEdgeProjectile projectile = effect.GetComponent<TempestEdgeProjectile>();
            if (projectile != null)
            {
                projectile.SetAirborneMultiplier(airborneMultiplier);
            }
        }

        private IEnumerator DelayedProjectile(
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier,
            float finalRange)
        {
            float delay = Mathf.Max(0f, secondaryProjectileDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            SpawnProjectile(
                context,
                baseDamage,
                damageMultiplier,
                critChance,
                critMultiplier,
                finalRange);
        }
    }
}
