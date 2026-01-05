using ProjectS.Core.Skills;
using UnityEngine;

namespace ProjectS.Core.Combat
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
