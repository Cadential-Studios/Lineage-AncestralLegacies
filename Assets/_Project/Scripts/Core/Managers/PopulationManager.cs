using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Entities;
using Lineage.Debug;
using Lineage.Systems;
using Lineage.Components;

namespace Lineage.Managers
{
    /// <summary>
    /// Manages the population of Kaari, spawning, death, and population cap.
    /// </summary>
    public class PopulationManager : MonoBehaviour
    {
        public static PopulationManager Instance { get; private set; }

        [Header("Population Settings")]
        public int populationCap = 5;
        public int currentPopulation = 0;
        public GameObject popPrefab; // Assign in inspector

        [Header("Spawning")]
        public Transform spawnPoint; // Optional - will auto-detect chunk center if not set
        public float spawnRadius = 2f;
        public Transform chunkTransform; // Reference to the terrain chunk

        private Camera mainCamera;
        private Vector3 cachedSpawnCenter;


        private List<Pop> livingPops = new List<Pop>();

        // Events
        public System.Action<int> OnPopulationChanged;
        public System.Action<int> OnPopulationCapChanged;

        // Debug toggle - set to false to reduce console spam
        private const bool SPAWN_DEBUG = true;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                if (SPAWN_DEBUG) UnityEngine.Debug.Log($"[PopManager] Initialized. Prefab: {(popPrefab != null ? popPrefab.name : "NULL!")}");
            }
            else
            {
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // Auto-assign Pop prefab when component is added or reset
            if (popPrefab == null)
            {
                popPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Entities/Pop/Pop.prefab");
                if (popPrefab != null)
                {
                    UnityEngine.Debug.Log("PopulationManager: Auto-assigned Pop prefab");
                }
            }
        }
#endif

        private void Start()
        {
            // Validate prefab assignment
            if (popPrefab == null)
            {
                UnityEngine.Debug.LogError("[PopManager] CRITICAL: popPrefab is NULL! Assign in Inspector.");
                UnityEngine.Debug.Break();
                return;
            }

            mainCamera = Camera.main;
            CalculateSpawnCenter();

            if (SPAWN_DEBUG) UnityEngine.Debug.Log($"[PopManager] Spawning {Mathf.Min(3, populationCap)} pops at {cachedSpawnCenter}");

            // Spawn initial population
            for (int i = 0; i < Mathf.Min(3, populationCap); i++)
            {
                SpawnPop();
            }

            OnPopulationChanged?.Invoke(currentPopulation);
            OnPopulationCapChanged?.Invoke(populationCap);

            if (SPAWN_DEBUG) UnityEngine.Debug.Log($"[PopManager] ✓ Spawned {currentPopulation} pops");
        }

        /// <summary>
        /// Calculates the center of the visible chunk for spawning pops
        /// </summary>
        private void CalculateSpawnCenter()
        {
            // Priority 1: Use chunk transform if assigned
            if (chunkTransform != null)
            {
                cachedSpawnCenter = chunkTransform.position;
                cachedSpawnCenter.y = 0;
                return;
            }

            // Priority 2: Auto-find Chunks parent object
            GameObject chunksObj = GameObject.Find("Chunks");
            if (chunksObj != null)
            {
                cachedSpawnCenter = chunksObj.transform.position;
                cachedSpawnCenter.y = 0;
                chunkTransform = chunksObj.transform;
                return;
            }

            // Priority 3: Auto-find initial chunk object
            GameObject chunkObj = GameObject.Find("initial chunk");
            if (chunkObj != null)
            {
                cachedSpawnCenter = chunkObj.transform.position;
                cachedSpawnCenter.y = 0;
                chunkTransform = chunkObj.transform;
                return;
            }

            // Priority 4: Use spawn point if assigned
            if (spawnPoint != null)
            {
                cachedSpawnCenter = spawnPoint.position;
                return;
            }

            // Priority 5: Calculate from camera view center
            if (mainCamera != null)
            {
                Vector3 cameraPos = mainCamera.transform.position;
                cachedSpawnCenter = new Vector3(cameraPos.x, 0, cameraPos.z);
                return;
            }

            // Fallback: Use world origin
            cachedSpawnCenter = Vector3.zero;
            Log.Warning("No spawn reference found, using world origin (0,0,0)", Log.LogCategory.Population);
        }

        private float _initializationDelay = 0.5f; // Give pops time to initialize
        private bool _canProcessNeeds = false;

        private void Update()
        {
            // Wait for pops to fully initialize before processing needs
            if (!_canProcessNeeds)
            {
                _initializationDelay -= Time.deltaTime;
                if (_initializationDelay <= 0f)
                {
                    _canProcessNeeds = true;
                    if (SPAWN_DEBUG) UnityEngine.Debug.Log("[PopManager] Needs processing enabled");
                }
                return;
            }

            // Process population needs and death
            ProcessPopulationNeeds();
        }

        private void ProcessPopulationNeeds()
        {
            for (int i = livingPops.Count - 1; i >= 0; i--)
            {
                var pop = livingPops[i];
                if (pop == null)
                {
                    livingPops.RemoveAt(i);
                    currentPopulation--;
                    OnPopulationChanged?.Invoke(currentPopulation);
                    continue;
                }

                // Check if pop should die from starvation or critical needs
                // Skip if entityData hasn't been initialized yet (hunger/thirst would be 0)
                if (pop.entityDataComponent != null && pop.entityDataComponent.isInitialized)
                {
                    if (pop.entityDataComponent.HasCriticalNeeds())
                    {
                        Log.Warning($"Pop {pop.name} died from critical needs!", Log.LogCategory.Population);
                        KillPop(pop);
                        continue;
                    }
                }

                // Consume food gradually
                float foodConsumption = ResourceManager.Instance.foodConsumptionRate * Time.deltaTime;
                if (ResourceManager.Instance.ConsumeFood(foodConsumption))
                {
                    // Pop eats, hunger decreases slowly
                    pop.entityDataComponent?.EatFood(2f * Time.deltaTime);
                }
                else
                {
                    // No food available, pop gets hungrier faster
                    // EntityData modifications are not available at this time
                }

                // Generate faith if needs are met
                if (pop.hunger > 30f && pop.thirst > 30f)
                {
                    ResourceManager.Instance.AddFaith(ResourceManager.Instance.faithGenerationRate * Time.deltaTime);
                }
            }
        }

