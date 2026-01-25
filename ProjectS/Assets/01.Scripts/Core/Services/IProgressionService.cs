using System;

namespace PS.Core.Services
{
    public interface IProgressionService
    {
        int Level { get; }
        float CurrentXp { get; }
        float XpToNext { get; }

        event Action<int> LevelUp;
    }
}
