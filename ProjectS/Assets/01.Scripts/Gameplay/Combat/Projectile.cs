using ProjectS.Core.Combat;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 4f;
        [SerializeField] private float damage = 15f;
        [SerializeField] private LayerMask hitLayers = ~0;

        private float expiryTime;

        private void Awake()
        {
            expiryTime = Time.time + lifetime;
        }

        private void Update()
        {
            transform.position += transform.forward * (speed * Time.deltaTime);

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
            }

            Destroy(gameObject);
        }
    }
}
