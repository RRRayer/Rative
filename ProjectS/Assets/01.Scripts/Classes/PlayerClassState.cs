using ProjectS.Core.Skills;
using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Classes
{
    public sealed class PlayerClassState : MonoBehaviour
    {
        [SerializeField] private ClassDefinition classDefinition;

        public ClassDefinition CurrentClass => classDefinition;

        public void SetClass(ClassDefinition definition)
        {
            classDefinition = definition;
        }

        public SkillDefinition GetSkill(SkillSlot slot)
        {
            if (classDefinition == null)
            {
                return null;
            }

            return slot switch
            {
                SkillSlot.Q => classDefinition.skillQ,
                SkillSlot.E => classDefinition.skillE,
                SkillSlot.R => classDefinition.skillR,
                _ => null
            };
        }
    }
}
