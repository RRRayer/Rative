using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ProjectS.Core.Combat;
using ProjectS.Progression.Leveling;
using UnityEngine;

namespace ProjectS.Networking
{
    public class SharedProgressionManager : MonoBehaviour, IOnEventCallback
    {
        public static SharedProgressionManager Instance { get; private set; }

        [Header("Shared Progression")]
        [SerializeField] private int level = 1;
        [SerializeField] private float currentXp;
        [SerializeField] private float xpToNext = 120f;
        [SerializeField] private bool useLevelBasedXpReward = true;
        [SerializeField] private float overrideXpReward = 5f;

        [Header("XP Pickup")]
        [SerializeField] private string xpPickupPrefabName = "XpPickup";

        private readonly HashSet<int> collectedPickupViewIds = new HashSet<int>();
        private readonly HashSet<int> selectionReadyActors = new HashSet<int>();
        private int pendingSelections;
        private bool selectionActive;
        public event System.Action SelectionStarted;
        public event System.Action SelectionEnded;
        public event System.Action<int, float, float> ProgressionChanged;

        public int Level => level;
        public float CurrentXp => currentXp;
        public float XpToNext => xpToNext;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<SharedProgressionManager>() == null)
            {
                GameObject manager = new GameObject("SharedProgressionManager");
                manager.AddComponent<SharedProgressionManager>();
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
            xpToNext = GetRequiredXpForLevel(level);
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
            KillEvents.Killed += HandleKilled;
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
            KillEvents.Killed -= HandleKilled;
        }

