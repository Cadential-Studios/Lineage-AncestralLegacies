using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lineage.Entities;
using Lineage.Debug;
using Lineage.Systems;
using Lineage.Components;
using Lineage.Core;

namespace Lineage.Managers
{
    /// <summary>
    /// Enhanced PopulationManager that uses GameData system for richer Pop generation.
    /// Supports genetic inheritance, traits, and dynamic stat generation.
    /// </summary>
    public class EnhancedPopulationManager : MonoBehaviour
    {
        public static EnhancedPopulationManager Instance { get; private set; }

        [Header("Population Settings")]
        public int populationCap = 20;
        public int currentPopulation = 0;
        public GameObject popPrefab;

        [Header("Spawning")]
        public Transform spawnPoint;
        public float spawnRadius = 2f;

        [Header("GameData Integration")]
        public bool useGameDataSystem = true;
        public bool enableGeneticInheritance = true;
        public bool enableRandomTraits = true;

        [Header("Breeding Settings")]
        public float breedingChance = 0.1f; // Chance per update cycle
        public float minBreedingAge = 18f;
        public float maxBreedingAge = 45f;

        private List<Pop> livingPops = new List<Pop>();
        private List<EntityDataComponent> entityComponents = new List<EntityDataComponent>();

        // Events
        public System.Action<int> OnPopulationChanged;
        public System.Action<int> OnPopulationCapChanged;
        public System.Action<Pop, Pop, Pop> OnPopBorn; // Parent A, Parent B, Child

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // Initialize GameData if not already done
                if (useGameDataSystem)
                {
                    _ = GameDataManager.Instance; // triggers loading on first access
                }
            }
            else
            {
                Destroy(gameObject);
            }

