using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ProjectS.Core.Combat;
using UnityEngine;

namespace ProjectS.Networking
{
    public class EnemyResultSyncManager : MonoBehaviour, IOnEventCallback
    {
        public static EnemyResultSyncManager Instance { get; private set; }

        [Header("Result Sync")]
        [SerializeField] private float deathMatchRadius = 2.5f;
        [SerializeField] private string xpPickupPrefabName = "XpPickup";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<EnemyResultSyncManager>() == null)
            {
                GameObject manager = new GameObject("EnemyResultSyncManager");
                manager.AddComponent<EnemyResultSyncManager>();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        public void ReportLocalDeath(Vector3 position, float xpReward, bool isElite, bool isBoss)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            object[] payload =
            {
                position.x, position.y, position.z,
                xpReward,
                isElite ? 1 : 0,
                isBoss ? 1 : 0
            };

            PhotonNetwork.RaiseEvent(
                EnemyResultEventCodes.EnemyDeathReport,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                SendOptions.SendReliable);
        }

        public void BroadcastDeath(Vector3 position)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            object[] payload = { position.x, position.y, position.z };
            PhotonNetwork.RaiseEvent(
                EnemyResultEventCodes.EnemyDeathBroadcast,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case EnemyResultEventCodes.EnemyDeathReport:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        HandleDeathReport(photonEvent.CustomData as object[]);
                    }
                    break;
                case EnemyResultEventCodes.EnemyDeathBroadcast:
                    HandleDeathBroadcast(photonEvent.CustomData as object[]);
                    break;
            }
        }

        private void HandleDeathReport(object[] payload)
        {
            if (payload == null || payload.Length < 6)
            {
                return;
            }

            Vector3 position = new Vector3((float)payload[0], (float)payload[1], (float)payload[2]);
            float xpReward = (float)payload[3];
            bool isElite = (int)payload[4] == 1;
            bool isBoss = (int)payload[5] == 1;

            if (xpReward > 0f)
            {
                PhotonNetwork.Instantiate(
                    xpPickupPrefabName,
                    position,
                    Quaternion.identity,
                    0,
                    new object[] { xpReward });
            }

            if (isElite)
            {
                TeamUpgradeManager.Instance?.SpawnChest(position);
            }

            BroadcastDeath(position);
        }

        private void HandleDeathBroadcast(object[] payload)
        {
            if (payload == null || payload.Length < 3)
            {
                return;
            }

            Vector3 position = new Vector3((float)payload[0], (float)payload[1], (float)payload[2]);
            IDeathSyncTarget target = FindClosestTarget(position, deathMatchRadius);
            if (target == null)
            {
                return;
            }

            target.ApplySyncedDeath();
        }

        private IDeathSyncTarget FindClosestTarget(Vector3 position, float radius)
        {
            MonoBehaviour[] candidates = FindObjectsOfType<MonoBehaviour>();
            IDeathSyncTarget closest = null;
            float closestDistance = float.MaxValue;
            float maxDistance = Mathf.Max(0.1f, radius);

            for (int i = 0; i < candidates.Length; i++)
            {
                IDeathSyncTarget candidate = candidates[i] as IDeathSyncTarget;
                if (candidate == null || !candidate.IsAlive)
                {
                    continue;
                }

                float distance = Vector3.Distance(candidate.Position, position);
                if (distance <= maxDistance && distance < closestDistance)
                {
                    closest = candidate;
                    closestDistance = distance;
                }
            }

            return closest;
        }
    }
}
