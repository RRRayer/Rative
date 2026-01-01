using System;

namespace ProjectS.Core.Services
{
    public interface IWaveService
    {
        int CurrentWave { get; }
        bool IsActive { get; }
        float TimeRemaining { get; }

        event Action<int> WaveStarted;
        event Action<int> WaveCompleted;
    }
}
