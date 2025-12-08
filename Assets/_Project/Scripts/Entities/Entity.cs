using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System;
using Lineage.Database;
using Lineage.Systems.Inventory;
using Lineage.Core;
using Lineage.Core.Entities;
using Lineage.Systems.Movement;

#pragma warning disable 0414

namespace Lineage.Entities
{
    /// <summary>
    /// Universal Entity component that handles all entity types and their data management.
    /// Combines entity behavior, stat management, and type-specific configuration.
    /// </summary>
    public class Entity : MonoBehaviour
    {
        [Header("Entity Configuration")]
        [SerializeField] private int entityID = 0;
        [SerializeField] private EntityTypeData entityTypeData;

        [Header("Entity Data")]
        [SerializeField] private Database.Entity _entityData;

        [Header("Identity")]
        [SerializeField] private string _entityName = "Unknown Entity";
        [SerializeField] private int _entityAge = 0;

        [Header("Base Stats")]
        [SerializeField] private float _entityHunger = 50f;
        [SerializeField] private float _entityThirst = 50f;
        [SerializeField] private float _entityEnergy = 50f;
        [SerializeField] private float _entitySpeed = 5f;
        [SerializeField] private float _entityHealth = 100f;
        [SerializeField] private float _entityMana = 50f;

        [Header("Max Stat Values")]
        [SerializeField] private float _entityMaxHunger = 100f;
        [SerializeField] private float _entityMaxThirst = 100f;
        [SerializeField] private float _entityMaxEnergy = 100f;
        [SerializeField] private float _entityMaxSpeed = 10f;
        [SerializeField] private float _entityMaxHealth = 100f;
        [SerializeField] private float _entityMaxMana = 100f;

        [Header("Ability Stats")]
        [SerializeField] private float _entityStrength = 10f;
        [SerializeField] private float _entityAgility = 10f;
        [SerializeField] private float _entityIntelligence = 10f;
        [SerializeField] private float _entityDefense = 10f;
        [SerializeField] private float _entityLuck = 5f;
        [SerializeField] private float _entityCharisma = 5f;

        [Header("Combat Stats")]
        [SerializeField] private float _entityAttack = 10f;
        [SerializeField] private float _entityMagicPower = 10f;
        [SerializeField] private float _entityMagicDefense = 10f;
        [SerializeField] private float _entityCriticalHitChance = 5f;
        [SerializeField] private float _entityCriticalHitDamage = 150f;

        [Header("Traits")]
        [SerializeField] private List<string> _entityTraits = new List<string>();

        [Header("Crafting")]
        [SerializeField] private bool _entityCanCraft = false;
        [SerializeField] private List<string> _craftingRecipes = new List<string>();
        [Header("Needs System Configuration")]
        [SerializeField] private bool enableNeedsDecay = true;
        [SerializeField] private float hungerDecayRate = 1f;
        [SerializeField] private float thirstDecayRate = 1.5f;
        [SerializeField] private float energyDecayRate = 0.8f;
        [SerializeField] private float restDecayRate = 0.5f;

        [Header("Behavior Tree Integration")]
        [SerializeField] private bool enableBehaviorTrees = true;
        [SerializeField] private string currentBehaviorState = "Idle";

        [Header("Blackboard Variables")]
        [SerializeField] private Dictionary<string, object> blackboardVariables = new Dictionary<string, object>();

        [Header("Enhanced Resource Tagging")]
        [SerializeField] private List<string> resourceTags = new List<string>();
        [SerializeField] private List<string> behaviorSubtags = new List<string>();

        [Header("Runtime State")]
        public bool isInitialized = false;
        // Enhanced Events
        public System.Action<Database.Entity> OnEntityDataChanged;
        public System.Action<Stat.ID, float> OnStatChanged;
        public System.Action<State.ID> OnStateChanged;
        public System.Action<string> OnBehaviorStateChanged;
        public System.Action<string, object> OnBlackboardVariableChanged;
        public System.Action<List<string>> OnResourceTagsChanged;

