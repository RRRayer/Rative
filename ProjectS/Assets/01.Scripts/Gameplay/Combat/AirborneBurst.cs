using System.Collections.Generic;
using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class AirborneBurst : MonoBehaviour, IDamageConfigurable
    {
        [SerializeField] private float radius = 2f;
        [SerializeField] private float airborneDuration = 1f;
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField] private float upwardImpulse = 6f;
        [SerializeField] private LayerMask hitLayers = ~0;

        private float damage;
        private float critChance;
        private float critMultiplier = 1.5f;
        private int sourceId;
        private SkillSlot slot = SkillSlot.E;
        private bool applied;
        private readonly HashSet<int> hitTargets = new HashSet<int>();

        private void Start()
        {
            ApplyBurst();
            if (lifetime > 0f)
            {
                Destroy(gameObject, lifetime);
            }
        }

        private void ApplyBurst()
        {
            if (applied)
            {
                return;
            }

            applied = true;
            hitTargets.Clear();

            float effectiveRadius = radius * transform.lossyScale.x;
            Collider[] hits = Physics.OverlapSphere(transform.position, effectiveRadius, hitLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                ICombatant combatant = hit.GetComponent<ICombatant>() ?? hit.GetComponentInParent<ICombatant>();
                if (combatant == null)
                {
                    continue;
                }

                Component combatantComponent = combatant as Component;
                if (combatantComponent == null)
                {
                    continue;
                }

                int instanceId = combatantComponent.gameObject.GetInstanceID();
                if (!hitTargets.Add(instanceId))
                {
                    continue;
                }

                float finalDamage = damage;
                if (critChance > 0f && Random.value < critChance)
                {
                    finalDamage *= critMultiplier;
                }

                DamageInfo info = new DamageInfo
                {
                    Amount = finalDamage,
                    Point = hit.ClosestPoint(transform.position),
                    Direction = (hit.transform.position - transform.position).normalized,
                    SourceId = sourceId,
                    Slot = slot
                };
                combatant.ApplyDamage(info);

                AirborneStatus airborne = combatantComponent.GetComponent<AirborneStatus>();
                if (airborne == null)
                {
                    airborne = combatantComponent.gameObject.AddComponent<AirborneStatus>();
                }

                airborne.Apply(airborneDuration);

                Rigidbody body = combatantComponent.GetComponent<Rigidbody>()
                    ?? combatantComponent.GetComponentInParent<Rigidbody>();
                if (body != null && !body.isKinematic)
                {
                    Vector3 velocity = body.linearVelocity;
                    velocity.y = 0f;
                    body.linearVelocity = velocity;
                    body.AddForce(Vector3.up * upwardImpulse, ForceMode.VelocityChange);
                }
            }
        }

        public void ConfigureDamage(
            float baseDamage,
            float multiplier,
            float critChance,
            float critMultiplier,
            int sourceId,
            SkillSlot slot)
        {
            damage = baseDamage * multiplier;
            this.critChance = critChance;
            this.critMultiplier = critMultiplier;
            this.sourceId = sourceId;
            this.slot = slot;
        }
    }
}
