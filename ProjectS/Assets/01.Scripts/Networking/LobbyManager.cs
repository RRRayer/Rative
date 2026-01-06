using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace ProjectS.Networking
{
    /// <summary>
    /// Lobby controller: Photon flow + view coordination (UI lives in ProjectS.UI).
    /// </summary>
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [Header("View")]
        [SerializeField] private MonoBehaviour lobbyViewBehaviour;
        [Header("Scenes")]
        [SerializeField] private string waitingRoomSceneName = "GameRoom";

        private ILobbyView lobbyView;
        private readonly Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
        private string pendingPrivateRoomName;

        private const byte MaxPlayersPerRoom = 4;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }


        public override void OnEnable()
        {
            base.OnEnable();
            BindView();         
        }
        public override void OnDisable()
        {
            base.OnDisable();
            UnbindView();
        }

        public void JoinLobby()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogWarning("[Lobby] Cannot join lobby. Not connected to Photon.");
                return;
            }

            ClientState state = PhotonNetwork.NetworkClientState;
            if (state == ClientState.JoinedLobby || state == ClientState.JoiningLobby)
            {
                return;
            }

            PhotonNetwork.JoinLobby();
        }

        public override void OnConnectedToMaster()
        {
            // Main+Lobby flow triggers JoinLobby from UI, so avoid auto-joining here.
        }

        public override void OnJoinedLobby()
        {
            Debug.Log("[Lobby] Joined lobby. Waiting for room list updates.");
            cachedRoomList.Clear();
            PushRoomsToView();
        }

        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                if (info.RemovedFromList || !info.IsOpen || !info.IsVisible)
                {
                    cachedRoomList.Remove(info.Name);
                    continue;
                }

                cachedRoomList[info.Name] = info;
            }

            PushRoomsToView();
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"[Lobby] Failed to join room. Code: {returnCode}, Message: {message}");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"[Lobby] Failed to create room. Code: {returnCode}, Message: {message}");
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("[Lobby] Joined room. Moving to waiting room scene as master for sync.");

            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel(waitingRoomSceneName);
            }
        }

        private void HandleCreateRoomRequested()
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogWarning("[Lobby] Cannot create room. Not connected to Photon.");
                return;
            }

            CreateRoomForm form = lobbyView?.ReadCreateRoomForm() ?? default;
            string roomName = string.IsNullOrWhiteSpace(form.RoomName)
                ? $"Room_{UnityEngine.Random.Range(0, 9999)}"
                : form.RoomName.Trim();

            bool isPrivate = form.IsPrivate;
            string password = isPrivate ? form.Password ?? string.Empty : string.Empty;

            Hashtable customProps = new Hashtable
            {
                { NetworkPropertyKeys.Room.Password, password },
                { NetworkPropertyKeys.Room.IsPrivate, isPrivate }
            };

            RoomOptions options = new RoomOptions
            {
                MaxPlayers = MaxPlayersPerRoom,
                IsVisible = true,
                IsOpen = true,
                CustomRoomProperties = customProps,
                CustomRoomPropertiesForLobby = new[] { NetworkPropertyKeys.Room.Password, NetworkPropertyKeys.Room.IsPrivate }
            };

            PhotonNetwork.CreateRoom(roomName, options, TypedLobby.Default);
        }

        private void HandleRefreshRequested()
        {
            PushRoomsToView();
        }

        private void HandleJoinRoomRequested(string roomName)
        {
            if (string.IsNullOrEmpty(roomName) || !cachedRoomList.TryGetValue(roomName, out RoomInfo roomInfo))
            {
                return;
            }

            if (IsPrivateRoom(roomInfo))
            {
                pendingPrivateRoomName = roomInfo.Name;
                lobbyView?.PromptPassword(roomInfo.Name);
                return;
            }

            PhotonNetwork.JoinRoom(roomInfo.Name);
        }

        private void HandlePasswordSubmitted(string password)
        {
            if (string.IsNullOrEmpty(pendingPrivateRoomName))
            {
                return;
            }

            if (!cachedRoomList.TryGetValue(pendingPrivateRoomName, out RoomInfo roomInfo))
            {
                Debug.LogWarning("[Lobby] Selected room disappeared before password entry.");
                lobbyView?.ClosePasswordPrompt();
                return;
            }

            string expectedPassword = TryGetRoomPassword(roomInfo);
            string providedPassword = password ?? string.Empty;

            if (expectedPassword == providedPassword)
            {
                PhotonNetwork.JoinRoom(pendingPrivateRoomName);
                lobbyView?.ClosePasswordPrompt();
                pendingPrivateRoomName = null;
            }
            else
            {
                Debug.LogWarning("[Lobby] Incorrect password for room join attempt.");
            }
        }

        private void HandlePasswordCancelled()
        {
            pendingPrivateRoomName = null;
            lobbyView?.ClosePasswordPrompt();
        }

        private void BindView()
        {
            lobbyView = lobbyViewBehaviour as ILobbyView;
            if (lobbyView == null)
            {
                Debug.LogError("[Lobby] Lobby view is not assigned or does not implement ILobbyView.");
                return;
            }

            lobbyView.OnCreateRoomClicked += HandleCreateRoomRequested;
            lobbyView.OnRefreshRequested += HandleRefreshRequested;
            lobbyView.OnJoinRoomRequested += HandleJoinRoomRequested;
            lobbyView.OnPasswordSubmitted += HandlePasswordSubmitted;
            lobbyView.OnPasswordCancelled += HandlePasswordCancelled;
        }

        private void UnbindView()
        {
            if (lobbyView == null)
            {
                return;
            }

            lobbyView.OnCreateRoomClicked -= HandleCreateRoomRequested;
            lobbyView.OnRefreshRequested -= HandleRefreshRequested;
            lobbyView.OnJoinRoomRequested -= HandleJoinRoomRequested;
            lobbyView.OnPasswordSubmitted -= HandlePasswordSubmitted;
            lobbyView.OnPasswordCancelled -= HandlePasswordCancelled;
        }

        private void PushRoomsToView()
        {
            if (lobbyView == null)
            {
                return;
            }

            IEnumerable<LobbyRoomViewModel> rooms = cachedRoomList.Values
                .OrderBy(r => r.Name)
                .Select(r => new LobbyRoomViewModel
                {
                    Name = r.Name,
                    PlayerCount = r.PlayerCount,
                    MaxPlayers = r.MaxPlayers,
                    IsPrivate = IsPrivateRoom(r)
                });

            lobbyView.RenderRooms(rooms);
        }

        private static bool IsPrivateRoom(RoomInfo roomInfo)
        {
            if (roomInfo?.CustomProperties == null)
            {
                return false;
            }

            if (roomInfo.CustomProperties.TryGetValue(NetworkPropertyKeys.Room.IsPrivate, out object value) &&
                value is bool isPrivate)
            {
                return isPrivate;
            }

            return false;
        }

        private static string TryGetRoomPassword(RoomInfo roomInfo)
        {
            if (roomInfo?.CustomProperties == null)
            {
                return string.Empty;
            }

            if (roomInfo.CustomProperties.TryGetValue(NetworkPropertyKeys.Room.Password, out object passwordObj) &&
                passwordObj is string password)
            {
                return password;
            }

            return string.Empty;
        }
    }
}
