using UnityEngine;

namespace PS.Data.Definitions
{
    public abstract class SkillUpgradeTrackBase : ScriptableObject
    {
        public abstract SkillUpgradeState Evaluate(int level);
        public abstract string GetStepDescription(int level);
    }
}
