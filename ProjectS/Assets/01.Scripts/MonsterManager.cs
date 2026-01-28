using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PS.Events;
using PS.Base;
using PS.AI.Boss; // Assuming the Boss script is in this namespace
using PS.Networking;

namespace PS.Manager
{
    public class MonsterManager : MonoBehaviour
    {
        [Header("Game Flow Events")]
        [SerializeField] private VoidEventChannelSO onStageStart;
        [SerializeField] private VoidEventChannelSO onEliteSpawn;
        [SerializeField] private VoidEventChannelSO onBossSpawn;
        [SerializeField] private VoidEventChannelSO onStageClear; // Added for stopping spawning

        [Header("Monster Spawning Settings")]
        [SerializeField] private List<GameObject> normalMonsterPrefabs;
        [SerializeField] private GameObject bossPrefab; // The boss prefab to spawn
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private int maxMonsters = 10; // TODO: Implement logic to track active monsters and respect this limit.
        [SerializeField] private float spawnRadius = 10f; // Radius around MonsterManager to spawn monsters
        [SerializeField] private float normalSpawnHeightOffset = 0.5f;
        [SerializeField] private float bossSpawnHeightOffset = 0.5f;

        private Coroutine _spawnCoroutine;
        private Boss _spawnedBoss; // Reference to the spawned boss instance
        private PunPrefabPool _prefabPool;

        private void Awake()
        {
            ConfigurePrefabPool();
        }

        private void OnEnable()
        {
            onStageStart.OnEventRaised += StartMonsterSpawning;
            onEliteSpawn.OnEventRaised += SpawnElite;
            onBossSpawn.OnEventRaised += SpawnBoss;
            onStageClear.OnEventRaised += StopMonsterSpawning;
        }

        private void OnDisable()
        {
            onStageStart.OnEventRaised -= StartMonsterSpawning;
            onEliteSpawn.OnEventRaised -= SpawnElite;
            onBossSpawn.OnEventRaised -= SpawnBoss;
            onStageClear.OnEventRaised -= StopMonsterSpawning;

            // Ensure we don't leave a dangling event subscription
            if (_spawnedBoss != null)
            {
                _spawnedBoss.OnDead -= HandleBossDeath;
            }
        }

        /// <summary>
        /// Starts the regular monster spawning process.
        /// Only executed on the Master Client.
        /// </summary>
        private void StartMonsterSpawning()
        {
            if (!IsMasterOrOffline())
            {
                return;
            }

            Log.D("[MonsterManager] Starting regular monster spawning.");
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
            }
            _spawnCoroutine = StartCoroutine(SpawnMonstersCoroutine());
        }

