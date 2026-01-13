using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Core.Combat
{
    public struct KillInfo
    {
        public int SourceId;
        public SkillSlot Slot;
        public float XpReward;
        public GameObject Target;
    }
}