            // todo: probably dont need this because we have GameData and it will handle initialization
        }

        private void Start()
        {
            // Spawn initial population
            for (int i = 0; i < Mathf.Min(3, populationCap); i++)
            {
                SpawnPop();
            }

            OnPopulationChanged?.Invoke(currentPopulation);
            OnPopulationCapChanged?.Invoke(populationCap);
        }

        private void Update()
        {
            ProcessPopulationNeeds();

            if (enableGeneticInheritance && currentPopulation < populationCap)
            {
                ProcessBreeding();
            }
        }

        private void ProcessPopulationNeeds()
        {
            for (int i = livingPops.Count - 1; i >= 0; i--)
            {
                var pop = livingPops[i];
                if (pop == null)
                {
                    livingPops.RemoveAt(i);
                    if (i < entityComponents.Count)
                        entityComponents.RemoveAt(i);
                    currentPopulation--;
                    OnPopulationChanged?.Invoke(currentPopulation);
                    continue;
                }

                // Use EntityDataComponent if available for richer behavior
                EntityDataComponent entityData = i < entityComponents.Count ? entityComponents[i] : null;

                if (entityData != null && useGameDataSystem)
                {
                    ProcessGameDataNeeds(pop, entityData);
                }
                else
                {
                    ProcessBasicNeeds(pop);
                }
            }
        }

        private void ProcessGameDataNeeds(Pop pop, EntityDataComponent entityData)
        {
            // GUARD: Skip uninitialized entities
            if (entityData == null || !entityData.isInitialized)
            {
                return;
            }

            // TODO: Implement state-based effects and stat modifications
            // This method is a stub pending full EntityDataComponent API implementation
        }

        private void ProcessBasicNeeds(Pop pop)
        {
            // Fallback to basic needs system
            // Check if pop has NeedsComponent for hunger/thirst
            var needsComponent = pop.GetComponent<Lineage.Systems.Needs.NeedsComponent>();

            if (needsComponent != null)
            {
                // Use the existing needs system
                if (needsComponent.hunger <= 0f)
                {
                    Log.Warning($"Pop {pop.name} died of starvation!", Log.LogCategory.Population);
                    KillPop(pop);
                    return;
                }

                // Generate resources based on basic needs
                if (needsComponent.hunger > 30f && needsComponent.thirst > 30f)
                {
                    if (ResourceManager.Instance != null)
                        ResourceManager.Instance.AddFaith(ResourceManager.Instance.faithGenerationRate * Time.deltaTime);
                }

                ResourceManager.Instance?.AddFood(0.5f * Time.deltaTime);
            }
        }

        private void ProcessBreeding()
        {
            if (Random.Range(0f, 1f) > breedingChance * Time.deltaTime) return;

            // Find suitable breeding pairs
            var breedingCandidates = GetBreedingCandidates();

            if (breedingCandidates.Count >= 2)
            {
                var parentA = breedingCandidates[Random.Range(0, breedingCandidates.Count)];
                var parentB = breedingCandidates[Random.Range(0, breedingCandidates.Count)];

            }
        }

        private List<Pop> GetBreedingCandidates()
        {
            var candidates = new List<Pop>();

            for (int i = 0; i < livingPops.Count; i++)
            {
                var pop = livingPops[i];
                if (pop == null) continue;

                // Check age (you'll need to implement age tracking)
                float age = pop.age; // Assuming this exists in Pop class

                if (age >= minBreedingAge && age <= maxBreedingAge)
                {
                    // Additional checks could include health, relationship status, etc.
                    if (useGameDataSystem && i < entityComponents.Count)
                    {
                        var entityData = entityComponents[i];
                        if (entityData != null && entityData.IsHealthy())
                        {
                            candidates.Add(pop);
                        }
                    }
                    else
                    {
                        candidates.Add(pop);
                    }
                }
            }

            return candidates;
        }

        // TODO: Implement state change system when EntityDataComponent API is stable
        // private void ChangeToRandomState(EntityDataComponent entityData) { }


        /// <summary>
        /// Spawns a new Pop entity using either the GameData system or basic instantiation.
        /// </summary>
        public void SpawnPop()
        {
            if (currentPopulation >= populationCap || popPrefab == null) return;

            // Generate spawn position around spawn point, keeping Z consistent
            Vector3 basePos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius; // Only randomize X and Y
            Vector3 spawnPos = new Vector3(
                basePos.x + randomOffset.x,
                basePos.y,
                basePos.z  // Keep Z consistent
            );

            GameObject popObj = Instantiate(popPrefab, spawnPos, Quaternion.identity);
            Pop pop = popObj.GetComponent<Pop>();

            if (pop == null)
            {
                Log.Error($"PopPrefab is missing Pop component!", Log.LogCategory.Population);
                Destroy(popObj);
                return;
            }

            pop.name = GenerateRandomName();

            if (useGameDataSystem)
            {
                // Apply GameData-based configuration
                var entityComponent = pop.GetComponent<EntityDataComponent>();
                if (entityComponent == null)
                {
                    entityComponent = popObj.AddComponent<EntityDataComponent>();
                }

                // Try to get entity definition from GameDataManager
                var entityDef = GameDataBridge.TryGetEntityDefinition("ENTITY_POP_GENERIC");
                if (entityDef != null)
                {
                    // Apply definition stats to the entity data component
                    // TODO: InitializeEntityFromDefinition commented out due to namespace issues
                    // InitializeEntityFromDefinition(entityComponent, entityDef);
                }

                // Apply random traits if enabled
                if (enableRandomTraits)
                {
                    // TODO: ApplyRandomTraits commented out due to namespace issues
                    // ApplyRandomTraits(entityComponent);
                }

                entityComponents.Add(entityComponent);
            }

            livingPops.Add(pop);
            currentPopulation++;
            OnPopulationChanged?.Invoke(currentPopulation);
            Log.Info($"New pop spawned: {pop.name}", Log.LogCategory.Population);
        }

        // TODO: Implement stat initialization when EntityDataComponent API is stable
        // private void InitializeEntityFromDefinition(EntityDataComponent component, Core.Entities.EntityDefinitionSO definition) { }


        // TODO: Implement trait variance system when EntityDataComponent API is stable
        // private void ApplyRandomTraits(EntityDataComponent component) { }


        // TODO: Implement entity data retrieval when API is stable
        // private Lineage.Database.Entity GetEntityDataForPop(Pop pop) { return default; }


        public void RegisterPop(Pop pop)
        {
            livingPops.Add(pop);

            // Add EntityDataComponent if using GameData system
            var entityComponent = pop.GetComponent<EntityDataComponent>();
            entityComponents.Add(entityComponent);

            currentPopulation++;
            OnPopulationChanged?.Invoke(currentPopulation);
        }

        public void KillPop(Pop pop)
        {
            if (pop == null) return;

            int index = livingPops.IndexOf(pop);
            if (index >= 0)
            {
                livingPops.RemoveAt(index);
                if (index < entityComponents.Count)
                    entityComponents.RemoveAt(index);

                currentPopulation--;
                OnPopulationChanged?.Invoke(currentPopulation);

                Log.Info($"Pop died: {pop.name}", Log.LogCategory.Population);
                Destroy(pop.gameObject);
            }
        }

        private string GenerateRandomName()
        {
            string[] names = { "Aelyn", "Baris", "Cira", "Daven", "Elyn", "Fynn", "Gara", "Hael", "Ira", "Jax" };
            return names[Random.Range(0, names.Length)];
        }

        public List<Pop> GetAllPops() => new List<Pop>(livingPops);
        public List<EntityDataComponent> GetAllEntityData() => new List<EntityDataComponent>(entityComponents);
    }
}