        /// <summary>
        /// Stops the regular monster spawning process.
        /// Only executed on the Master Client.
        /// </summary>
        private void StopMonsterSpawning()
        {
            if (!IsMasterOrOffline())
            {
                return;
            }

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
                Log.D("[MonsterManager] Regular monster spawning stopped.");
            }
        }

        /// <summary>
        /// Coroutine that continuously spawns normal monsters at intervals.
        /// Only runs on the Master Client.
        /// </summary>
        private IEnumerator SpawnMonstersCoroutine()
        {
            while (true)
            {
                if (normalMonsterPrefabs != null && normalMonsterPrefabs.Count > 0)
                {
                    // TODO: Implement logic to track active monster count and respect maxMonsters limit.
                    // For now, it spawns regardless of active count.

                    // Choose a random monster prefab
                    GameObject monsterPrefab = normalMonsterPrefabs[Random.Range(0, normalMonsterPrefabs.Count)];
                    
                    // Determine a spawn position within the defined radius
                    if (!TryGetSpawnCenter(out Vector3 centerPosition))
                    {
                        yield return new WaitForSeconds(spawnInterval);
                        continue;
                    }
                    Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
                    Vector3 spawnPosition = centerPosition + new Vector3(randomDirection.x, 0f, randomDirection.z);
                    spawnPosition = ResolveGroundPosition(spawnPosition, normalSpawnHeightOffset);

                    if (Photon.Pun.PhotonNetwork.InRoom)
                    {
                        if (!Photon.Pun.PhotonNetwork.IsMasterClient)
                        {
                            yield return new WaitForSeconds(spawnInterval);
                            continue;
                        }

                        if (Resources.Load<GameObject>(monsterPrefab.name) == null)
                        {
                            Log.E($"[MonsterManager] '{monsterPrefab.name}' is not in Resources. PhotonNetwork.Instantiate will fail.");
                        }
                        else
                        {
                            Photon.Pun.PhotonNetwork.Instantiate(monsterPrefab.name, spawnPosition, Quaternion.identity, 0);
                        }
                    }
                    else
                    {
                        Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
                    }

                    Log.D($"[MonsterManager] Spawned {monsterPrefab.name} at {spawnPosition}.");
                }
                else
                {
                    Log.W("[MonsterManager] No normalMonsterPrefabs assigned.");
                }
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        /// <summary>
        /// Placeholder method for spawning an elite monster.
        /// Only executed on the Master Client.
        /// </summary>
        private void SpawnElite()
        {
            if (!IsMasterOrOffline())
            {
                return;
            }

            Log.D("[MonsterManager] Received request to spawn ELITE monster.");
            // TODO: Implement elite monster spawning logic here.
            // Stop regular monster spawning when elite spawns? Or continue? Depends on design.
            // For now, regular spawning continues.
        }

        /// <summary>
        /// Spawns the boss monster, stops regular monster spawning, and subscribes to the boss's death event.
        /// Only executed on the Master Client.
        /// </summary>
        private void SpawnBoss()
        {
            if (!IsMasterOrOffline())
            {
                return;
            }

            Log.D("[MonsterManager] Received request to spawn BOSS monster.");
            StopMonsterSpawning(); // Stop regular monster spawning when boss appears

            if (bossPrefab == null)
            {
                Log.E("[MonsterManager] Boss Prefab is not assigned!");
                return;
            }

            // Spawn the boss at a designated point (e.g., the MonsterManager's position)
            // You might want a more specific spawn point.
            if (!TryGetSpawnCenter(out Vector3 bossCenter))
            {
                Log.W("[MonsterManager] Boss spawn skipped: no player found.");
                return;
            }

            Vector3 bossSpawnPosition = ResolveGroundPosition(bossCenter, bossSpawnHeightOffset);
            GameObject bossGO;
            if (Photon.Pun.PhotonNetwork.InRoom)
            {
                if (Resources.Load<GameObject>(bossPrefab.name) == null)
                {
                    Log.E($"[MonsterManager] '{bossPrefab.name}' is not in Resources. PhotonNetwork.Instantiate will fail.");
                    bossGO = null;
                }
                else
                {
                    bossGO = Photon.Pun.PhotonNetwork.Instantiate(bossPrefab.name, bossSpawnPosition, Quaternion.identity, 0);
                }
            }
            else
            {
                bossGO = Instantiate(bossPrefab, bossSpawnPosition, Quaternion.identity);
            }
            if (bossGO != null)
                _spawnedBoss = bossGO.GetComponent<Boss>();

            // --- TEMPORARY TEST CODE ---
            StartCoroutine(KillBossAfterDelay(3f));
            // -------------------------
            
            if (_spawnedBoss != null)
            {
                Log.D($"[MonsterManager] Boss {_spawnedBoss.name} spawned. Subscribing to OnDead event.");
                _spawnedBoss.OnDead += HandleBossDeath;

                // --- TEMPORARY TEST CODE ---
                StartCoroutine(KillBossAfterDelay(3f));
                // -------------------------
            }
            else
            {
                Log.E($"[MonsterManager] The spawned boss prefab '{bossPrefab.name}' does not have a 'Boss' component attached!");
            }
        }

        // --- TEMPORARY TEST CODE ---
        /// <summary>
        /// Coroutine to automatically kill the boss after a delay for testing purposes.
        /// </summary>
        private IEnumerator KillBossAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            Log.W("[MonsterManager] TEST: Simulating boss death after 3 seconds.");
            // Directly call the handler to simulate the OnDead event being invoked.
            HandleBossDeath();
            
            // Check if the boss hasn't been destroyed by other means already
            if (_spawnedBoss != null)
            {
                Log.W("[MonsterManager] TEST: Simulating boss death after 3 seconds.");
                // Directly call the handler to simulate the OnDead event being invoked.
                HandleBossDeath();
            }
        }
        // -------------------------

        /// <summary>
        /// Handles the logic for when the boss is defeated.
        /// It raises the OnStageClear event to signal the GameManager.
        /// </summary>
        private void HandleBossDeath()
        {
            Log.D("[MonsterManager] Boss has been defeated. Signaling Stage Clear.");

            // Unsubscribe to prevent any further calls
            if (_spawnedBoss != null)
            {
                _spawnedBoss.OnDead -= HandleBossDeath;
                _spawnedBoss = null;
            }
            
            // Raise the event to notify GameManager and other systems
            onStageClear?.RaiseEvent();
        }

        private bool IsMasterOrOffline()
        {
            return !Photon.Pun.PhotonNetwork.InRoom || Photon.Pun.PhotonNetwork.IsMasterClient;
        }

        private Vector3 ResolveGroundPosition(Vector3 basePosition, float heightOffset)
        {
            Vector3 rayOrigin = basePosition + Vector3.up * 50f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 100f, ~0, QueryTriggerInteraction.Ignore))
            {
                return hit.point + Vector3.up * Mathf.Max(0f, heightOffset);
            }

            return basePosition + Vector3.up * Mathf.Max(0f, heightOffset);
        }

        private bool TryGetSpawnCenter(out Vector3 center)
        {
            if (PlayerManager.LocalPlayerInstance != null)
            {
                center = PlayerManager.LocalPlayerInstance.transform.position;
                return true;
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                center = player.transform.position;
                return true;
            }

            center = transform.position;
            return false;
        }
        private void ConfigurePrefabPool()
        {
            if (_prefabPool != null) return;

            var existingPool = Photon.Pun.PhotonNetwork.PrefabPool as PunPrefabPool;
            if (existingPool != null)
            {
                _prefabPool = existingPool;
                _prefabPool.RegisterPrefabs(normalMonsterPrefabs);
                if (bossPrefab != null)
                {
                    _prefabPool.RegisterPrefab(bossPrefab);
                }
                return;
            }

            _prefabPool = new PunPrefabPool();
            _prefabPool.RegisterPrefabs(normalMonsterPrefabs);
            if (bossPrefab != null)
            {
                _prefabPool.RegisterPrefab(bossPrefab);
            }
            Photon.Pun.PhotonNetwork.PrefabPool = _prefabPool;
        }

    }
}