        public void RequestPickup(int viewId, float xpAmount)
        {
            if (!PhotonNetwork.InRoom)
            {
                int previousLevel = level;
                ApplyXp(xpAmount);
                int levelUps = Mathf.Max(0, level - previousLevel);
                UpdateProgressionManagers();
                if (levelUps > 0)
                {
                    QueueSelections(levelUps);
                    StartLocalSelection();
                }
                return;
            }

            object[] payload = { viewId, xpAmount };
            PhotonNetwork.RaiseEvent(
                ProgressionEventCodes.RequestPickup,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case ProgressionEventCodes.RequestPickup:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        HandlePickupRequest(photonEvent.CustomData as object[]);
                    }
                    break;
                case ProgressionEventCodes.SharedProgressionUpdate:
                    ApplySharedProgression(photonEvent.CustomData as object[]);
                    break;
                case ProgressionEventCodes.SelectionStart:
                    BeginSelection();
                    break;
                case ProgressionEventCodes.SelectionChoice:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        RegisterSelection(photonEvent.CustomData as object[]);
                    }
                    break;
                case ProgressionEventCodes.SelectionEnd:
                    EndSelection();
                    break;
            }
        }

        private void HandlePickupRequest(object[] payload)
        {
            if (payload == null || payload.Length < 2)
            {
                return;
            }

            int viewId = (int)payload[0];
            float amount = (float)payload[1];
            if (!collectedPickupViewIds.Add(viewId))
            {
                return;
            }

            PhotonView view = PhotonView.Find(viewId);
            if (view != null && view.gameObject != null)
            {
                PhotonNetwork.Destroy(view.gameObject);
            }

            int previousLevel = level;
            ApplyXp(amount);
            int levelUps = Mathf.Max(0, level - previousLevel);
            BroadcastProgression();

            if (levelUps > 0)
            {
                QueueSelections(levelUps);
                StartSelection();
            }
        }

        private void HandleKilled(KillInfo info)
        {
            if (info.XpReward <= 0f || info.Target == null)
            {
                return;
            }

            float reward = useLevelBasedXpReward ? GetKillXpForLevel(level) : (overrideXpReward > 0f ? overrideXpReward : info.XpReward);
            if (!PhotonNetwork.InRoom)
            {
                SpawnLocalPickup(info.Target.transform.position, reward);
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            PhotonNetwork.Instantiate(
                xpPickupPrefabName,
                info.Target.transform.position,
                Quaternion.identity,
                0,
                new object[] { reward });
        }

        private void ApplyXp(float amount)
        {
            currentXp += amount;
            while (currentXp >= xpToNext)
            {
                currentXp -= xpToNext;
                level += 1;
                xpToNext = GetRequiredXpForLevel(level);
            }
        }

        private void BroadcastProgression()
        {
            object[] payload = { level, currentXp, xpToNext };
            PhotonNetwork.RaiseEvent(
                ProgressionEventCodes.SharedProgressionUpdate,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        private void ApplySharedProgression(object[] payload)
        {
            if (payload == null || payload.Length < 3)
            {
                return;
            }

            level = (int)payload[0];
            currentXp = (float)payload[1];
            xpToNext = (float)payload[2];
            UpdateProgressionManagers();
        }

        private void StartSelection()
        {
            if (selectionActive)
            {
                return;
            }

            selectionActive = true;
            selectionReadyActors.Clear();
            PhotonNetwork.RaiseEvent(
                ProgressionEventCodes.SelectionStart,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        private void QueueSelections(int count)
        {
            pendingSelections += Mathf.Max(0, count);
        }

        private void StartLocalSelection()
        {
            if (selectionActive)
            {
                return;
            }

            selectionActive = true;
            selectionReadyActors.Clear();
            BeginSelection();
        }

        private void BeginSelection()
        {
            PauseGame(true);
            SelectionStarted?.Invoke();
        }

        private void EndSelection()
        {
            selectionActive = false;
            selectionReadyActors.Clear();
            pendingSelections = Mathf.Max(0, pendingSelections - 1);

            if (pendingSelections > 0)
            {
                SelectionEnded?.Invoke();
                StartSelection();
                return;
            }

            PauseGame(false);
            SelectionEnded?.Invoke();
        }

        public void SubmitSelection()
        {
            if (!PhotonNetwork.InRoom)
            {
                EndSelection();
                return;
            }

            object[] payload = { PhotonNetwork.LocalPlayer.ActorNumber };
            PhotonNetwork.RaiseEvent(
                ProgressionEventCodes.SelectionChoice,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                SendOptions.SendReliable);
        }

        private void RegisterSelection(object[] payload)
        {
            if (payload == null || payload.Length < 1)
            {
                return;
            }

            int actorNumber = (int)payload[0];
            selectionReadyActors.Add(actorNumber);

            if (PhotonNetwork.CurrentRoom == null)
            {
                return;
            }

            Player[] players = PhotonNetwork.PlayerList;
            for (int i = 0; i < players.Length; i++)
            {
                if (!selectionReadyActors.Contains(players[i].ActorNumber))
                {
                    return;
                }
            }

            PhotonNetwork.RaiseEvent(
                ProgressionEventCodes.SelectionEnd,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        private void PauseGame(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void UpdateProgressionManagers()
        {
            ProgressionManager[] managers = FindObjectsOfType<ProgressionManager>();
            for (int i = 0; i < managers.Length; i++)
            {
                managers[i].SetProgress(level, currentXp, xpToNext);
            }

            ProgressionChanged?.Invoke(level, currentXp, xpToNext);
        }

        private void SpawnLocalPickup(Vector3 position, float amount)
        {
            GameObject prefab = Resources.Load<GameObject>(xpPickupPrefabName);
            if (prefab == null)
            {
                return;
            }

            GameObject pickupObject = Instantiate(prefab, position, Quaternion.identity);
            XpPickup pickup = pickupObject.GetComponent<XpPickup>();
            if (pickup != null)
            {
                pickup.SetAmount(amount);
            }
        }

        private static float GetRequiredXpForLevel(int currentLevel)
        {
            int levelValue = Mathf.Max(1, currentLevel);
            float needed = (100f * (levelValue - 1) * (levelValue - 1)) + 20f;
            return Mathf.Ceil(needed);
        }

        private static float GetKillXpForLevel(int currentLevel)
        {
            int levelValue = Mathf.Max(1, currentLevel);
            float reward = (10f * (levelValue - 1) * (levelValue - 1)) + 20f;
            return Mathf.Ceil(reward);
        }
    }
}
