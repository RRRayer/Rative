using System.Collections.Generic;
using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    [RequireComponent(typeof(Collider))]
    public class WeaponHitbox : MonoBehaviour, IDamageConfigurable
    {
        [SerializeField] private LayerMask hitLayers = ~0;
        [SerializeField] private float baseSizeMultiplier = 1f;

        private Collider hitboxCollider;
        private float activeUntil;
        private float damage;
        private float critChance;
        private float critMultiplier = 1.5f;
        private int sourceId;
        private SkillSlot slot = SkillSlot.Basic;
        private Transform ownerRoot;
        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private Vector3 initialScale;

        private void Awake()
        {
            hitboxCollider = GetComponent<Collider>();
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false;
            initialScale = transform.localScale;
        }

        private void Update()
        {
            if (activeUntil > 0f && Time.time >= activeUntil)
            {
                EndSwing();
            }
        }

        public void ConfigureOwner(GameObject owner)
        {
            ownerRoot = owner != null ? owner.transform.root : null;
        }

        public void BeginSwing(
            float duration,
            float baseDamage,
            float multiplier,
            float critChance,
            float critMultiplier,
            int sourceId,
            SkillSlot slot,
            float sizeMultiplier)
        {
            if (hitboxCollider == null)
            {
                hitboxCollider = GetComponent<Collider>();
            }

            hitTargets.Clear();
            activeUntil = Time.time + Mathf.Max(0.05f, duration);
            damage = baseDamage * multiplier;
            this.critChance = critChance;
            this.critMultiplier = critMultiplier;
            this.sourceId = sourceId;
            this.slot = slot;
            hitboxCollider.enabled = true;

            float finalMultiplier = Mathf.Max(0.1f, baseSizeMultiplier * sizeMultiplier);
            transform.localScale = initialScale * finalMultiplier;
        }

        private void EndSwing()
        {
            activeUntil = 0f;
            hitboxCollider.enabled = false;
            transform.localScale = initialScale;
            hitTargets.Clear();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!hitboxCollider.enabled)
            {
                return;
            }

            if ((hitLayers.value & (1 << other.gameObject.layer)) == 0)
            {
                return;
            }

            if (ownerRoot != null && other.transform.root == ownerRoot)
            {
                return;
            }

            int instanceId = other.GetInstanceID();
            if (!hitTargets.Add(instanceId))
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