        // Component references
        private IMovementController movementController;
        private NavMeshAgent navAgent;
        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private InventoryComponent inventoryComponent;

        // Behavior tree component reference (if available)
        private Component behaviorTreeComponent;

        // Properties
        public int EntityID
        {
            get => entityID;
            set => entityID = value;
        }

        public string EntityName
        {
            get => _entityName;
            set => _entityName = value;
        }

        public int Age
        {
            get => _entityAge;
            set => _entityAge = value;
        }

        public Database.Entity EntityData
        {
            get => _entityData;
            set
            {
                _entityData = value;
                OnEntityDataChanged?.Invoke(_entityData);
                isInitialized = true;
            }
        }
        public EntityTypeData TypeData
        {
            get => entityTypeData;
            set => entityTypeData = value;
        }

        // Enhanced Properties for Behavior System
        public string CurrentBehaviorState
        {
            get => currentBehaviorState;
            set
            {
                string oldState = currentBehaviorState;
                currentBehaviorState = value;
                if (oldState != value)
                {
                    OnBehaviorStateChanged?.Invoke(value);
                }
            }
        }

        public Dictionary<string, object> BlackboardVariables => blackboardVariables;
        public List<string> ResourceTags => resourceTags;
        public List<string> BehaviorSubtags => behaviorSubtags;

        // Stat properties (read-only, use ModifyStat to change)
        public float Health => GetStat(Stat.ID.Health).currentValue;
        public float MaxHealth => GetStat(Stat.ID.Health).maxValue;
        public float Mana => GetStat(Stat.ID.Mana).currentValue;
        public float MaxMana => GetStat(Stat.ID.Mana).maxValue;
        public float Hunger => GetStat(Stat.ID.Hunger).currentValue;
        public float Thirst => GetStat(Stat.ID.Thirst).currentValue;
        public float Energy => GetStat(Stat.ID.Energy).currentValue;
        public float Stamina => GetStat(Stat.ID.Stamina).currentValue;

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
            InitializeEntityData();
            InitializeBehaviorSystem();
        }
        private void Start()
        {
            if (entityTypeData != null)
            {
                entityTypeData.OnInitialize(this);
            }

            LoadFromDatabase();
            UpdateVisuals();
            SetupBlackboardVariables();
        }
        private void Update()
        {
            if (!isInitialized) return;

            // Update type-specific behavior
            if (entityTypeData != null)
            {
                entityTypeData.OnUpdate(this);
            }

            // Update needs decay if enabled
            if (enableNeedsDecay)
            {
                UpdateNeedsDecay();
            }

            // Update behavior system
            if (enableBehaviorTrees)
            {
                UpdateBehaviorSystem();
            }
        }

