using ProjectS.Core.Combat;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class Damageable : MonoBehaviour, ICombatant
    {
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private bool destroyOnDeath = true;

        public float Health { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsAlive => Health > 0f;
        public Vector3 Position => transform.position;

        private void Awake()
        {
            Health = maxHealth;
        }

        public void ApplyDamage(DamageInfo info)
        {
            if (!IsAlive)
            {
                return;
            }

            Health = Mathf.Max(0f, Health - info.Amount);

            if (!IsAlive && destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
