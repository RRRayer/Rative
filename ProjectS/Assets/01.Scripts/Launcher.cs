using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace PS
{
    /// <summary>
    /// MonoBehaviourPunCallbacks: OnEnable, OnDisable 호출 없이 콜백 넣고 빼줌
    /// 서버 접속 및 방 입장, 관리: 포톤 마스터 서버 접속, 방 입장
    /// 방 이장 후 자신이 마스터 클라이언트라면 게임 씬 로드.
    /// 다른 클라이언트들도 자동으로 같은 씬으로 동기화
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
        
        [SerializeField] private int maxPlayersPerRoom = 3;
        [SerializeField] private string fixedRoomName = "Room1"; // Added fixed room name

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
        /// 게임 플레이 버튼과 바인딩
        /// </summary>
        public void Connect()
        {
            isConnecting = true;
            
            // UI 초기화
            progressLabel.SetActive(true);
            controlPanel.SetActive(false);
            
            // 포톤 서버와 연결 됐다면 '룸'에 들어간다.
            if (PhotonNetwork.IsConnected)
            {
                // 특정 Room에 들어가기 위한 시도. 실패시 OnJoinRoomFailed()로 Notify 후 새로 Room 만든다.
                PhotonNetwork.JoinRoom(fixedRoomName);
            }
            // 포톤 서버 접속부터 한다.
            else
            {
                // Photon 온라인 서버와 연결
                PhotonNetwork.GameVersion = gameVersion;
                // Photon > PhotonUnityNetworking > Resource > PhotonServerSetting 존재
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            if (isConnecting)
            {
                Debug.Log("[Launcher]: OnConnectedToMaster() was called by PUN");
                // Random room에 들어가는 대신 고정된 Room에 들어간다.
                PhotonNetwork.JoinRoom(fixedRoomName);    
            }
        }

        /// <summary>
        /// 서버와 연결 해제되는 시점에 실행
        /// </summary>
        public override void OnDisconnected(DisconnectCause cause)
        {
            // UI 초기화
            progressLabel.SetActive(false);
            controlPanel.SetActive(true);
            Debug.LogWarningFormat("[Launcher]: OnDisconnected() was called by PUN with reason {0}", cause);
        }

        /// <summary>
        /// PhotonNetwork.JoinRoom() 의 실패 시 콜백
        /// </summary>
        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogFormat("[Launcher]: OnJoinRoomFailed() was called by PUN. DebugMessage: {0}", message);

            // 새로운 룸 생성
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayersPerRoom;
            PhotonNetwork.CreateRoom(fixedRoomName, roomOptions);
        }

        /// <summary>
        /// 룸 입장 시 콜백
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
        /// 마스터 클라이언트는 레벨을 로드한다.(씬 로딩)
        /// </summary>
        private void LoadLevel(string sceneName)
        {
            Debug.Log("[Launcher] OnJoinedRoom: As Master Client, loading 'RoomFor1' scene.");
            PhotonNetwork.LoadLevel(sceneName);
        }
    }
}

