using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace PS
{
    /// <summary>
    /// MonoBehaviourPunCallbacks: OnEnable, OnDisable 호출 없이 콜백 넣고 빼줌
    /// </summary>
    public class Launcher : MonoBehaviourPunCallbacks
    {
        /// <summary>
        /// # 어플리케이션 ID와 게임 버전
        /// 어플리케이션 ID로 플레이어 구분
        /// </summary>
        private const string gameVersion = "1";
        
        [Header("UI Components")]
        [SerializeField] private GameObject progressLabel;
        [SerializeField] private GameObject controlPanel;
        
        [SerializeField] private int maxPlayersPerRoom = 4;

        private bool isConnecting;

        private void Awake()
        {
            // Master client가 PhotonNetwork.LoadLevel() 사용 가능
            // 모든 Client의 레벨 싱크 맞춤
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            // UI 초기화
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
        }

        /// <summary>
        /// # Room
        /// '룸 기반 게임'으로 구축. 경기 당 제한된 플레이어 수가 있다.
        /// 룸 안의 모든 사람은 다른 사람이 보낸 것을 수신하며, 룸 밖에서는 통신이 안 된다.
        /// # Robby
        /// 로비는 마스터 서버에 존재하며 '게임의 룸 목록'을 제공
        /// # 게임 플레이 버튼과 바인딩
        /// </summary>
        public void Connect()
        {
            isConnecting = true;
            
            // UI 초기화
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            
            if (PhotonNetwork.IsConnected)
            {
                // Random room에 들어가기 위한 시도. 실패시 OnJoinRandomFailed()로 Notify 후 새로 Room 만든다.
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                // Photon 온라인 서버와 연결
                PhotonNetwork.GameVersion = gameVersion;
                // Photon > PhotonUnityNetworking > Resource > PhotonServerSetting 존재
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        /// <summary>
        /// Master server에 연결되는 시점에 실행
        /// 씬이 로드되면 OnEnable에서 OnConnectedToMaster() 메서드를 실행한다.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            if (isConnecting)
            {
                Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
                PhotonNetwork.JoinRandomRoom();    
            }
        }

        public override void OnDisconnected(DisconnectCause cause)
        {
            // UI 초기화
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        /// <summary>
        /// PhotonNetwork.JoinRandomRoom() 의 실패 시 콜백
        /// </summary>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayersPerRoom;
            PhotonNetwork.CreateRoom(null, roomOptions);
        }

        /// <summary>
        /// PhotonNetwork.JoinRandomRoom() 의 성공 시 콜백
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log("PUN Basics Tutorial/Launcher: OnJoinedRoom() called by PUN. Now this client is in a room.");

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                Debug.Log("We load the 'Room for 1'");
                PhotonNetwork.LoadLevel("RoomFor1");
            }
        }
    }
}