        public void SpawnPop()
        {
            if (popPrefab == null)
            {
                UnityEngine.Debug.LogError("[PopManager] SpawnPop FAILED: popPrefab is null!");
                UnityEngine.Debug.Break();
                return;
            }

            if (currentPopulation >= populationCap) return;

            // Spawn around the calculated center position, keeping Z consistent
            Vector3 spawnPos = cachedSpawnCenter + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0,
                0  // Keep Z at center Z position - no randomization
            );

            // Validate that spawn position is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            if (!UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                // If not on NavMesh, try to find nearest valid position
                if (SPAWN_DEBUG) UnityEngine.Debug.LogWarning($"[PopManager] Spawn position {spawnPos} not on NavMesh, searching for nearest valid position...");

                // Try larger search radius
                if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 20f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                    if (SPAWN_DEBUG) UnityEngine.Debug.Log($"[PopManager] Found valid NavMesh position at {spawnPos}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[PopManager] Could not find valid NavMesh position near {spawnPos}. Make sure NavMesh is baked!");
                    return;
                }
            }

            GameObject popObj = Instantiate(popPrefab, spawnPos, Quaternion.identity);

            if (popObj == null)
            {
                UnityEngine.Debug.LogError("[PopManager] Instantiate failed!");
                return;
            }

            // Parent to Pops container if it exists
            Transform popsContainer = GameObject.Find("Pops")?.transform;
            if (popsContainer != null)
            {
                popObj.transform.SetParent(popsContainer);
            }

            Pop pop = popObj.GetComponent<Pop>();

            if (pop != null)
            {
                pop.name = GenerateRandomName();
                pop.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f);
                livingPops.Add(pop);
                currentPopulation++;
                OnPopulationChanged?.Invoke(currentPopulation);

                if (SPAWN_DEBUG) UnityEngine.Debug.Log($"[Pop] ✓ {pop.name} spawned at {spawnPos}");
            }
            else
            {
                UnityEngine.Debug.LogError("[PopManager] Prefab missing Pop component!");
                Destroy(popObj);
            }
        }

        public Pop SpawnPopAt(Vector3 position)
        {
            if (currentPopulation >= populationCap || popPrefab == null) return null;

            // Validate spawn position is on NavMesh
            UnityEngine.AI.NavMeshHit hit;
            Vector3 validPosition = position;
            if (!UnityEngine.AI.NavMesh.SamplePosition(position, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            {
                if (UnityEngine.AI.NavMesh.SamplePosition(position, out hit, 20f, UnityEngine.AI.NavMesh.AllAreas))
                {
                    validPosition = hit.position;
                    UnityEngine.Debug.LogWarning($"[PopManager] Spawn position {position} adjusted to nearest NavMesh position {validPosition}");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[PopManager] Cannot spawn pop at {position} - no valid NavMesh nearby!");
                    return null;
                }
            }

            GameObject popObj = Instantiate(popPrefab, validPosition, Quaternion.identity);

            // Parent to Pops container if it exists
            Transform popsContainer = GameObject.Find("Pops")?.transform;
            if (popsContainer != null)
            {
                popObj.transform.SetParent(popsContainer);
            }

            Pop pop = popObj.GetComponent<Pop>();

            if (pop != null)
            {
                pop.name = GenerateRandomName();
                livingPops.Add(pop);
                currentPopulation++;
                OnPopulationChanged?.Invoke(currentPopulation);
                Log.Info($"New pop spawned at {position}: {pop.name}", Log.LogCategory.Population);
            }

            return pop;
        }

        public void KillPop(Pop pop)
        {
            if (livingPops.Contains(pop))
            {
                livingPops.Remove(pop);
                currentPopulation--;
                OnPopulationChanged?.Invoke(currentPopulation);

                if (pop.gameObject != null)
                    Destroy(pop.gameObject);
            }
        }

        public void OnPopDied(Pop pop)
        {
            // Called when a pop dies naturally (not killed by manager)
            if (livingPops.Contains(pop))
            {
                livingPops.Remove(pop);
                currentPopulation--;
                OnPopulationChanged?.Invoke(currentPopulation);
                Log.Info($"Pop {pop.name} died naturally. Population: {currentPopulation}", Log.LogCategory.Population);
            }
        }

        public bool ImproveShelter(float faithCost = 10f)
        {
            if (ResourceManager.Instance.ConsumeFaith(faithCost))
            {
                populationCap++;
                OnPopulationCapChanged?.Invoke(populationCap);
                Log.Info($"Shelter improved! Population cap increased to {populationCap}", Log.LogCategory.Population);
                return true;
            }
            return false;
        }

        private string GenerateRandomName()
        {
            string[] names = { "Kael", "Mira", "Thane", "Zara", "Bren", "Lyra", "Dak", "Nira", "Vor", "Tessa" };
            return names[Random.Range(0, names.Length)];
        }

        public List<Pop> GetLivingPops()
        {
            return new List<Pop>(livingPops);
        }
    }
}
