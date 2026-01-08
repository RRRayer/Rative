using ProjectS.Core.Combat;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class Damageable : MonoBehaviour, ICombatant
    {
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float xpReward = 5f;
        [SerializeField] private bool destroyOnDeath = true;

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

            Health = Mathf.Max(0f, Health - info.Amount);
            currentHealth = Health;
            DamageEvents.Raise(info);

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
