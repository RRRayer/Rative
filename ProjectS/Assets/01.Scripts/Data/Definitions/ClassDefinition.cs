using UnityEngine;

namespace PS.Data.Definitions
{
    [CreateAssetMenu(menuName = "PS/Definitions/Class")]
    public class ClassDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public StatBlock stats;

        public SkillDefinition basicAttack;
        public SkillDefinition skillQ;
        public SkillDefinition skillE;
        public SkillDefinition skillR;

        public WarriorBasicUpgradeTrack basicUpgradeTrack;
        public WarriorQUpgradeTrack skillQUpgradeTrack;
        public WarriorEUpgradeTrack skillEUpgradeTrack;
        public WarriorRUpgradeTrack skillRUpgradeTrack;
        public PassiveUpgradeTrack passiveUpgradeTrack;
    }
}
