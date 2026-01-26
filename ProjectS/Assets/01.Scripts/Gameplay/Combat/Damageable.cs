using Photon.Pun;
using PS.Core.Combat;
using PS.Gameplay.Combat;
using PS.Core.Skills;
using PS.Gameplay.Stats;
using PS.Networking;
using UnityEngine;

namespace PS.Gameplay.Combat
{
    public class Damageable : MonoBehaviour, IDeathSyncTarget, ICombatant
    {
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float xpReward = 5f;
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float deathDespawnDelay = 1f;
        [SerializeField] private bool isElite;
        [SerializeField] private bool isBoss;
        [SerializeField, Range(0f, 0.5f)] private float damageReductionPercent;

        public bool IsBoss => isBoss;

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

            if (info.SourceId == gameObject.GetInstanceID())
            {
                return;
            }

            float reduction = damageReductionPercent;
            PlayerStats stats = GetComponent<PlayerStats>();
            if (stats != null)
            {
                reduction = stats.DamageReductionPercent;
            }

            float finalAmount = info.Amount;
            if (TeamUpgradeManager.Instance != null && TeamUpgradeManager.Instance.HasUpgrade(TeamUpgradeType.ExecutionerEye))
            {
                if (isBoss)
                {
                    finalAmount *= 1.5f;
                }
                else if (MaxHealth > 0f && (Health / MaxHealth) <= 0.2f)
                {
                    finalAmount = Health;
                }
            }

            if (finalAmount < Health)
            {
                finalAmount *= 1f - Mathf.Clamp(reduction, 0f, 0.5f);
            }
            Health = Mathf.Max(0f, Health - finalAmount);
            currentHealth = Health;
            DamageInfo resolvedInfo = info;
            resolvedInfo.Amount = finalAmount;
            DamageEvents.Raise(resolvedInfo);

            EnemyMeleeAttackDriver enemyAnim = GetComponent<EnemyMeleeAttackDriver>() ?? GetComponentInParent<EnemyMeleeAttackDriver>();
            if (enemyAnim != null)
            {
                enemyAnim.TriggerHitAnimation();
            }

            if (!IsAlive)
            {
                if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                {
                    EnemyResultSyncManager.Instance?.ReportLocalDeath(transform.position, xpReward, isElite, isBoss);
                    PlayDeathAnimation();
                    DestroyAfterDelay();
                    return;
                }

                KillEvents.Raise(new KillInfo
                {
                    SourceId = info.SourceId,
                    Slot = info.Slot,
                    XpReward = xpReward,
                    Target = gameObject
                });

                if (isElite)
                {
                    TeamUpgradeManager.Instance?.SpawnChest(transform.position);
                }

                EnemyResultSyncManager.Instance?.BroadcastDeath(transform.position);

                PlayDeathAnimation();
                DestroyAfterDelay();
            }
        }

        public void ApplySyncedDeath()
        {
            if (!IsAlive)
            {
                return;
            }

            Health = 0f;
            currentHealth = 0f;

            PlayDeathAnimation();
            DestroyAfterDelay();
        }

        private void PlayDeathAnimation()
        {
            EnemyMeleeAttackDriver enemyAnim = GetComponent<EnemyMeleeAttackDriver>() ?? GetComponentInParent<EnemyMeleeAttackDriver>();
            if (enemyAnim != null)
            {
                enemyAnim.TriggerDieAnimation();
            }
        }

        private void DestroyAfterDelay()
        {
            if (!destroyOnDeath)
            {
                return;
            }

            float delay = Mathf.Max(0f, deathDespawnDelay);
            if (delay > 0f)
            {
                Destroy(gameObject, delay);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
