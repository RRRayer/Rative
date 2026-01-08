using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [CreateAssetMenu(menuName = "ProjectS/Definitions/Class Catalog")]
    public class ClassCatalog : ScriptableObject
    {
        public ClassDefinition defaultClass;
        public ClassDefinition[] classes;

        public ClassDefinition GetById(string id)
        {
            if (classes == null || classes.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < classes.Length; i++)
            {
                ClassDefinition entry = classes[i];
                if (entry != null && entry.id == id)
                {
                    return entry;
                }
            }

            return null;
        }
    }
}
