using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Entities;
using Lineage.Debug;
using Lineage.Systems;
using Lineage.Components;
using Lineage.Database;

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

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Get main camera reference
            mainCamera = Camera.main;
            
            // Calculate spawn center from chunk or camera
            CalculateSpawnCenter();
            
            // Spawn initial population
            for (int i = 0; i < Mathf.Min(3, populationCap); i++)
            {
                SpawnPop();
            }

            OnPopulationChanged?.Invoke(currentPopulation);
            OnPopulationCapChanged?.Invoke(populationCap);
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
                cachedSpawnCenter.y = 0; // Ground level
                Log.Info($"Spawn center set to chunk position: {cachedSpawnCenter}", Log.LogCategory.Population);
                return;
            }
            
            // Priority 2: Auto-find Chunks parent object
            GameObject chunksObj = GameObject.Find("Chunks");
            if (chunksObj != null)
            {
                cachedSpawnCenter = chunksObj.transform.position;
                cachedSpawnCenter.y = 0;
                chunkTransform = chunksObj.transform; // Cache for future use
                Log.Info($"Spawn center auto-detected from Chunks object: {cachedSpawnCenter}", Log.LogCategory.Population);
                return;
            }
            
            // Priority 3: Auto-find initial chunk object
            GameObject chunkObj = GameObject.Find("initial chunk");
            if (chunkObj != null)
            {
                cachedSpawnCenter = chunkObj.transform.position;
                cachedSpawnCenter.y = 0;
                chunkTransform = chunkObj.transform; // Cache for future use
                Log.Info($"Spawn center auto-detected from initial chunk: {cachedSpawnCenter}", Log.LogCategory.Population);
                return;
            }
            
            // Priority 4: Use spawn point if assigned
            if (spawnPoint != null)
            {
                cachedSpawnCenter = spawnPoint.position;
                Log.Info($"Spawn center set to spawn point: {cachedSpawnCenter}", Log.LogCategory.Population);
                return;
            }
            
            // Priority 5: Calculate from camera view center
            if (mainCamera != null)
            {
                // Get the center of the camera's view at ground level (y=0)
                Vector3 cameraPos = mainCamera.transform.position;
                Vector3 viewCenter = new Vector3(cameraPos.x, 0, cameraPos.y); // Camera's X maps to world X, Camera's Y maps to world Z
                cachedSpawnCenter = viewCenter;
                Log.Info($"Spawn center calculated from camera view: {cachedSpawnCenter}", Log.LogCategory.Population);
                return;
            }
            
            // Fallback: Use world origin
            cachedSpawnCenter = Vector3.zero;
            Log.Warning("No spawn reference found, using world origin (0,0,0)", Log.LogCategory.Population);
        }

        private void Update()
        {
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
                if (pop.entityDataComponent != null && pop.entityDataComponent.HasCriticalNeeds())
                {
                    Log.Warning($"Pop {pop.name} died from critical needs!", Log.LogCategory.Population);
                    KillPop(pop);
                    continue;
                }

                // Consume food gradually
                float foodConsumption = ResourceManager.Instance.foodConsumptionRate * Time.deltaTime;
                if (ResourceManager.Instance.ConsumeFood(foodConsumption))
                {
                    // Pop eats, hunger decreases slowly
                    pop.EatFood(2f * Time.deltaTime);
                }
                else
                {
                    // No food available, pop gets hungrier faster
                    if (pop.entityDataComponent != null)
                    {
                        pop.entityDataComponent.ModifyStat(Stat.ID.Hunger, -5f * Time.deltaTime);
                    }
                }

                // Generate faith if needs are met
                if (pop.hunger > 30f && pop.thirst > 30f)
                {
                    ResourceManager.Instance.AddFaith(ResourceManager.Instance.faithGenerationRate * Time.deltaTime);
                }
            }
        }        public void SpawnPop()
        {
            if (currentPopulation >= populationCap || popPrefab == null) return;

            // Spawn around the calculated center position
            Vector3 spawnPos = cachedSpawnCenter + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                0, // Keep on ground level
                Random.Range(-spawnRadius, spawnRadius)
            );

            GameObject popObj = Instantiate(popPrefab, spawnPos, Quaternion.identity);
            
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
                pop.transform.localScale = Vector3.one * Random.Range(0.9f, 1.1f); // Randomize size slightly
                livingPops.Add(pop);
                currentPopulation++;
                OnPopulationChanged?.Invoke(currentPopulation);
                Log.Info($"New pop spawned: {pop.name} at {spawnPos}", Log.LogCategory.Population);
            }
        }

        public Pop SpawnPopAt(Vector3 position)
        {
            if (currentPopulation >= populationCap || popPrefab == null) return null;

            GameObject popObj = Instantiate(popPrefab, position, Quaternion.identity);
            
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
