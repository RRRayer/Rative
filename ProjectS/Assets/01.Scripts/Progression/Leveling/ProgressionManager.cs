using System;
using UnityEngine;
using ProjectS.Core.Services;

namespace ProjectS.Progression.Leveling
{
    public class ProgressionManager : MonoBehaviour, IProgressionService
    {
        [SerializeField] private int level = 1;
        [SerializeField] private float currentXp;
        [SerializeField] private float xpToNext = 10f;

        public int Level => level;
        public float CurrentXp => currentXp;
        public float XpToNext => xpToNext;

        public event Action<int> LevelUp;

        public void AddXp(float amount)
        {
            currentXp += amount;
            while (currentXp >= xpToNext)
            {
                currentXp -= xpToNext;
                level += 1;
                xpToNext = GetRequiredXpForLevel(level);
                LevelUp?.Invoke(level);
            }
        }

        public void SetProgress(int newLevel, float newCurrentXp, float newXpToNext)
        {
            level = Mathf.Max(1, newLevel);
            currentXp = Mathf.Max(0f, newCurrentXp);
            xpToNext = Mathf.Max(1f, newXpToNext);
        }

        public static float GetRequiredXpForLevel(int currentLevel)
        {
            int levelValue = Mathf.Max(1, currentLevel);
            float needed = (100f * (levelValue - 1) * (levelValue - 1)) + 20f;
            return Mathf.Ceil(needed);
        }
    }
}
