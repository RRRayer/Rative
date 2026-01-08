using System;
using UnityEngine;
using ProjectS.Core.Services;

namespace ProjectS.Progression.Leveling
{
    public class ProgressionManager : MonoBehaviour, IProgressionService
    {
        public int Level { get; private set; } = 1;
        public float CurrentXp { get; private set; }
        public float XpToNext { get; private set; } = 10f;

        public event Action<int> LevelUp;

        public void AddXp(float amount)
        {
            CurrentXp += amount;
            while (CurrentXp >= XpToNext)
            {
                CurrentXp -= XpToNext;
                Level += 1;
                XpToNext *= 1.2f;
                LevelUp?.Invoke(Level);
            }
        }
    }
}
