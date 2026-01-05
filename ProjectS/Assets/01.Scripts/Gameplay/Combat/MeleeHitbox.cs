using ProjectS.Core.Combat;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class MeleeHitbox : MonoBehaviour
    {
        [SerializeField] private float forwardOffset = 1.2f;
        [SerializeField] private float lifetime = 0.15f;
        [SerializeField] private float damage = 10f;
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
                DamageInfo info = new DamageInfo
                {
                    Amount = damage,
                    Point = other.ClosestPoint(transform.position),
                    Direction = transform.forward
                };
                combatant.ApplyDamage(info);

                if (destroyOnHit)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
