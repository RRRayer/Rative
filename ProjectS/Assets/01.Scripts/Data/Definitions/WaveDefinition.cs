using System;
using UnityEngine;

namespace PS.Data.Definitions
{
    [Serializable]
    public struct WaveSpawnEntry
    {
        public EnemyDefinition enemy;
        public int count;
        public float spawnRatePerSecond;
    }

    [CreateAssetMenu(menuName = "PS/Definitions/Wave")]
    public class WaveDefinition : ScriptableObject
    {
        public string id;
        public float durationSeconds;
        public WaveSpawnEntry[] spawns;
    }
}
