using UnityEngine;
using PS.Core.Services;

namespace PS.Networking
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
