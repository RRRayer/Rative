using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectS.Networking;

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

            if (SharedProgressionManager.Instance == null)
            {
                gameObject.AddComponent<SharedProgressionManager>();
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
                    // Wait for room join before spawning the local player.
                    StartCoroutine(InstantiatePlayerWhenInRoom());
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManager.GetActiveScene().name);
                }
            }
        }

        /// <summary>
        /// Wait until the client is in a room before instantiating the local player.
        /// </summary>
        private IEnumerator InstantiatePlayerWhenInRoom()
        {
            // Wait until the client is in a room.
            while (!PhotonNetwork.InRoom)
            {
                yield return null;
            }

            Debug.LogFormat("[GameManager] LocalPlayer spawned in {0}", SceneManager.GetActiveScene().name);
            PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }

        /// <summary>
        /// Requests leaving the current room if connected.
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
