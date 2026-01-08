using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Gameplay.Skills.Behaviours
{
    [CreateAssetMenu(menuName = "ProjectS/Skills/Dash")]
    public class DashSkillBehaviour : SkillBehaviour
    {
        public float baseDamage = 20f;
        public GameObject prefab;
        public float dashDistance = 5f;
        public float dashDuration = 0.2f;
        public float pullRadius = 2f;
        public float pullStrength = 2f;

        public void Execute(SkillContext context)
        {
            if (context.Executor == null)
            {
                return;
            }

            context.Executor.ExecuteDash(this, context);
        }
    }
}
