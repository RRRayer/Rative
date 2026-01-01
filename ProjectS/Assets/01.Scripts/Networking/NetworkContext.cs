using UnityEngine;
using ProjectS.Core.Services;

namespace ProjectS.Networking
{
    public class NetworkContext : MonoBehaviour, INetworkContext
    {
        [SerializeField] private bool isConnected;
        [SerializeField] private bool isHost;
        [SerializeField] private int localPlayerId;

        public bool IsConnected => isConnected;
        public bool IsHost => isHost;
        public int LocalPlayerId => localPlayerId;
    }
}
