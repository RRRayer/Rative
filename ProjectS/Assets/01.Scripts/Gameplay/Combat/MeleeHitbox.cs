using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class MeleeHitbox : MonoBehaviour, IDamageConfigurable
    {
        [SerializeField] private float forwardOffset = 1.2f;
        [SerializeField] private float lifetime = 0.15f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float critChance;
        [SerializeField] private float critMultiplier = 1.5f;
        [SerializeField] private int sourceId;
        [SerializeField] private SkillSlot slot = SkillSlot.Basic;
        [SerializeField] private bool destroyOnHit = true;
        [SerializeField] private LayerMask hitLayers = ~0;

        private float expiryTime;

        private void Awake()
        {
            transform.position += transform.forward * forwardOffset;
            expiryTime = Time.time + lifetime;
        }

        private void Update()
        {
            if (Time.time >= expiryTime)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            if (other.TryGetComponent<ICombatant>(out ICombatant combatant))
            {
                float finalDamage = damage;
                if (critChance > 0f && Random.value < critChance)
                {
                    finalDamage *= critMultiplier;
                }

                DamageInfo info = new DamageInfo
                {
                    Amount = finalDamage,
                    Point = other.ClosestPoint(transform.position),
                    Direction = transform.forward,
                    SourceId = sourceId,
                    Slot = slot
                };
                combatant.ApplyDamage(info);

                if (destroyOnHit)
                {
                    Destroy(gameObject);
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
