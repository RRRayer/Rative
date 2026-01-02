using UnityEngine;

namespace ProjectS.Data.Definitions
{
    [CreateAssetMenu(menuName = "ProjectS/Definitions/Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float maxHealth;
        public float moveSpeed;
        public GameObject prefab;
    }
}
