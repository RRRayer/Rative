using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PS.Manager
{
    public class GameManager : MonoBehaviourPunCallbacks
    {
        public static GameManager Instance { get; private set; }
        [Tooltip("The prefab to use for representing the player")]
        [SerializeField] private GameObject playerPrefab;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.LocalPlayerInstance == null)
                {
                    // 방에 입장하기 전까지 기다렸다가 플레이어 오브젝트를 생성한다.
                    StartCoroutine(InstantiatePlayerWhenInRoom());
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManager.GetActiveScene().name);
                }
            }
        }
        
        /// <summary>
        /// 방에 입장할 때까지 대기 후 플레이어 오브젝트를 생성한다.(Race Condition 방지)
        /// </summary>
        private IEnumerator InstantiatePlayerWhenInRoom()
        {
            // Wait until the client is in a room
            while (!PhotonNetwork.InRoom)
            {
                yield return null;
            }

            Debug.LogFormat("[GameManager] LocalPlayer 객체 생성 from {0}", SceneManager.GetActiveScene().name);
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }

        /// <summary>
        /// Called when local player left the room
        /// </summary>
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// 룸 종료 버튼에 바인딩
        /// </summary>
        public void LeaveRoom()
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        // OnPlayerEnteredRoom, OnPlayerLeftRoom, and LoadArena are no longer needed.
        // Scene loading is now handled by the Launcher, and instantiation is handled by this GameManager's Start method.
        // This avoids all race conditions related to scene loading and player instantiation.
    }
}

