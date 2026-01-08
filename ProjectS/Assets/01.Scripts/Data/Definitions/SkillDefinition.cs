using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [CreateAssetMenu(menuName = "ProjectS/Definitions/Skill")]
    public class SkillDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float cooldown;
        public Sprite icon;
        public SkillBehaviour behaviour;
        public SkillUpgradeTrack upgradeTrack;
    }
}
