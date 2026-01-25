using PS.Core.Skills;
using UnityEngine;

namespace PS.Core.Combat
{
    public struct KillInfo
    {
        public int SourceId;
        public SkillSlot Slot;
        public float XpReward;
        public GameObject Target;
    }
}
