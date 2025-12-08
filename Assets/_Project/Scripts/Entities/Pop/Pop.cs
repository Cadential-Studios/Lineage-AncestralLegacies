using UnityEngine;
using Lineage.Systems.Inventory;
using Lineage.Managers;
using Lineage.Debug;
using Lineage.Components;
using UnityEngine.UI;
using UnityEngine.AI;
using Lineage.Core;
using Lineage.Entities;
using Lineage.Systems.Movement;
using LogCategory = Lineage.Debug.Log.LogCategory;

namespace Lineage.Entities
{
    /// <summary>
    /// Core Pop entity representing a population unit with needs, inventory, and AI.
    /// 
    /// ARCHITECTURE CHANGE (v2.0):
    /// This class has been refactored to use EntityDataComponent as the primary data store.
    /// All stat management, needs tracking, and entity data is now handled by EntityDataComponent.
    /// 
    /// Pop class responsibilities:
    /// - Unity component orchestration (NavMeshAgent, SpriteRenderer, Animator, etc.)
    /// - Visual systems (health bars, selection highlighting, animations)
    /// - Navigation and movement logic
    /// - Population management integration
    /// - Game lifecycle events (death, spawning, etc.)
    /// - PopData ScriptableObject integration
    /// 
    /// EntityDataComponent responsibilities:
    /// - All stat storage and management (health, hunger, thirst, etc.)
    /// - Needs system logic and decay
    /// - Entity data structures (Entity struct from Database)
    /// - Buff/debuff management
    /// - State management
    /// </summary>
    [RequireComponent(typeof(EntityDataComponent))]
    [RequireComponent(typeof(InventoryComponent))]
    public class Pop : MonoBehaviour
    {
        [Header("Pop Identity")]
        public string popName = "Unnamed Pop";
        public int age = 0;
        [Header("Health & Stats - Use EntityDataComponent")]
        // These properties delegate to EntityDataComponent for consistency
        // The actual stat values are stored in EntityDataComponent.EntityData
        // This provides backward compatibility while using EntityDataComponent as the source of truth

        // Property accessors for compatibility (delegate to EntityDataComponent when available)
        public float health => 100f; // TODO: pull from EntityDataComponent when API exposed
        public float thirst => entityDataComponent?.GetThirst() ?? 100f;
        public float hunger => entityDataComponent?.GetHunger() ?? 100f;
        public float maxHealth => 100f; // TODO: expose from EntityDataComponent when available
        public float maxThirst => 100f; // TODO: expose from EntityDataComponent when available
        public float maxHunger => 100f; // TODO: expose from EntityDataComponent when available

        [Header("Pop Data Reference")]
        public PopData popData;
        public EntityDataComponent entityDataComponent;
        public InventoryComponent inventoryComponent;

        [Header("Navigation")]
        private IMovementController movementController;
        public bool isMoving => movementController != null && movementController.IsMoving;
        public float movementSpeed => movementController != null ? movementController.GetSpeed() : 0f;

        [Header("Visuals")]
        public SpriteRenderer spriteRenderer;
        public Image healthBar;
        public Animator animator;
        public bool lockRotation = true;


        public bool showHealthBar = true; // Toggle for health bar visibility
        // Property for PopController animation access (capital A for compatibility)
        public Animator Animator => animator;

        private Color originalSpriteColor;
        private bool _hasStoredOriginalColor = false;

        // Event for when this pop is destroyed
        public static System.Action<Pop> OnPopDestroyed;

        private void Awake()
        {
            // Get component references FIRST
            entityDataComponent = GetComponent<EntityDataComponent>();
            inventoryComponent = GetComponent<InventoryComponent>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            // Initialize movement controller (supports both NavMesh and new systems)
            InitializeMovementController();

            // Log what we found
            UnityEngine.Debug.Log($"[Pop] {gameObject.name} Awake - EntityDataComponent: {(entityDataComponent != null ? "Found" : "MISSING!")}");

            // CRITICAL: Initialize entity data in Awake to ensure it's ready before Update runs
            if (entityDataComponent != null)
            {
                if (!entityDataComponent.isInitialized)
                {
                    // InitializeEntityData();  // NOTE: Disabled - namespace conflicts
                    UnityEngine.Debug.Log($"[Pop] {gameObject.name} initialized in Awake. isInitialized={entityDataComponent.isInitialized}");
                }
                else
                {
                    UnityEngine.Debug.Log($"[Pop] {gameObject.name} was already initialized");
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"[Pop] {gameObject.name} has NO EntityDataComponent! Adding one...");
                entityDataComponent = gameObject.AddComponent<EntityDataComponent>();
                // InitializeEntityData();  // NOTE: Disabled - namespace conflicts
            }

            // Store original color for selection highlighting
            if (spriteRenderer != null && !_hasStoredOriginalColor)
            {
                originalSpriteColor = spriteRenderer.color;
                _hasStoredOriginalColor = true;
            }
        }

