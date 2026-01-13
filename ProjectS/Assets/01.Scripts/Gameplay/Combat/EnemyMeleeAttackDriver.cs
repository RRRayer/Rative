using ProjectS.Core.Combat;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class EnemyMeleeAttackDriver : MonoBehaviour
    {
        [SerializeField] private MeleeAttack attack;
        [SerializeField] private float detectionRange = 2.5f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private float turnSpeed = 360f;

        private float nextCheckTime;
        private ICombatant currentTarget;
        private Rigidbody body;

        private void Awake()
        {
            if (attack == null)
            {
                attack = GetComponent<MeleeAttack>();
            }

            body = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (attack == null)
            {
                return;
            }

            if (Time.time >= nextCheckTime)
            {
                nextCheckTime = Time.time + checkInterval;
                currentTarget = FindClosestTarget();
            }

            if (currentTarget == null)
            {
                return;
            }

            Vector3 direction = currentTarget.Position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
                Quaternion nextRotation = Quaternion.RotateTowards(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
                if (body != null)
                {
                    body.MoveRotation(nextRotation);
                }
                else
                {
                    transform.rotation = nextRotation;
                }
            }

            Vector3 moveDirection = direction;
            moveDirection.y = 0f;
            if (moveDirection.sqrMagnitude > 0.01f)
            {
                Vector3 nextPosition = transform.position + moveDirection.normalized * moveSpeed * Time.deltaTime;
                if (body != null)
                {
                    body.MovePosition(nextPosition);
                }
                else
                {
                    transform.position = nextPosition;
                }
            }

            attack.TryAttack();
        }

        private ICombatant FindClosestTarget()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange, targetLayers, QueryTriggerInteraction.Ignore);
            ICombatant closest = null;
            float closestDistance = float.MaxValue;

            for (int i = 0; i < hits.Length; i++)
            {
                ICombatant combatant = hits[i].GetComponent<ICombatant>();
                if (combatant == null)
                {
                    combatant = hits[i].GetComponentInParent<ICombatant>();
                }

                if (combatant == null)
                {
                    continue;
                }

                if (combatant is Component component && component.gameObject == gameObject)
                {
                    continue;
                }

                float distance = Vector3.Distance(transform.position, combatant.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = combatant;
                }
            }

            return closest;
        }
    }
}
