using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct WaveSpawnEntry
    {
        public EnemyDefinition enemy;
        public int count;
        public float spawnRatePerSecond;
    }

    [CreateAssetMenu(menuName = "ProjectS/Definitions/Wave")]
    public class WaveDefinition : ScriptableObject
    {
        public string id;
        public float durationSeconds;
        public WaveSpawnEntry[] spawns;
    }
}
