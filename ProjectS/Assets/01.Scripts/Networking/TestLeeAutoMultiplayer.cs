using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PS.Networking
{
    public class TestLeeAutoMultiplayer : MonoBehaviourPunCallbacks
    {
        [SerializeField] private string targetSceneName = "TestLee";
        [SerializeField] private string roomName = "TestLeeRoom";
        [SerializeField] private byte maxPlayers = 4;
        [SerializeField] private bool autoConnect = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureInstance()
        {
            if (FindObjectOfType<TestLeeAutoMultiplayer>() != null)
            {
                return;
            }

            GameObject host = new GameObject("TestLeeAutoMultiplayer");
            host.AddComponent<TestLeeAutoMultiplayer>();
        }

        private void Start()
        {
            if (!autoConnect)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != targetSceneName)
            {
                return;
            }

            PhotonNetwork.AutomaticallySyncScene = true;

            if (PhotonNetwork.IsConnectedAndReady)
            {
                JoinRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings();
            }
        }

        public override void OnConnectedToMaster()
        {
            if (!autoConnect)
            {
                return;
            }

            JoinRoom();
        }

        public override void OnJoinedRoom()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name == targetSceneName)
            {
                return;
            }

            PhotonNetwork.LoadLevel(targetSceneName);
        }

        private void JoinRoom()
        {
            RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
            PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        }
    }
}