        private void OnDestroy()
        {
            if (entityTypeData != null)
            {
                // entityTypeData.OnEntityDestroy(this); // If needed
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponents()
        {
            // Try to get IMovementController first (supports multiple implementations)
            movementController = GetComponent<IMovementController>();

            // Fall back to NavMeshAgent if no movement controller exists
            if (movementController == null)
            {
                navAgent = GetComponent<NavMeshAgent>();
            }

            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            inventoryComponent = GetComponent<InventoryComponent>();
        }

        private void InitializeEntityData()
        {
            if (_entityData.entityID == 0 && entityID != 0)
            {
                LoadFromDatabase();
            }
        }
        private void LoadFromDatabase()
        {
            if (entityID == 0) return;

            // Prefer new ScriptableObject based GameData if available
            var entityDef = GameDataManager.Instance.GetEntityDefinition(entityID.ToString());
            if (entityDef != null)
            {
                UnityEngine.Debug.Log($"Loaded entity definition {entityDef.displayName} via GameDataManager");
                _entityData = new Database.Entity(entityDef.displayName, (Database.Entity.ID)entityID, description: entityDef.description);
                SyncSerializedFields();
                return;
            }

            UnityEngine.Debug.LogWarning($"Entity definition for ID {entityID} not found in GameData");
        }
        private void SyncSerializedFields()
        {
            // Sync serialized fields with entity data
            _entityName = _entityData.entityName;
            _entityAge = _entityData.level; // Assuming level represents age

            // Sync stats
            _entityHealth = _entityData.health.current;
            _entityMana = _entityData.mana.currentValue;
            _entityHunger = _entityData.hunger.currentValue;
            _entityThirst = _entityData.thirst.currentValue;
            _entityEnergy = _entityData.energy.currentValue;

            // Sync max values
            _entityMaxHealth = _entityData.health.max;
            _entityMaxMana = _entityData.mana.maxValue;
            _entityMaxHunger = _entityData.hunger.maxValue;
            _entityMaxThirst = _entityData.thirst.maxValue;
            _entityMaxEnergy = _entityData.energy.maxValue;

            // Sync ability stats
            _entityStrength = _entityData.strength.currentValue;
            _entityAgility = _entityData.agility.currentValue;
            _entityIntelligence = _entityData.intelligence.currentValue;
            _entityDefense = _entityData.defense.currentValue;
            _entityLuck = _entityData.luck.currentValue;
            _entityCharisma = _entityData.charisma.currentValue;

            // Sync combat stats
            _entityAttack = _entityData.attack.currentValue;
            _entityMagicPower = _entityData.magicPower.currentValue;
            _entityMagicDefense = _entityData.magicDefense.currentValue;
            _entityCriticalHitChance = _entityData.criticalHitChance.currentValue;
            _entityCriticalHitDamage = _entityData.criticalHitDamage.currentValue;
        }

        #endregion

        #region Stat Management
        /// <summary>
        /// Gets a stat by ID from the entity data.
        /// </summary>
        public Stat.StatValue GetStat(Stat.ID statID)
        {
            // Convert Database.Stat to Lineage.Entities.Stat.StatValue
            Database.Stat dbStat;

            switch (statID)
            {
                case Stat.ID.Health:
                    return new Stat.StatValue(_entityData.health.max, _entityData.health.current);

                case Stat.ID.Mana:
                    dbStat = _entityData.mana;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Stamina:
                    dbStat = _entityData.stamina;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Strength:
                    dbStat = _entityData.strength;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Agility:
                    dbStat = _entityData.agility;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Intelligence:
                    dbStat = _entityData.intelligence;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Defense:
                    dbStat = _entityData.defense;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Speed:
                    dbStat = _entityData.speed;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Attack:
                    dbStat = _entityData.attack;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.MagicPower:
                    dbStat = _entityData.magicPower;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.MagicDefense:
                    dbStat = _entityData.magicDefense;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.CriticalHitChance:
                    dbStat = _entityData.criticalHitChance;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.CriticalHitDamage:
                    dbStat = _entityData.criticalHitDamage;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Luck:
                    dbStat = _entityData.luck;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Charisma:
                    dbStat = _entityData.charisma;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Hunger:
                    dbStat = _entityData.hunger;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Thirst:
                    dbStat = _entityData.thirst;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);
                case Stat.ID.Energy:
                    dbStat = _entityData.energy;
                    return new Stat.StatValue(dbStat.baseValue, dbStat.currentValue);

                default:
                    UnityEngine.Debug.LogWarning($"Stat {statID} not found for entity {EntityName}");
                    return new Stat.StatValue(); // Return default stat value
            }
        }

        /// <summary>
        /// Modifies a stat by the given amount.
        /// </summary>
        public void ModifyStat(Stat.ID statID, float amount)
        {
            var currentStat = GetStat(statID);
            float oldValue = currentStat.currentValue;
            float newValue = Mathf.Clamp(oldValue + amount, 0f, currentStat.maxValue);

            switch (statID)
            {
                case Stat.ID.Health:
                    if (amount > 0)
                        _entityData.health = _entityData.health.Heal(amount);
                    else
                        _entityData.health = _entityData.health.TakeDamage(-amount);
                    break;

                case Stat.ID.Mana:
                    _entityData.mana.currentValue = newValue;
                    break;
                case Stat.ID.Stamina:
                    _entityData.stamina.currentValue = newValue;
                    break;
                case Stat.ID.Strength:
                    _entityData.strength.currentValue = newValue;
                    break;
                case Stat.ID.Agility:
                    _entityData.agility.currentValue = newValue;
                    break;
                case Stat.ID.Intelligence:
                    _entityData.intelligence.currentValue = newValue;
                    break;
                case Stat.ID.Defense:
                    _entityData.defense.currentValue = newValue;
                    break;
                case Stat.ID.Speed:
                    _entityData.speed.currentValue = newValue;
                    break;
                case Stat.ID.Attack:
                    _entityData.attack.currentValue = newValue;
                    break;
                case Stat.ID.MagicPower:
                    _entityData.magicPower.currentValue = newValue;
                    break;
                case Stat.ID.MagicDefense:
                    _entityData.magicDefense.currentValue = newValue;
                    break;
                case Stat.ID.CriticalHitChance:
                    _entityData.criticalHitChance.currentValue = newValue;
                    break;
                case Stat.ID.CriticalHitDamage:
                    _entityData.criticalHitDamage.currentValue = newValue;
                    break;
                case Stat.ID.Luck:
                    _entityData.luck.currentValue = newValue;
                    break;
                case Stat.ID.Charisma:
                    _entityData.charisma.currentValue = newValue;
                    break;
                case Stat.ID.Hunger:
                    _entityData.hunger.currentValue = newValue;
                    break;
                case Stat.ID.Thirst:
                    _entityData.thirst.currentValue = newValue;
                    break;
                case Stat.ID.Energy:
                    _entityData.energy.currentValue = newValue;
                    break;
                // Note: Rest, Experience, and Level stats not mapped in Lineage.Entities.Stat.ID
                // These exist in Database.Stat.ID but not in the simplified Stat enum

                default:
                    UnityEngine.Debug.LogWarning($"Cannot modify stat {statID} - not implemented");
                    return;
            }

            // Sync serialized fields
            SyncSerializedFields();

            // Fire events
            OnStatChanged?.Invoke(statID, newValue);

            // Notify type data
            if (entityTypeData != null)
            {
                entityTypeData.OnStatChanged(this, statID, newValue);
            }
        }

        #endregion

        #region State Management

        public void ChangeState(State.ID newState)
        {
            // Implementation for state changes
            OnStateChanged?.Invoke(newState);

            if (entityTypeData != null)
            {
                entityTypeData.OnStateChanged(this, newState);
            }
        }

        #endregion

        #region Needs Management

        private void UpdateNeedsDecay()
        {
            //todo: Implement a boolean to check if needs decay is enabled for this entity
            // Example: if (!enableNeedsDecay) return;
            if (Time.time % 1f < Time.deltaTime) // Every second
            {
                ModifyStat(Stat.ID.Hunger, -hungerDecayRate * Time.deltaTime);
                ModifyStat(Stat.ID.Thirst, -thirstDecayRate * Time.deltaTime);
                ModifyStat(Stat.ID.Energy, -energyDecayRate * Time.deltaTime);
                // Rest stat not available in Lineage.Entities.Stat.ID
            }
        }

        #endregion

        #region Visual Updates

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                // Update sprite based on entity data
                // Implementation depends on your sprite system
            }

