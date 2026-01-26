using System;
using UnityEngine;

namespace PS.Data.Definitions
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
        [TextArea(2, 4)] public string description;
    }
}
