using Photon.Pun;
using ProjectS.Core.Combat;
using ProjectS.Core.Skills;
using ProjectS.Gameplay.Stats;
using ProjectS.Networking;
using UnityEngine;

namespace ProjectS.Gameplay.Combat
{
    public class Damageable : MonoBehaviour, IDeathSyncTarget
    {
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float currentHealth;
        [SerializeField] private float xpReward = 5f;
        [SerializeField] private bool destroyOnDeath = true;
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

            if (!IsAlive)
            {
                if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
                {
                    EnemyResultSyncManager.Instance?.ReportLocalDeath(transform.position, xpReward, isElite, isBoss);
                    if (destroyOnDeath)
                    {
                        Destroy(gameObject);
                    }
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

                if (destroyOnDeath)
                {
                    Destroy(gameObject);
                }
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

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