            if (animator != null)
            {
                // Update animator parameters
                // Implementation depends on your animation system
            }
        }

        #endregion

        #region Configuration Methods

        /// <summary>
        /// Configure this entity with the given ID and type data
        /// </summary>
        public void ConfigureAsEntity(int id, EntityTypeData typeData)
        {
            EntityID = id;
            TypeData = typeData;
            LoadFromDatabase();
        }

        /// <summary>
        /// Change the entity type data (for dynamic type changes)
        /// </summary>
        public void ChangeEntityType(EntityTypeData newTypeData)
        {
            if (entityTypeData != null)
            {
                // Clean up old type data if needed
            }

            entityTypeData = newTypeData;

            if (newTypeData != null)
            {
                newTypeData.OnInitialize(this);
            }
        }

        #endregion

        #region Movement (Abstracted from specific implementation)

        /// <summary>
        /// Move the entity to a destination.
        /// Works with any IMovementController implementation or NavMeshAgent.
        /// </summary>
        public void MoveTo(Vector3 destination)
        {
            if (movementController != null && movementController.IsEnabled)
            {
                movementController.SetDestination(destination);
            }
            else if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                // Fallback to NavMeshAgent if no IMovementController
                navAgent.SetDestination(destination);
            }
        }

        /// <summary>
        /// Stop entity movement
        /// </summary>
        public void StopMovement()
        {
            if (movementController != null && movementController.IsEnabled)
            {
                movementController.Stop();
            }
            else if (navAgent != null && navAgent.isActiveAndEnabled)
            {
                navAgent.ResetPath();
            }
        }

        /// <summary>
        /// Check if entity is currently moving
        /// </summary>
        public bool IsMoving
        {
            get
            {
                if (movementController != null && movementController.IsEnabled)
                    return movementController.IsMoving;

                if (navAgent != null && navAgent.isActiveAndEnabled)
                    return navAgent.hasPath && navAgent.remainingDistance > 0.1f;

                return false;
            }
        }

        #endregion

        #region Behavior Tree Integration

        /// <summary>
        /// Initialize behavior system components
        /// </summary>
        private void InitializeBehaviorSystem()
        {
            // Try to find behavior tree component using reflection to avoid hard dependency
            var behaviorAuthoring = GetComponent("BehaviorAuthoring");
            if (behaviorAuthoring != null)
            {
                behaviorTreeComponent = behaviorAuthoring;
            }
        }

        /// <summary>
        /// Update behavior tree system
        /// </summary>
        private void UpdateBehaviorSystem()
        {
            if (behaviorTreeComponent != null)
            {
                // Update blackboard variables with current entity state
                UpdateBlackboardVariables();
            }
        }

        /// <summary>
        /// Set a blackboard variable for behavior tree integration
        /// </summary>
        public void SetBlackboardVariable(string key, object value)
        {
            if (blackboardVariables.ContainsKey(key))
            {
                blackboardVariables[key] = value;
            }
            else
            {
                blackboardVariables.Add(key, value);
            }

            OnBlackboardVariableChanged?.Invoke(key, value);
        }

        /// <summary>
        /// Get a blackboard variable
        /// </summary>
        public T GetBlackboardVariable<T>(string key, T defaultValue = default(T))
        {
            if (blackboardVariables.ContainsKey(key) && blackboardVariables[key] is T)
            {
                return (T)blackboardVariables[key];
            }
            return defaultValue;
        }

        /// <summary>
        /// Update blackboard variables with current entity state
        /// </summary>
        private void UpdateBlackboardVariables()
        {
            if (blackboardVariables != null)
            {
                SetBlackboardVariable("Health", Health);
                SetBlackboardVariable("Hunger", Hunger);
                SetBlackboardVariable("Thirst", Thirst);
                SetBlackboardVariable("Energy", Energy);
                SetBlackboardVariable("CurrentState", currentBehaviorState);
                SetBlackboardVariable("IsMoving", IsMoving);

                if (navAgent != null)
                {
                    SetBlackboardVariable("AgentPosition", transform.position);
                    SetBlackboardVariable("AgentVelocity", navAgent.velocity);
                }
            }
        }

        /// <summary>
        /// Setup initial blackboard variables
        /// </summary>
        private void SetupBlackboardVariables()
        {
            if (entityTypeData != null)
            {
                // Initialize core blackboard variables
                SetBlackboardVariable("EntityType", entityTypeData.entityType.ToString());
                SetBlackboardVariable("EntityID", entityID);
                SetBlackboardVariable("EntityName", EntityName);
                SetBlackboardVariable("MaxHealth", MaxHealth);
                SetBlackboardVariable("MaxMana", MaxMana);

                // Add behavior subtags to blackboard
                if (behaviorSubtags.Count > 0)
                {
                    SetBlackboardVariable("BehaviorSubtags", behaviorSubtags);

                    // Set individual subtype flags for behavior trees
                    SetBlackboardVariable("IsCarnivore", behaviorSubtags.Contains("carnivore"));
                    SetBlackboardVariable("IsHerbivore", behaviorSubtags.Contains("herbivore"));
                    SetBlackboardVariable("IsWolf", behaviorSubtags.Contains("wolf"));
                    SetBlackboardVariable("IsBandit", behaviorSubtags.Contains("bandit"));
                }

                // Add resource tags to blackboard
                if (resourceTags.Count > 0)
                {
                    SetBlackboardVariable("ResourceTags", resourceTags);

                    // Set individual resource flags for behavior trees
                    SetBlackboardVariable("HasFood", resourceTags.Contains("Food"));
                    SetBlackboardVariable("HasWater", resourceTags.Contains("Water"));
                    SetBlackboardVariable("HasCleanWater", resourceTags.Contains("Water.Clean"));
                    SetBlackboardVariable("HasDirtyWater", resourceTags.Contains("Water.Dirty"));
                    SetBlackboardVariable("IsGatherable", resourceTags.Contains("Gatherable"));
                }
            }
        }

        #endregion

        #region Enhanced Resource Management

        /// <summary>
        /// Add a resource tag to this entity
        /// </summary>
        public void AddResourceTag(string tag)
        {
            if (!resourceTags.Contains(tag))
            {
                resourceTags.Add(tag);
                OnResourceTagsChanged?.Invoke(resourceTags);

                // Update blackboard if behavior trees are enabled
                if (enableBehaviorTrees)
                {
                    SetBlackboardVariable("ResourceTags", resourceTags);
                    UpdateResourceFlags();
                }
            }
        }

        /// <summary>
        /// Remove a resource tag from this entity
        /// </summary>
        public void RemoveResourceTag(string tag)
        {
            if (resourceTags.Remove(tag))
            {
                OnResourceTagsChanged?.Invoke(resourceTags);

                // Update blackboard if behavior trees are enabled
                if (enableBehaviorTrees)
                {
                    SetBlackboardVariable("ResourceTags", resourceTags);
                    UpdateResourceFlags();
                }
            }
        }

        /// <summary>
        /// Check if entity has a specific resource tag
        /// </summary>
        public bool HasResourceTag(string tag)
        {
            return resourceTags.Contains(tag);
        }

        /// <summary>
        /// Add a behavior subtag to this entity
        /// </summary>
        public void AddBehaviorSubtag(string subtag)
        {
            if (!behaviorSubtags.Contains(subtag))
            {
                behaviorSubtags.Add(subtag);

                // Update blackboard if behavior trees are enabled
                if (enableBehaviorTrees)
                {
                    SetBlackboardVariable("BehaviorSubtags", behaviorSubtags);
                    UpdateBehaviorFlags();
                }
            }
        }

        /// <summary>
        /// Remove a behavior subtag from this entity
        /// </summary>
        public void RemoveBehaviorSubtag(string subtag)
        {
            if (behaviorSubtags.Remove(subtag))
            {
                // Update blackboard if behavior trees are enabled
                if (enableBehaviorTrees)
                {
                    SetBlackboardVariable("BehaviorSubtags", behaviorSubtags);
                    UpdateBehaviorFlags();
                }
            }
        }

        /// <summary>
        /// Check if entity has a specific behavior subtag
        /// </summary>
        public bool HasBehaviorSubtag(string subtag)
        {
            return behaviorSubtags.Contains(subtag);
        }

        /// <summary>
        /// Update resource-related flags in blackboard
        /// </summary>
        private void UpdateResourceFlags()
        {
            SetBlackboardVariable("HasFood", resourceTags.Contains("Food"));
            SetBlackboardVariable("HasWater", resourceTags.Contains("Water"));
            SetBlackboardVariable("HasCleanWater", resourceTags.Contains("Water.Clean"));
            SetBlackboardVariable("HasDirtyWater", resourceTags.Contains("Water.Dirty"));
            SetBlackboardVariable("IsGatherable", resourceTags.Contains("Gatherable"));
            SetBlackboardVariable("IsCraftingResource", resourceTags.Contains("Crafting"));
        }

        /// <summary>
        /// Update behavior-related flags in blackboard
        /// </summary>
        private void UpdateBehaviorFlags()
        {
            SetBlackboardVariable("IsCarnivore", behaviorSubtags.Contains("carnivore"));
            SetBlackboardVariable("IsHerbivore", behaviorSubtags.Contains("herbivore"));
            SetBlackboardVariable("IsWolf", behaviorSubtags.Contains("wolf"));
            SetBlackboardVariable("IsBandit", behaviorSubtags.Contains("bandit"));
            SetBlackboardVariable("IsAggressiveType", behaviorSubtags.Contains("aggressive"));
            SetBlackboardVariable("IsPassiveType", behaviorSubtags.Contains("passive"));
        }

        #endregion

        #region Enhanced State Management

        /// <summary>
        /// Change behavior state with enhanced tracking
        /// </summary>
        public void ChangeBehaviorState(string newState)
        {
            string oldState = currentBehaviorState;
            CurrentBehaviorState = newState;

            // Update blackboard
            if (enableBehaviorTrees)
            {
                SetBlackboardVariable("CurrentState", newState);
                SetBlackboardVariable("PreviousState", oldState);
            }

            UnityEngine.Debug.Log($"{EntityName} behavior state changed from {oldState} to {newState}");
        }

        /// <summary>
        /// Enhanced stat modification with behavior tree integration
        /// </summary>
        public void ModifyStatEnhanced(Stat.ID statID, float amount, string reason = "")
        {
            ModifyStat(statID, amount);

            // Log the reason if provided
            if (!string.IsNullOrEmpty(reason))
            {
                UnityEngine.Debug.Log($"{EntityName} {statID} modified by {amount} - Reason: {reason}");
            }

            // Update blackboard variables for behavior trees
            if (enableBehaviorTrees)
            {
                UpdateBlackboardVariables();

                // Check for critical stat levels
                CheckCriticalStatLevels(statID);
            }
        }

        /// <summary>
        /// Check for critical stat levels and update blackboard flags
        /// </summary>
        private void CheckCriticalStatLevels(Stat.ID statID)
        {
            switch (statID)
            {
                case Stat.ID.Health:
                    float healthPercent = (Health / MaxHealth) * 100f;
                    SetBlackboardVariable("HealthPercent", healthPercent);
                    SetBlackboardVariable("IsLowHealth", healthPercent <= 25f);
                    SetBlackboardVariable("IsCriticalHealth", healthPercent <= 10f);
                    break;

                case Stat.ID.Hunger:
                    float hungerPercent = (Hunger / 100f) * 100f; // Assuming max hunger is 100
                    SetBlackboardVariable("HungerPercent", hungerPercent);
                    SetBlackboardVariable("IsHungry", hungerPercent <= 30f);
                    SetBlackboardVariable("IsStarving", hungerPercent <= 10f);
                    break;

                case Stat.ID.Thirst:
                    float thirstPercent = (Thirst / 100f) * 100f; // Assuming max thirst is 100
                    SetBlackboardVariable("ThirstPercent", thirstPercent);
                    SetBlackboardVariable("IsThirsty", thirstPercent <= 30f);
                    SetBlackboardVariable("IsDehydrated", thirstPercent <= 10f);
                    break;

                case Stat.ID.Energy:
                    float energyPercent = (Energy / 100f) * 100f; // Assuming max energy is 100
                    SetBlackboardVariable("EnergyPercent", energyPercent);
                    SetBlackboardVariable("IsTired", energyPercent <= 30f);
                    SetBlackboardVariable("IsExhausted", energyPercent <= 10f);
                    break;
            }
        }

        #endregion
    }
}