using ProjectS.Data.Definitions;
using UnityEngine;

namespace ProjectS.Classes
{
    public static class TestClassFactory
    {
        private static ClassDefinition cachedClass;

        public static ClassDefinition GetOrCreate()
        {
            if (cachedClass == null)
            {
                cachedClass = CreateTestClass();
            }

            return cachedClass;
        }

        private static ClassDefinition CreateTestClass()
        {
            ClassDefinition classDefinition = ScriptableObject.CreateInstance<ClassDefinition>();
            classDefinition.hideFlags = HideFlags.DontSave;
            classDefinition.id = "test_class";
            classDefinition.displayName = "Test Class";
            classDefinition.description = "Temporary class for early skill testing.";

            classDefinition.skillQ = CreateSkill("test_q", "Test Q", 3f);
            classDefinition.skillE = CreateSkill("test_e", "Test E", 5f);
            classDefinition.skillR = CreateSkill("test_r", "Test R", 8f);

            return classDefinition;
        }

        private static SkillDefinition CreateSkill(string id, string displayName, float cooldown)
        {
            SkillDefinition skillDefinition = ScriptableObject.CreateInstance<SkillDefinition>();
            skillDefinition.hideFlags = HideFlags.DontSave;
            skillDefinition.id = id;
            skillDefinition.displayName = displayName;
            skillDefinition.cooldown = cooldown;
            skillDefinition.icon = null;
            skillDefinition.prefab = null;
            return skillDefinition;
        }
    }
}
