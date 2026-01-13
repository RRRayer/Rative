using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [CreateAssetMenu(menuName = "ProjectS/Definitions/Passive Upgrade Track")]
    public class PassiveUpgradeTrack : ScriptableObject
    {
        public PassiveUpgradeStep[] steps;

        public bool TryGetStep(int level, out PassiveUpgradeStep step)
        {
            if (steps != null)
            {
                for (int i = 0; i < steps.Length; i++)
                {
                    if (steps[i].level == level)
                    {
                        step = steps[i];
                        return true;
                    }
                }
            }

            step = default;
            return false;
        }

        public PassiveUpgradeStep Evaluate(int level)
        {
            PassiveUpgradeStep result = default;
            if (steps == null || steps.Length == 0)
            {
                return result;
            }

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].level <= level)
                {
                    result = steps[i];
                }
            }

            return result;
        }
    }
}
