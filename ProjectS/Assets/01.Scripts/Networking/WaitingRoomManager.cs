using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectS.Networking
{
    /// <summary>
    /// Waiting room controller: Photon sync + view coordination (UI lives in ProjectS.UI).
    /// </summary>
    public class WaitingRoomManager : MonoBehaviourPunCallbacks
    {
        private static readonly int[] DefaultClassIds = { 0, 1, 2, 3 };

        [Header("View")]
        [SerializeField] private MonoBehaviour waitingRoomViewBehaviour;
        [Header("Scenes")]
        [SerializeField] private string lobbySceneName = "Lobby";
        [SerializeField] private string inGameSceneName = "InGame";

        private IWaitingRoomView waitingRoomView;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            BindView();
        }

        private void OnDestroy()
        {
            UnbindView();
        }

        private void Start()
        {
            RefreshPlayerSlots();
        }

        public override void OnJoinedRoom()
        {
            PhotonNetwork.EnableCloseConnection = true;
            InitializeLocalCustomProperties();
            RefreshPlayerSlots();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            RefreshPlayerSlots();
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            RefreshPlayerSlots();
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (changedProps.ContainsKey(NetworkPropertyKeys.Player.Ready) ||
                changedProps.ContainsKey(NetworkPropertyKeys.Player.ClassId))
            {
                RefreshPlayerSlots();
            }
        }

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            RefreshPlayerSlots();
        }

        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(lobbySceneName);
        }

        private void HandleReadyClicked()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                return;
            }

            bool currentReady = IsPlayerReady(PhotonNetwork.LocalPlayer);
            Hashtable properties = new Hashtable
            {
                { NetworkPropertyKeys.Player.Ready, !currentReady }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            RefreshPlayerSlots();
        }

        private void HandleStartClicked()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (!CanStartGame())
            {
                Debug.LogWarning("[WaitingRoom] Cannot start: not all clients are ready.");
                return;
            }

            PhotonNetwork.LoadLevel(inGameSceneName);
        }

        private void HandleLeaveClicked()
        {
            if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
            {
                PhotonNetwork.CurrentRoom.IsOpen = false;
                PhotonNetwork.CurrentRoom.IsVisible = false;
            }

            PhotonNetwork.LeaveRoom();
        }

        private void HandleKickClicked(int actorNumber)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (!PhotonNetwork.EnableCloseConnection)
            {
                Debug.LogWarning("[WaitingRoom] Kick requested but CloseConnection is disabled.");
                return;
            }

            Player target = PhotonNetwork.PlayerList.FirstOrDefault(p => p.ActorNumber == actorNumber);
            if (target != null)
            {
                PhotonNetwork.CloseConnection(target);
            }
        }

        private void HandleClassSelected(int classId)
        {
            if (!PhotonNetwork.InRoom)
            {
                return;
            }

            if (!IsClassAvailable(classId))
            {
                Debug.LogWarning($"[WaitingRoom] Class {classId} is already taken.");
                return;
            }

            Hashtable properties = new Hashtable
            {
                { NetworkPropertyKeys.Player.ClassId, classId }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
            RefreshPlayerSlots();
        }

        private void RefreshPlayerSlots()
        {
            if (waitingRoomView == null)
            {
                return;
            }

            List<Player> orderedPlayers = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToList();
            IEnumerable<WaitingRoomPlayerViewModel> viewModels = orderedPlayers.Select(p =>
            {
                bool isHost = PhotonNetwork.MasterClient != null && p.ActorNumber == PhotonNetwork.MasterClient.ActorNumber;
                bool canKick = PhotonNetwork.IsMasterClient && !isHost;
                return new WaitingRoomPlayerViewModel
                {
                    ActorNumber = p.ActorNumber,
                    Nickname = p.NickName,
                    IsReady = IsPlayerReady(p),
                    IsHost = isHost,
                    CanKick = canKick,
                    IsLocal = PhotonNetwork.LocalPlayer != null && p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber,
                    ClassId = GetPlayerClassId(p)
                };
            });

            waitingRoomView.RenderSlots(viewModels);

            bool isHostLocal = PhotonNetwork.IsMasterClient;
            bool canStart = isHostLocal && CanStartGame();
            bool localReady = IsPlayerReady(PhotonNetwork.LocalPlayer);
            waitingRoomView.UpdateControls(isHostLocal, canStart, localReady);
            waitingRoomView.UpdateClassSelection(GetTakenClassIds(), GetPlayerClassId(PhotonNetwork.LocalPlayer));
        }

        private bool CanStartGame()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

            if (HasDuplicateClassIds())
            {
                return false;
            }

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == PhotonNetwork.MasterClient.ActorNumber)
                {
                    continue;
                }

                if (!IsPlayerReady(player))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPlayerReady(Player player)
        {
            if (player?.CustomProperties == null)
            {
                return false;
            }

            if (player.CustomProperties.TryGetValue(NetworkPropertyKeys.Player.Ready, out object value) &&
                value is bool isReady)
            {
                return isReady;
            }

            return false;
        }

        private static int GetPlayerClassId(Player player)
        {
            if (player?.CustomProperties == null)
            {
                return 0;
            }

            if (player.CustomProperties.TryGetValue(NetworkPropertyKeys.Player.ClassId, out object value))
            {
                if (value is int intValue)
                {
                    return intValue;
                }

                if (value is byte byteValue)
                {
                    return byteValue;
                }
            }

            return 0;
        }

        private IReadOnlyCollection<int> GetTakenClassIds()
        {
            HashSet<int> taken = new HashSet<int>();
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                taken.Add(GetPlayerClassId(player));
            }

            return taken;
        }

        private IReadOnlyCollection<int> GetTakenClassIdsExcludingLocal()
        {
            HashSet<int> taken = new HashSet<int>();
            int localActorNumber = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == localActorNumber)
                {
                    continue;
                }

                taken.Add(GetPlayerClassId(player));
            }

            return taken;
        }

        private bool IsClassAvailable(int classId)
        {
            int localActorNumber = PhotonNetwork.LocalPlayer != null ? PhotonNetwork.LocalPlayer.ActorNumber : -1;
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                if (player.ActorNumber == localActorNumber)
                {
                    continue;
                }

                if (GetPlayerClassId(player) == classId)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasDuplicateClassIds()
        {
            HashSet<int> seen = new HashSet<int>();
            foreach (Player player in PhotonNetwork.PlayerList)
            {
                int classId = GetPlayerClassId(player);
                if (!seen.Add(classId))
                {
                    return true;
                }
            }

            return false;
        }

        private int GetFirstAvailableClassId()
        {
            IReadOnlyCollection<int> taken = GetTakenClassIdsExcludingLocal();
            foreach (int classId in DefaultClassIds)
            {
                if (!taken.Contains(classId))
                {
                    return classId;
                }
            }

            return DefaultClassIds[0];
        }

        private void InitializeLocalCustomProperties()
        {
            Hashtable props = new Hashtable();
            bool shouldUpdate = false;

            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(NetworkPropertyKeys.Player.Ready))
            {
                props[NetworkPropertyKeys.Player.Ready] = false;
                shouldUpdate = true;
            }

            if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(NetworkPropertyKeys.Player.ClassId))
            {
                props[NetworkPropertyKeys.Player.ClassId] = GetFirstAvailableClassId();
                shouldUpdate = true;
            }
            else
            {
                int currentClassId = GetPlayerClassId(PhotonNetwork.LocalPlayer);
                if (!IsClassAvailable(currentClassId))
                {
                    props[NetworkPropertyKeys.Player.ClassId] = GetFirstAvailableClassId();
                    shouldUpdate = true;
                }
            }

            if (shouldUpdate)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }

        private void BindView()
        {
            waitingRoomView = waitingRoomViewBehaviour as IWaitingRoomView;
            if (waitingRoomView == null)
            {
                Debug.LogError("[WaitingRoom] View is not assigned or does not implement IWaitingRoomView.");
                return;
            }

            waitingRoomView.OnReadyClicked += HandleReadyClicked;
            waitingRoomView.OnStartClicked += HandleStartClicked;
            waitingRoomView.OnLeaveClicked += HandleLeaveClicked;
            waitingRoomView.OnKickClicked += HandleKickClicked;
            waitingRoomView.OnClassSelected += HandleClassSelected;
        }

        private void UnbindView()
        {
            if (waitingRoomView == null)
            {
                return;
            }

            waitingRoomView.OnReadyClicked -= HandleReadyClicked;
            waitingRoomView.OnStartClicked -= HandleStartClicked;
            waitingRoomView.OnLeaveClicked -= HandleLeaveClicked;
            waitingRoomView.OnKickClicked -= HandleKickClicked;
            waitingRoomView.OnClassSelected -= HandleClassSelected;
        }
    }
}
