using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [CreateAssetMenu(menuName = "ProjectS/Definitions/Class")]
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

        public SkillUpgradeTrack basicUpgradeTrack;
        public SkillUpgradeTrack skillQUpgradeTrack;
        public SkillUpgradeTrack skillEUpgradeTrack;
        public SkillUpgradeTrack skillRUpgradeTrack;
    }
}
