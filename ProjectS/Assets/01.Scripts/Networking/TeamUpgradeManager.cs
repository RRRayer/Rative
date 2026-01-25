using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using PS.Core.Skills;
using UnityEngine;

namespace PS.Networking
{
    public class TeamUpgradeManager : MonoBehaviour, IOnEventCallback
    {
        private const string ChestPrefabName = "TeamUpgradeChest";

        public static TeamUpgradeManager Instance { get; private set; }

        [Header("Vote Settings")]
        [SerializeField] private float voteDurationSeconds = 15f;

        private readonly HashSet<TeamUpgradeType> acquiredUpgrades = new HashSet<TeamUpgradeType>();
        private readonly Dictionary<int, TeamUpgradeType?> votesByActor = new Dictionary<int, TeamUpgradeType?>();
        private readonly List<TeamUpgradeType> currentOptions = new List<TeamUpgradeType>();
        private bool voteActive;
        private float voteEndTime;

        public event System.Action<IReadOnlyList<TeamUpgradeOption>, float> VoteStarted;
        public event System.Action<IReadOnlyList<float>> VoteUpdated;
        public event System.Action<TeamUpgradeType> UpgradeGranted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<TeamUpgradeManager>() == null)
            {
                GameObject manager = new GameObject("TeamUpgradeManager");
                manager.AddComponent<TeamUpgradeManager>();
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

        private void Update()
        {
            if (!voteActive)
            {
                return;
            }

            if (Time.unscaledTime >= voteEndTime)
            {
                if (!PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient)
                {
                    ResolveVote();
                }
            }
        }

        public void SpawnChest(Vector3 position)
        {
            if (!PhotonNetwork.InRoom)
            {
                SpawnLocalChest(position);
                return;
            }

            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            PhotonNetwork.Instantiate(ChestPrefabName, position, Quaternion.identity);
        }

        public void RequestChestPickup(int viewId)
        {
            if (!PhotonNetwork.InRoom)
            {
                BeginVote();
                return;
            }

            object[] payload = { viewId };
            PhotonNetwork.RaiseEvent(
                TeamUpgradeEventCodes.ChestPickup,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                SendOptions.SendReliable);
        }

        public void SubmitVote(TeamUpgradeType? type)
        {
            if (!voteActive)
            {
                return;
            }

            if (!PhotonNetwork.InRoom)
            {
                votesByActor[0] = type;
                return;
            }

            object[] payload = { PhotonNetwork.LocalPlayer.ActorNumber, type.HasValue ? (int)type.Value : -1 };
            PhotonNetwork.RaiseEvent(
                TeamUpgradeEventCodes.VoteSubmit,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
                SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            switch (photonEvent.Code)
            {
                case TeamUpgradeEventCodes.ChestPickup:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        HandleChestPickup(photonEvent.CustomData as object[]);
                    }
                    break;
                case TeamUpgradeEventCodes.VoteStart:
                    HandleVoteStart(photonEvent.CustomData as object[]);
                    break;
                case TeamUpgradeEventCodes.VoteSubmit:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        HandleVoteSubmit(photonEvent.CustomData as object[]);
                    }
                    break;
                case TeamUpgradeEventCodes.VoteEnd:
                    if (PhotonNetwork.IsMasterClient)
                    {
                        ResolveVote();
                    }
                    break;
                case TeamUpgradeEventCodes.VoteUpdate:
                    HandleVoteUpdate(photonEvent.CustomData as object[]);
                    break;
                case TeamUpgradeEventCodes.UpgradeGranted:
                    HandleUpgradeGranted(photonEvent.CustomData as object[]);
                    break;
            }
        }

        private void HandleChestPickup(object[] payload)
        {
            if (payload == null || payload.Length < 1)
            {
                return;
            }

            int viewId = (int)payload[0];
            PhotonView view = PhotonView.Find(viewId);
            if (view != null && view.gameObject != null)
            {
                PhotonNetwork.Destroy(view.gameObject);
            }

            BeginVote();
        }

