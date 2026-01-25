using UnityEngine;

namespace PS.Data.Definitions
{
    [CreateAssetMenu(menuName = "PS/Definitions/Skill")]
    public class SkillDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float cooldown;
        public Sprite icon;
        public SkillBehaviour behaviour;
        public SkillUpgradeTrackBase upgradeTrack;
    }
}
