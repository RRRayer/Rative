using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace PS
{
    /// <summary>
    /// MonoBehaviourPunCallbacks handles PUN callbacks without manual registration.
    /// Connects to Photon, joins/creates a room, and syncs scene loading.
    /// The master client loads the game scene; others sync automatically.
    /// </summary>
    public class Launcher : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// Application version used by Photon for matchmaking.
        /// </summary>
        private const string gameVersion = "1";

        [Header("UI Components")]
        [SerializeField] private GameObject progressLabel;
        [SerializeField] private GameObject controlPanel;

        [SerializeField] private int maxPlayersPerRoom = 3;
        [SerializeField] private string fixedRoomName = "Room1"; // Added fixed room name

        private bool isConnecting;

        private void Awake()
        {
            // Allow the master client to call PhotonNetwork.LoadLevel.
            // This syncs the level for all clients.
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            // Initialize UI.
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        /// <summary>
        /// Called by the UI play button.
        /// </summary>
        public void Connect()
        {
            isConnecting = true;

            // Initialize UI.
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);

            // If already connected, join the fixed room directly.
            if (PhotonNetwork.IsConnected)
            {
                // Attempt to join the fixed room. If it fails, OnJoinRoomFailed will create it.
                PhotonNetwork.JoinRoom(fixedRoomName);
            }
            // Otherwise, connect to Photon first.
            else
            {
                // Connect to the Photon online server.
                PhotonNetwork.GameVersion = gameVersion;
                // Photon > PhotonUnityNetworking > Resource > PhotonServerSetting
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            if (isConnecting)
            {
                Debug.Log("[Launcher]: OnConnectedToMaster() was called by PUN");
                // Join the fixed room instead of a random room.
                PhotonNetwork.JoinRoom(fixedRoomName);
            }
        }

        /// <summary>
        /// Called when the client disconnects from Photon.
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            // Initialize UI.
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("[Launcher]: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        /// <summary>
        /// Called when PhotonNetwork.JoinRoom fails.
        /// </summary>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogFormat("[Launcher]: OnJoinRoomFailed() was called by PUN. DebugMessage: {0}", message);

            // Create a new room with the fixed name.
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayersPerRoom;
            PhotonNetwork.JoinOrCreateRoom(fixedRoomName, roomOptions, TypedLobby.Default);
        }

        /// <summary>
        /// Called when the client joins a room.
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() called by PUN. This client is now in a room.");

            // Only the master client should load the level. Others will sync automatically.
            if (PhotonNetwork.IsMasterClient)
            {
                LoadLevel("RoomFor1");
            }
        }

        /// <summary>
        /// Loads the scene from the master client.
        /// </summary>
        private void LoadLevel(string sceneName)
        {
            Debug.Log("[Launcher] OnJoinedRoom: As Master Client, loading 'RoomFor1' scene.");
            PhotonNetwork.LoadLevel(sceneName);
        }
    }
}
