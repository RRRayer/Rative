using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using ProjectS.Gameplay.Combat;
using UnityEngine;

namespace ProjectS.Gameplay.Skills
{
    public static class SkillCombatUtility
    {
        public static float GetDamageMultiplier(SkillContext context, SkillSlot slot)
        {
            if (context.Stats == null)
            {
                return 1f + context.UpgradeState.DamageBonusPercent;
            }

            float baseMultiplier = context.Stats.GetDamageMultiplier(slot);
            return baseMultiplier * (1f + context.UpgradeState.DamageBonusPercent);
        }

        public static float GetCritChance(SkillContext context)
        {
            return context.Stats != null ? context.Stats.CritChance : 0f;
        }

        public static float GetCritMultiplier(SkillContext context)
        {
            return context.Stats != null ? context.Stats.CritMultiplier : 1.5f;
        }

        public static void SpawnDamagePrefab(
            GameObject prefab,
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier)
        {
            if (prefab == null)
            {
                return;
            }

            GameObject spawned = Object.Instantiate(prefab, context.Origin.position, context.Origin.rotation);
            if (spawned == null || baseDamage <= 0f)
            {
                return;
            }

            float scaleMultiplier = 1f + context.UpgradeState.PrefabScaleBonusPercent;
            if (scaleMultiplier > 0.01f && scaleMultiplier != 1f)
            {
                spawned.transform.localScale *= scaleMultiplier;
            }

            int sourceId = context.SourceId;
            IDamageConfigurable[] damageTargets = spawned.GetComponentsInChildren<IDamageConfigurable>();
            for (int i = 0; i < damageTargets.Length; i++)
            {
                damageTargets[i].ConfigureDamage(
                    baseDamage,
                    damageMultiplier,
                    critChance,
                    critMultiplier,
                    sourceId,
                    context.Slot);
            }
        }

        public static void ExecuteConeDamage(
            SkillContext context,
            float baseDamage,
            float damageMultiplier,
            float critChance,
            float critMultiplier,
            float range,
            float angle,
            float airborneMultiplier,
            bool forceAirborneCrit)
        {
            Collider[] hits = Physics.OverlapSphere(context.Origin.position, range, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                Vector3 direction = (hit.transform.position - context.Origin.position);
                if (Vector3.Angle(context.Origin.forward, direction) > angle * 0.5f)
                {
                    continue;
                }

                if (hit.TryGetComponent<ICombatant>(out ICombatant combatant))
                {
                    float finalDamage = baseDamage * damageMultiplier;
                    AirborneStatus airborne = hit.GetComponent<AirborneStatus>();
                    if (airborne != null && airborne.IsAirborne)
                    {
                        finalDamage *= forceAirborneCrit ? critMultiplier : airborneMultiplier;
                    }
                    else if (critChance > 0f && Random.value < critChance)
                    {
                        finalDamage *= critMultiplier;
                    }

                    DamageInfo info = new DamageInfo
                    {
                        Amount = finalDamage,
                        Point = hit.ClosestPoint(context.Origin.position),
                        Direction = context.Origin.forward,
                        SourceId = context.SourceId,
                        Slot = context.Slot
                    };
                    combatant.ApplyDamage(info);
                }
            }
        }

        public static void ApplyPull(Vector3 origin, float radius, float strength)
        {
            Collider[] hits = Physics.OverlapSphere(origin, radius, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Rigidbody body = hits[i].attachedRigidbody;
                if (body == null)
                {
                    continue;
                }

                Vector3 direction = (origin - body.position);
                body.AddForce(direction.normalized * strength, ForceMode.VelocityChange);
            }
        }
    }
}
