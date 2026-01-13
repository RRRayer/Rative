using UnityEngine;

namespace ProjectS.Data.Definitions
{
    public abstract class SkillUpgradeTrackBase : ScriptableObject
    {
        public abstract SkillUpgradeState Evaluate(int level);
    }
}
