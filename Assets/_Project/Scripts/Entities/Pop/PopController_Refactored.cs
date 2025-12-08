// File: Assets/_Project/Scripts/Entities/Pop/PopController_Refactored.cs
using UnityEngine;
using Lineage.Entities;
using Lineage.Managers;
using Lineage.Debug;
using Lineage.Systems.Movement;
using LogCategory = Lineage.Debug.Log.LogCategory;

namespace Lineage.Entities
{
    /// <summary>
    /// Refactored PopController using IMovementController abstraction.
    /// Supports any movement implementation (NavMesh, Grid, Simple movement).
    /// This replaces the NavMesh-specific PopController.
    /// </summary>
    [RequireComponent(typeof(Pop))]
    public class PopControllerRefactored : MonoBehaviour
    {
        // Component references
        private Pop pop;
        private IMovementController movementController;

        // Selection State & Indicator
        private bool isSelected = false;
        private Transform selectionIndicator;
        private bool _hasStoredOriginalColor = false;
        private Color _originalColor = Color.white;
        private SpriteRenderer _popSpriteRenderer;

        private void Awake()
        {
            // Get required components
            pop = GetComponent<Pop>();
            movementController = GetComponent<IMovementController>();

            if (pop == null)
            {
                UnityEngine.Debug.LogError($"PopControllerRefactored on {gameObject.name} is missing Pop component!", this);
                enabled = false;
                return;
            }

            if (movementController == null)
            {
                UnityEngine.Debug.LogError($"PopControllerRefactored on {gameObject.name} is missing IMovementController!", this);
                enabled = false;
                return;
            }

            // Cache the main sprite renderer for selection tinting
            _popSpriteRenderer = GetComponent<SpriteRenderer>();
            if (_popSpriteRenderer == null) _popSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (_popSpriteRenderer == null)
                UnityEngine.Debug.LogWarning($"PopControllerRefactored: Pop {pop.name} has no SpriteRenderer for selection tinting.", pop);

            // Configure movement speed
            float initialSpeed = 3.5f;
            if (pop.popData != null)
            {
                // TODO: Get speed from popData if available
            }
            else
            {
                Log.Warning(
                    $"PopControllerRefactored on {gameObject.name}: Could not get speed from PopData, using default speed of {initialSpeed}",
                    LogCategory.Gameplay,
                    this);
            }

            movementController.SetSpeed(initialSpeed);

            SetupSelectionIndicator();
        }

        private void SetupSelectionIndicator()
        {
            selectionIndicator = transform.Find("SelectionIndicator");
            if (selectionIndicator == null)
            {
                GameObject indicatorGO = new GameObject("SelectionIndicator");
                selectionIndicator = indicatorGO.transform;
                selectionIndicator.SetParent(transform);
                selectionIndicator.localPosition = new Vector3(0, -0.3f, 0);

                selectionIndicator.localScale = new Vector3(
                    0.5f / transform.localScale.x,
                    0.5f / transform.localScale.y,
                    1f / transform.localScale.z
                );

                SpriteRenderer sr = indicatorGO.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.2f, 1f, 0.2f, 0.45f);

                sr.sortingLayerName = _popSpriteRenderer != null ? _popSpriteRenderer.sortingLayerName : "Default";
                sr.sortingOrder = _popSpriteRenderer != null ? _popSpriteRenderer.sortingOrder - 1 : -1;
            }
            selectionIndicator.gameObject.SetActive(false);
        }

        public void OnSelected(bool selected)
        {
            if (isSelected == selected) return;

            isSelected = selected;

            if (_popSpriteRenderer != null)
            {
                if (selected)
                {
                    if (!_hasStoredOriginalColor)
                    {
                        _originalColor = _popSpriteRenderer.color;
                        _hasStoredOriginalColor = true;
                    }

                    _popSpriteRenderer.color = new Color(
                        _originalColor.r * 0.7f + 0.3f,
                        _originalColor.g * 0.7f + 0.3f,
                        _originalColor.b * 0.7f + 0.3f,
                        _originalColor.a
                    );

                    if (_popSpriteRenderer.sortingOrder < 100)
                    {
                        _popSpriteRenderer.sortingOrder = 100;
                    }
                }
                else if (_hasStoredOriginalColor)
                {
                    _popSpriteRenderer.color = _originalColor;
                    _popSpriteRenderer.sortingOrder = 0;
                }
            }

            if (selectionIndicator != null)
            {
                selectionIndicator.gameObject.SetActive(selected);
            }
        }

        /// <summary>
        /// Move to target position using the active movement controller
        /// </summary>
        public void MoveTo(Vector3 targetPosition)
        {
            if (movementController == null || !movementController.IsEnabled)
            {
                Log.Warning(
                    $"PopControllerRefactored: Pop {pop.name} cannot move - movement controller unavailable",
                    LogCategory.AI,
                    this);
                return;
            }

            movementController.SetDestination(targetPosition);

            // Update animation
            if (pop.Animator != null && HasAnimatorParameter(pop.Animator, "IsMoving"))
            {
                pop.Animator.SetBool("IsMoving", true);
            }
        }

        /// <summary>
        /// Stop movement immediately
        /// </summary>
        public void StopMovement()
        {
            if (movementController != null && movementController.IsEnabled)
            {
                movementController.Stop();
            }

            if (pop.Animator != null && HasAnimatorParameter(pop.Animator, "IsMoving"))
            {
                pop.Animator.SetBool("IsMoving", false);
            }
        }

        /// <summary>
        /// Update agent speed dynamically
        /// </summary>
        public void UpdateAgentSpeed(float newSpeed)
        {
            if (movementController != null && movementController.IsEnabled)
            {
                movementController.SetSpeed(newSpeed);
            }
        }

        /// <summary>
        /// Get the Pop component
        /// </summary>
        public Pop GetPop() => pop;

        /// <summary>
        /// Force select this Pop
        /// </summary>
        public void ForceSelect()
        {
            OnSelected(true);
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.SelectPop(pop);
            }
        }

        /// <summary>
        /// Check if animator has a specific parameter
        /// </summary>
        private bool HasAnimatorParameter(Animator animator, string parameterName)
        {
            foreach (AnimatorParameter param in animator.parameters)
            {
                if (param.name == parameterName)
                    return true;
            }
            return false;
        }

        private void Update()
        {
            if (pop == null || movementController == null || !movementController.IsMoving)
                return;

            // Update animation movement state
            if (pop.Animator != null && HasAnimatorParameter(pop.Animator, "IsMoving"))
            {
                pop.Animator.SetBool("IsMoving", movementController.IsMoving);
            }

            // Optional: Handle sprite flipping based on movement direction
            HandleSpriteDirection();
        }

        private void HandleSpriteDirection()
        {
            if (_popSpriteRenderer == null)
                return;

            Vector3 velocity = movementController.GetDesiredVelocity();
            if (velocity.magnitude > 0.1f)
            {
                // Flip sprite based on x velocity (for 2D top-down)
                if (velocity.x < -0.1f)
                {
                    _popSpriteRenderer.flipX = true;
                }
                else if (velocity.x > 0.1f)
                {
                    _popSpriteRenderer.flipX = false;
                }
            }
        }
    }
}
