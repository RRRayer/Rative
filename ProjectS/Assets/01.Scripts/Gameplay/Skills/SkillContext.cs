using PS.Core.Skills;
using PS.Data.Definitions;
using PS.Gameplay.Stats;
using UnityEngine;

namespace PS.Gameplay.Skills
{
    public struct SkillContext
    {
        public SkillDefinition Definition;
        public SkillSlot Slot;
        public int SkillLevel;
        public SkillUpgradeState UpgradeState;
        public Transform Origin;
        public GameObject Owner;
        public PlayerStats Stats;
        public CharacterController CharacterController;
        public PlayerSkillExecutor Executor;
        public int SourceId;
    }
}
