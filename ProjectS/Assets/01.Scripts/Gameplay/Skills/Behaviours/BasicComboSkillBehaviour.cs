using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Gameplay.Skills.Behaviours
{
    [CreateAssetMenu(menuName = "ProjectS/Skills/Basic Combo")]
    public class BasicComboSkillBehaviour : SkillBehaviour
    {
        public float baseDamage = 12f;
        public GameObject swingPrefab;
        public GameObject finisherPrefab;
        public float comboResetSeconds = 0.9f;
        public float finisherMultiplier = 1.5f;
        public float swingHitboxDuration = 0.2f;
        public float finisherHitboxDuration = 0.25f;

        public void Execute(SkillContext context)
        {
            if (context.Executor == null)
            {
                return;
            }

            context.Executor.ExecuteBasicCombo(this, context);
        }
    }
}
