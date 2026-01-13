using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class MeleeAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackRadius = 0.6f;
        [SerializeField] private float attackDamage = 12f;
        [SerializeField] private int sourceIdOverride;
        [SerializeField] private float attackCooldown = 0.4f;
        [SerializeField] private LayerMask hitLayers = ~0;

        private float nextAttackTime;

        public bool TryAttack()
        {
            if (Time.time < nextAttackTime)
            {
                return false;
            }

            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }

            nextAttackTime = Time.time + attackCooldown;

            Vector3 center = attackOrigin.position + attackOrigin.forward * attackRange;
            Collider[] hits = Physics.OverlapSphere(center, attackRadius, hitLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit.TryGetComponent<ICombatant>(out ICombatant combatant))
                {
                    int sourceId = sourceIdOverride != 0 ? sourceIdOverride : gameObject.GetInstanceID();
                    DamageInfo info = new DamageInfo
                    {
                        Amount = attackDamage,
                        Point = hit.ClosestPoint(center),
                        Direction = attackOrigin.forward,
                        SourceId = sourceId,
                        Slot = SkillSlot.Basic
                    };
                    combatant.ApplyDamage(info);
                }
            }

            return hits.Length > 0;
        }
    }
}
