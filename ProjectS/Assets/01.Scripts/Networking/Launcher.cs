using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace ProjectS.Networking
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        private readonly string gameVersion = "1";
        private readonly int maxPlayersPerRoom = 4;

        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}