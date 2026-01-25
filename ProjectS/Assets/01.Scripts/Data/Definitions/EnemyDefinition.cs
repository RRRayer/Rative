using UnityEngine;

namespace PS.Data.Definitions
{
    [CreateAssetMenu(menuName = "PS/Definitions/Enemy")]
    public class EnemyDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public float maxHealth;
        public float moveSpeed;
        public GameObject prefab;
    }
}