        private void InitializeMovementController()
        {
            // Try to get IMovementController interface (supports multiple implementations)
            movementController = GetComponent<IMovementController>();

            if (movementController == null)
            {
                // Fall back to SimpleMovementController as default
                movementController = gameObject.AddComponent<SimpleMovementController>();
                Log.Info(
                    $"[Pop] {gameObject.name}: Added SimpleMovementController (default movement system)",
                    LogCategory.AI,
                    this);
            }
            else
            {
                Log.Info(
                    $"[Pop] {gameObject.name}: Using {movementController.GetType().Name} for movement",
                    LogCategory.AI,
                    this);
            }
        }

        private void Start()
        {
            // Apply pop data if available (entity data already initialized in Awake)
            if (popData != null)
            {
                ApplyPopData();
            }
        }

        private void Update()
        {
            // GUARD: Skip all processing until entity is initialized
            if (entityDataComponent == null || !entityDataComponent.isInitialized)
            {
                return;
            }

            // Update needs decay through EntityDataComponent
            entityDataComponent.UpdateNeeds(Time.deltaTime);

            if (showHealthBar && healthBar != null)
            {
                healthBar.gameObject.SetActive(true);
            }
            else if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
            }

            // Check for death conditions (only if initialized - checked above)
            CheckDeathConditions();

            // Update health bar if available
            UpdateHealthBar();

            if (lockRotation && agent != null && agent.isOnNavMesh)
            {
                gameObject.transform.rotation = Quaternion.LookRotation(agent.velocity.normalized, Vector3.up);
            }
        }

