using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [CreateAssetMenu(menuName = "ProjectS/Definitions/Skill Upgrade Track")]
    public class SkillUpgradeTrack : ScriptableObject
    {
        public SkillUpgradeStep[] steps;

        public SkillUpgradeState Evaluate(int level)
        {
            SkillUpgradeState state = SkillUpgradeState.CreateDefault();
            if (steps == null || steps.Length == 0)
            {
                return state;
            }

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].level <= level)
                {
                    state.Apply(steps[i]);
                }
            }

            return state;
        }
    }
}
