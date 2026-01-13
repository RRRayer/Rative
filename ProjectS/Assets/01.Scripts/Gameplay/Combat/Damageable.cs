using ProjectS.Core.Combat;
using ProjectS.Gameplay.Stats;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class Damageable : MonoBehaviour, ICombatant
    {
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float xpReward = 5f;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField, Range(0f, 0.5f)] private float damageReductionPercent;

        public float Health { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsAlive => Health > 0f;
        public Vector3 Position => transform.position;
        public float XpReward => xpReward;

        private void Awake()
        {
            if (currentHealth <= 0f || currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }

            Health = currentHealth;
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive)
            {
                return;
            }

            float reduction = damageReductionPercent;
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                reduction = stats.DamageReductionPercent;
            }

            float finalAmount = info.Amount * (1f - Mathf.Clamp(reduction, 0f, 0.5f));
            Health = Mathf.Max(0f, Health - finalAmount);
            currentHealth = Health;
            DamageInfo resolvedInfo = info;
            resolvedInfo.Amount = finalAmount;
            DamageEvents.Raise(resolvedInfo);

            if (!IsAlive)
            {
                KillEvents.Raise(new KillInfo
                {
                    SourceId = info.SourceId,
                    Slot = info.Slot,
                    XpReward = xpReward,
                    Target = gameObject
                });

                if (destroyOnDeath)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