        /// <summary>
        /// Applies PopData configuration to this Pop instance.
        /// </summary>
        private void ApplyPopData()
        {
            if (popData == null) return;

            // Apply age
            age = Random.Range(8, 64);

            // Apply starting items to inventory
            if (inventoryComponent != null && popData.startingItems != null)
            {
                foreach (var item in popData.startingItems)
                {
                    if (item != null)
                    {
                        inventoryComponent.AddItem(item.itemId, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the pop should die based on health or critical needs.
        /// </summary>
        private void CheckDeathConditions()
        {
            // GUARD: Never kill uninitialized pops
            if (entityDataComponent == null || !entityDataComponent.isInitialized)
            {
                return;
            }

            if (health <= 0)
            {
                UnityEngine.Debug.Log($"[Pop] {popName} died: health={health}");
                Die();
                return;
            }

            // Check critical needs through EntityDataComponent
            if (entityDataComponent.HasCriticalNeeds())
            {
                UnityEngine.Debug.Log($"[Pop] {popName} died: critical needs");
                Die();
            }
        }

        /// <summary>
        /// Kills this pop and handles cleanup.
        /// </summary>
        public void Die()
        {
            Log.Info($"Pop {popName} has died.", Log.LogCategory.Population);            // Notify managers
            OnPopDestroyed?.Invoke(this);

            if (PopulationManager.Instance != null)
            {
                PopulationManager.Instance.OnPopDied(this);
            }

            // Destroy the game object
            Destroy(gameObject);
        }

        /// <summary>
        /// Updates the health bar UI if available.
        /// </summary>
        private void UpdateHealthBar()
        {
            if (healthBar != null)
            {
                healthBar.fillAmount = health / maxHealth;
            }
        }

        #region Needs Management (Delegated to EntityDataComponent)

        /// <summary>
        /// Gets the current hunger level (0-100).
        /// </summary>
        public float GetHunger() => entityDataComponent?.GetHunger() ?? 50f;

        /// <summary>
        /// Gets the current thirst level (0-100).
        /// </summary>
        public float GetThirst() => entityDataComponent?.GetThirst() ?? 50f;

        /// <summary>
        /// Gets the current energy level (0-100).
        /// </summary>
        public float GetEnergy() => entityDataComponent?.GetEnergy() ?? 50f;

        /// <summary>
        /// Gets the current rest level (0-100).
        /// </summary>
        public float GetRest() => entityDataComponent?.GetRest() ?? 50f;

        /// <summary>
        /// Feeds the pop, increasing hunger satisfaction.
        /// </summary>
        public void EatFood(float amount)
        {
            entityDataComponent?.EatFood(amount);
        }

        /// <summary>
        /// Gives the pop water, increasing thirst satisfaction.
        /// </summary>
        public void DrinkWater(float amount)
        {
            entityDataComponent?.DrinkWater(amount);
        }

        /// <summary>
        /// Restores the pop's energy.
        /// </summary>
        public void RestoreEnergy(float amount)
        {
            entityDataComponent?.RestoreEnergy(amount);
        }

        /// <summary>
        /// Allows the pop to sleep, restoring rest.
        /// </summary>
        public void Sleep(float amount)
        {
            entityDataComponent?.Sleep(amount);
        }        // Convenience properties for backward compatibility - these delegate to EntityDataComponent
        public float stamina => GetEnergy(); // Map energy to stamina for backward compatibility

        public bool IsHungry => GetHunger() < 50f;
        public bool IsThirsty => GetThirst() < 50f;
        public bool IsTired => GetEnergy() < 30f;

        #endregion

        #region Selection and Interaction

        /// <summary>
        /// Handles pop selection for UI highlighting.        /// </summary>
        public void OnSelected(bool selected)
        {
            if (spriteRenderer == null) return;

            if (selected)
            {
                // Highlight the pop when selected
                spriteRenderer.color = Color.yellow;
            }
            else
            {
                // Restore original color when deselected
                if (_hasStoredOriginalColor)
                {
                    spriteRenderer.color = originalSpriteColor;
                }
            }
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Move the pop to a specific world position.
        /// Works with any IMovementController implementation (NavMesh, Grid, or Simple movement).
        /// </summary>
        /// <param name="targetPosition">The world position to move to</param>
        /// <returns>True if the destination was set successfully</returns>
        public bool MoveTo(Vector3 targetPosition)
        {
            if (movementController == null || !movementController.IsEnabled)
            {
                Log.Warning(
                    $"Pop {popName}: Cannot move - movement controller not ready",
                    LogCategory.AI,
                    this);
                return false;
            }

            movementController.SetDestination(targetPosition);
            return true;
        }

        /// <summary>
        /// Move the pop to a specific transform location.
        /// </summary>
        /// <param name="target">The target transform to move to</param>
        /// <returns>True if the destination was set successfully</returns>
        public bool MoveTo(Transform target)
        {
            if (target == null)
            {
                Log.Warning(
                    $"Pop {popName}: Cannot move to null target",
                    LogCategory.AI,
                    this);
                return false;
            }

            return MoveTo(target.position);
        }

        /// <summary>
        /// Stop the pop's current movement.
        /// </summary>
        public void StopMovement()
        {
            if (movementController != null && movementController.IsEnabled)
            {
                movementController.Stop();
            }
        }

        /// <summary>
        /// Check if the pop is currently moving.
        /// </summary>
        /// <returns>True if the pop is moving</returns>
        public bool IsMoving()
        {
            return movementController != null && movementController.IsMoving;
        }

        /// <summary>
        /// Check if the pop has reached its destination.
        /// </summary>
        /// <returns>True if the pop has reached its destination</returns>
        public bool HasReachedDestination()
        {
            if (movementController == null || !movementController.IsEnabled)
                return true; // Consider "reached" if movement not available

            return movementController.HasReachedDestination;
        }

        /// <summary>
        /// Get the current movement speed of the pop.
        /// </summary>
        /// <returns>Current speed</returns>
        public float GetMovementSpeed()
        {
            if (movementController == null || !movementController.IsEnabled)
                return 0f;

            return movementController.GetSpeed();
        }

        #endregion

        #region Debug and Status

        /// <summary>
        /// Gets a comprehensive status string for debugging.
        /// </summary>
        public string GetStatusString()
        {
            if (entityDataComponent == null)
                return $"{popName}: Health={health:F1}/{maxHealth:F1}, Needs=N/A (EntityDataComponent missing)";

            return $"{popName}: Health={health:F1}/{maxHealth:F1}, " +
                   $"Hunger={GetHunger():F1}, Thirst={GetThirst():F1}, " +
                   $"Energy={GetEnergy():F1}, Rest={GetRest():F1}";
        }

        #endregion

        private void OnDestroy()
        {
            // Cleanup when destroyed
            OnPopDestroyed?.Invoke(this);
        }
    }
}