        private void BeginVote()
        {
            if (voteActive)
            {
                return;
            }

            currentOptions.Clear();
            currentOptions.AddRange(BuildOptions());
            if (currentOptions.Count == 0)
            {
                return;
            }

            votesByActor.Clear();
            voteActive = true;
            voteEndTime = Time.unscaledTime + voteDurationSeconds;

            PauseGame(true);

            if (!PhotonNetwork.InRoom)
            {
                HandleVoteStart(new object[] { BuildOptionPayload() });
                BroadcastVoteUpdate();
                return;
            }

            PhotonNetwork.RaiseEvent(
                TeamUpgradeEventCodes.VoteStart,
                new object[] { BuildOptionPayload() },
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
            BroadcastVoteUpdate();
        }

        private void HandleVoteStart(object[] payload)
        {
            if (payload == null || payload.Length < 1)
            {
                return;
            }

            object[] optionPayload = payload[0] as object[];
            currentOptions.Clear();
            if (optionPayload != null)
            {
                for (int i = 0; i < optionPayload.Length; i++)
                {
                    currentOptions.Add((TeamUpgradeType)(int)optionPayload[i]);
                }
            }

            voteActive = true;
            voteEndTime = Time.unscaledTime + voteDurationSeconds;
            PauseGame(true);
            VoteStarted?.Invoke(BuildOptionData(currentOptions), voteDurationSeconds);
        }

        private void HandleVoteSubmit(object[] payload)
        {
            if (payload == null || payload.Length < 2)
            {
                return;
            }

            int actorNumber = (int)payload[0];
            int value = (int)payload[1];
            TeamUpgradeType? selection = value >= 0 ? (TeamUpgradeType?)value : null;
            votesByActor[actorNumber] = selection;
            BroadcastVoteUpdate();

            if (PhotonNetwork.CurrentRoom == null)
            {
                return;
            }

            Player[] players = PhotonNetwork.PlayerList;
            for (int i = 0; i < players.Length; i++)
            {
                if (!votesByActor.ContainsKey(players[i].ActorNumber))
                {
                    return;
                }
            }

            PhotonNetwork.RaiseEvent(
                TeamUpgradeEventCodes.VoteEnd,
                null,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        private void ResolveVote()
        {
            if (!voteActive)
            {
                return;
            }

            TeamUpgradeType chosen = PickWeightedOption();
            acquiredUpgrades.Add(chosen);
            voteActive = false;
            votesByActor.Clear();
            PauseGame(false);

            if (!PhotonNetwork.InRoom)
            {
                HandleUpgradeGranted(new object[] { (int)chosen });
                return;
            }

            PhotonNetwork.RaiseEvent(
                TeamUpgradeEventCodes.UpgradeGranted,
                new object[] { (int)chosen },
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        public bool HasUpgrade(TeamUpgradeType type)
        {
            return acquiredUpgrades.Contains(type);
        }

        private void HandleUpgradeGranted(object[] payload)
        {
            if (payload == null || payload.Length < 1)
            {
                return;
            }

            TeamUpgradeType type = (TeamUpgradeType)(int)payload[0];
            acquiredUpgrades.Add(type);
            voteActive = false;
            votesByActor.Clear();
            PauseGame(false);
            UpgradeGranted?.Invoke(type);
        }

        private TeamUpgradeType PickWeightedOption()
        {
            Dictionary<TeamUpgradeType, int> counts = new Dictionary<TeamUpgradeType, int>();
            for (int i = 0; i < currentOptions.Count; i++)
            {
                counts[currentOptions[i]] = 0;
            }

            foreach (KeyValuePair<int, TeamUpgradeType?> entry in votesByActor)
            {
                if (!entry.Value.HasValue)
                {
                    continue;
                }

                TeamUpgradeType value = entry.Value.Value;
                if (counts.ContainsKey(value))
                {
                    counts[value] += 1;
                }
            }

            int totalVotes = 0;
            foreach (KeyValuePair<TeamUpgradeType, int> entry in counts)
            {
                totalVotes += entry.Value;
            }

            if (totalVotes <= 0)
            {
                return currentOptions[Random.Range(0, currentOptions.Count)];
            }

            int roll = Random.Range(1, totalVotes + 1);
            int cumulative = 0;
            foreach (TeamUpgradeType option in currentOptions)
            {
                cumulative += counts[option];
                if (roll <= cumulative)
                {
                    return option;
                }
            }

            return currentOptions[0];
        }

        private void BroadcastVoteUpdate()
        {
            List<float> ratios = BuildVoteRatios();
            VoteUpdated?.Invoke(ratios);

            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            object[] payload = new object[ratios.Count];
            for (int i = 0; i < ratios.Count; i++)
            {
                payload[i] = ratios[i];
            }

            PhotonNetwork.RaiseEvent(
                TeamUpgradeEventCodes.VoteUpdate,
                payload,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
        }

        private void HandleVoteUpdate(object[] payload)
        {
            if (payload == null)
            {
                return;
            }

            List<float> ratios = new List<float>();
            for (int i = 0; i < payload.Length; i++)
            {
                ratios.Add((float)payload[i]);
            }

            VoteUpdated?.Invoke(ratios);
        }

        private List<float> BuildVoteRatios()
        {
            Dictionary<TeamUpgradeType, int> counts = new Dictionary<TeamUpgradeType, int>();
            for (int i = 0; i < currentOptions.Count; i++)
            {
                counts[currentOptions[i]] = 0;
            }

            foreach (KeyValuePair<int, TeamUpgradeType?> entry in votesByActor)
            {
                if (!entry.Value.HasValue)
                {
                    continue;
                }

                TeamUpgradeType value = entry.Value.Value;
                if (counts.ContainsKey(value))
                {
                    counts[value] += 1;
                }
            }

            int totalVotes = 0;
            foreach (KeyValuePair<TeamUpgradeType, int> entry in counts)
            {
                totalVotes += entry.Value;
            }

            List<float> ratios = new List<float>();
            for (int i = 0; i < currentOptions.Count; i++)
            {
                TeamUpgradeType option = currentOptions[i];
                if (totalVotes <= 0)
                {
                    ratios.Add(0f);
                }
                else
                {
                    ratios.Add((float)counts[option] / totalVotes);
                }
            }

            return ratios;
        }

        private List<TeamUpgradeType> BuildOptions()
        {
            List<TeamUpgradeType> available = new List<TeamUpgradeType>();
            foreach (TeamUpgradeType value in System.Enum.GetValues(typeof(TeamUpgradeType)))
            {
                if (!acquiredUpgrades.Contains(value))
                {
                    available.Add(value);
                }
            }

            if (available.Count == 0)
            {
                return available;
            }

            List<TeamUpgradeType> result = new List<TeamUpgradeType>();
            int pickCount = Mathf.Min(3, available.Count);
            for (int i = 0; i < pickCount; i++)
            {
                int index = Random.Range(0, available.Count);
                TeamUpgradeType chosen = available[index];
                available.RemoveAt(index);
                result.Add(chosen);
            }

            return result;
        }

        private IReadOnlyList<TeamUpgradeOption> BuildOptionData(List<TeamUpgradeType> options)
        {
            List<TeamUpgradeOption> data = new List<TeamUpgradeOption>();
            for (int i = 0; i < options.Count; i++)
            {
                TeamUpgradeType type = options[i];
                data.Add(new TeamUpgradeOption
                {
                    Type = type,
                    Title = GetTitle(type),
                    Description = GetDescription(type)
                });
            }

            return data;
        }

        private object[] BuildOptionPayload()
        {
            object[] payload = new object[currentOptions.Count];
            for (int i = 0; i < currentOptions.Count; i++)
            {
                payload[i] = (int)currentOptions[i];
            }
            return payload;
        }

        private void PauseGame(bool paused)
        {
            Time.timeScale = paused ? 0f : 1f;
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private static string GetTitle(TeamUpgradeType type)
        {
            switch (type)
            {
                case TeamUpgradeType.ExecutionerEye:
                    return "Executioner's Eye";
                case TeamUpgradeType.BloodFrenzy:
                    return "Blood Frenzy";
                case TeamUpgradeType.GuardianAngel:
                    return "Guardian Angel";
                case TeamUpgradeType.VampiricOath:
                    return "Vampiric Oath";
                case TeamUpgradeType.TitaniumSkin:
                    return "Titanium Skin";
                case TeamUpgradeType.GoldenHand:
                    return "Golden Hand";
                case TeamUpgradeType.Adrenaline:
                    return "Adrenaline";
                case TeamUpgradeType.SoulLink:
                    return "Soul Link";
                default:
                    return "Unknown";
            }
        }

        private static string GetDescription(TeamUpgradeType type)
        {
            switch (type)
            {
                case TeamUpgradeType.ExecutionerEye:
                    return "Execute non-boss targets at <= 20% HP\nBoss takes 1.5x damage";
                case TeamUpgradeType.BloodFrenzy:
                    return "On kill: +5% attack for 5s\n(max 10 stacks)";
                case TeamUpgradeType.GuardianAngel:
                    return "Survive lethal hit at 1 HP\n3s invulnerable (once per stage)";
                case TeamUpgradeType.VampiricOath:
                    return "Heal for 5% of damage dealt";
                case TeamUpgradeType.TitaniumSkin:
                    return "Super armor (stagger immune)\nMove speed -10%";
                case TeamUpgradeType.GoldenHand:
                    return "Monster kills grant +50% gold/points";
                case TeamUpgradeType.Adrenaline:
                    return "Ultimate reduces allies' skill cooldowns by 5s";
                case TeamUpgradeType.SoulLink:
                    return "Incoming damage is shared across the party";
                default:
                    return string.Empty;
            }
        }

        private void SpawnLocalChest(Vector3 position)
        {
            GameObject prefab = Resources.Load<GameObject>(ChestPrefabName);
            if (prefab == null)
            {
                return;
            }

            Instantiate(prefab, position, Quaternion.identity);
        }
    }
}
