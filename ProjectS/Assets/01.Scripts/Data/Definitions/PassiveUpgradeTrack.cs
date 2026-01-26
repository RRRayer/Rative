using UnityEngine;

namespace PS.Data.Definitions
{
    [CreateAssetMenu(menuName = "PS/Definitions/Passive Upgrade Track")]
    public class PassiveUpgradeTrack : ScriptableObject
    {
        public string displayName;
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

        public string GetStepDescription(int level)
        {
            if (steps == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < steps.Length; i++)
            {
                if (steps[i].level == level)
                {
                    return steps[i].description;
                }
            }

            return string.Empty;
        }
    }
}
