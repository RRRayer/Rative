using Photon.Pun;
using PS.Core.Combat;
using UnityEngine;

namespace PS.Gameplay.Combat
{
    public class EnemyMeleeAttackDriver : MonoBehaviour
    {
        [SerializeField] private MeleeAttack attack;
        [SerializeField] private float detectionRange = 2.5f;
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private LayerMask targetLayers = ~0;
        [SerializeField] private float checkInterval = 0.1f;
        [SerializeField] private float turnSpeed = 360f;
        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private float moveThreshold = 0.05f;

        private float nextCheckTime;
        private ICombatant currentTarget;
        private Rigidbody body;
        private PhotonView photonView;

        private static readonly int IsWalkHash = Animator.StringToHash("isWalk");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int DieHash = Animator.StringToHash("Die");

        private void Awake()
        {
            if (attack == null)
            {
                attack = GetComponent<MeleeAttack>();
            }

            body = GetComponent<Rigidbody>();
            photonView = GetComponent<PhotonView>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }
        }

        private void Update()
        {
            if (PhotonNetwork.InRoom && photonView != null && !photonView.IsMine)
            {
                return;
            }

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
                SetMovementAnimation(Vector3.zero);
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
            SetMovementAnimation(moveDirection);
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

            if (attack.TryAttack())
            {
                TriggerAttackAnimation();
            }
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
        private void SetMovementAnimation(Vector3 moveDirection)
        {
            if (animator == null)
            {
                return;
            }

            bool moving = moveDirection.sqrMagnitude > (moveThreshold * moveThreshold);
            animator.SetBool(IsWalkHash, moving);
        }

        private void TriggerAttackAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(AttackHash);
        }

        public void TriggerHitAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(HitHash);
        }

        public void TriggerDieAnimation()
        {
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(DieHash);
        }

    }
}
