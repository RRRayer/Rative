using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class TempestEdgeProjectile : MonoBehaviour, IDamageConfigurable
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 0.5f;
        [SerializeField] private float airborneMultiplier = 1.5f;
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField] private bool destroyOnHit = true;

        private float damage;
        private float critChance;
        private float critMultiplier = 1.5f;
        private int sourceId;
        private SkillSlot slot = SkillSlot.R;
        private float destroyAt;

        private void OnEnable()
        {
            destroyAt = Time.time + Mathf.Max(0.05f, lifetime);
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
            if (Time.time >= destroyAt)
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

            ICombatant combatant = other.GetComponent<ICombatant>() ?? other.GetComponentInParent<ICombatant>();
            if (combatant == null)
            {
                return;
            }

            float finalDamage = damage;
            AirborneStatus airborne = other.GetComponent<AirborneStatus>() ?? other.GetComponentInParent<AirborneStatus>();
            if (airborne != null && airborne.IsAirborne)
            {
                finalDamage *= airborneMultiplier;
            }
            else if (critChance > 0f && Random.value < critChance)
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

        public void SetAirborneMultiplier(float value)
        {
            airborneMultiplier = value;
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
