using UnityEngine;
using ProjectS.Core.Services;

namespace ProjectS.Systems.Flow
{
    public enum GameMode
    {
        Survival,
        Raid
    }

    public class GameFlowManager : MonoBehaviour
    {
        [SerializeField] private GameMode mode = GameMode.Survival;

        private IWaveService waveService;
        private IProgressionService progressionService;
        private INetworkContext networkContext;

        public void Initialize(IWaveService wave, IProgressionService progression, INetworkContext network)
        {
            waveService = wave;
            progressionService = progression;
            networkContext = network;
        }
    }
}
