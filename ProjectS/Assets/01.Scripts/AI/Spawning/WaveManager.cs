using System;
using UnityEngine;
using ProjectS.Core.Services;

namespace ProjectS.AI.Spawning
{
    public class WaveManager : MonoBehaviour, IWaveService
    {
        public int CurrentWave { get; private set; }
        public bool IsActive { get; private set; }
        public float TimeRemaining { get; private set; }

        public event Action<int> WaveStarted;
        public event Action<int> WaveCompleted;

        public void BeginWave(int waveIndex, float durationSeconds)
        {
            CurrentWave = waveIndex;
            IsActive = true;
            TimeRemaining = durationSeconds;
            WaveStarted?.Invoke(waveIndex);
        }

        public void EndWave()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;
            WaveCompleted?.Invoke(CurrentWave);
        }
    }
}
