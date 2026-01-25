using PS.Core.Skills;
using UnityEngine;

namespace PS.Core.Combat
{
    public struct DamageInfo
    {
        public float Amount;
        public Vector3 Point;
        public Vector3 Direction;
        public int SourceId;
        public SkillSlot Slot;
    }
}
