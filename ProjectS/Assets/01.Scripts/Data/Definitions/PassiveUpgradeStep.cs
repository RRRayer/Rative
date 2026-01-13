using System;
using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [Serializable]
    public struct PassiveUpgradeStep
    {
        [Range(1, 5)] public int level;
        [Range(0f, 0.5f)] public float attackSpeedPerStackPercent;
        public float stackDurationSeconds;
        [Range(0f, 0.5f)] public float moveSpeedPerStackPercent;
        public int maxStacks;
        [Range(0f, 1f)] public float cooldownReductionAtMaxPercent;
    }
}
