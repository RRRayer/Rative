using System.Collections.Generic;
using UnityEngine;

namespace ProjectS.Networking
{
    public sealed class PunPrefabPool : Photon.Pun.IPunPrefabPool
    {
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Stack<GameObject>> _pool = new Dictionary<string, Stack<GameObject>>();

        public void RegisterPrefabs(List<GameObject> prefabs)
        {
            if (prefabs == null) return;

            for (int i = 0; i < prefabs.Count; i++)
            {
                RegisterPrefab(prefabs[i]);
            }
        }

        public void RegisterPrefab(GameObject prefab)
        {
            if (prefab == null) return;

            string prefabId = prefab.name;
            if (!_prefabs.ContainsKey(prefabId))
            {
                _prefabs.Add(prefabId, prefab);
            }

            if (!_pool.ContainsKey(prefabId))
            {
                _pool.Add(prefabId, new Stack<GameObject>());
            }
        }

        public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
        {
            if (!_prefabs.TryGetValue(prefabId, out GameObject prefab) || prefab == null)
            {
                Debug.LogError($"[PunPrefabPool] Prefab '{prefabId}' is not registered.");
                return null;
            }

            if (_pool.TryGetValue(prefabId, out Stack<GameObject> stack))
            {
                while (stack.Count > 0)
                {
                    GameObject instance = stack.Pop();
                    if (instance == null) continue;

                    instance.transform.SetPositionAndRotation(position, rotation);
                    instance.SetActive(false);
                    return instance;
                }
            }

            bool wasActive = prefab.activeSelf;
            if (wasActive) prefab.SetActive(false);

            GameObject created = Object.Instantiate(prefab, position, rotation);

            if (wasActive) prefab.SetActive(true);

            created.name = prefabId;
            created.SetActive(false);
            return created;
        }

        public void Destroy(GameObject gameObject)
        {
            if (gameObject == null) return;

            string prefabId = gameObject.name;
            if (!_pool.TryGetValue(prefabId, out Stack<GameObject> stack))
            {
                Object.Destroy(gameObject);
                return;
            }

            gameObject.SetActive(false);
            stack.Push(gameObject);
        }
    }
}
