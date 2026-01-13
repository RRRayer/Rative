using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using ProjectS.Gameplay.Stats;
using UnityEngine;

namespace ProjectS.Gameplay.Skills
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
