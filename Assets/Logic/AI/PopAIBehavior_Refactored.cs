// File: Assets/Logic/AI/PopAIBehavior_Refactored.cs
using UnityEngine;
using Lineage.Entities;
using Lineage.Managers;
using Lineage.Systems.ResourceNodes;
using Lineage.Systems.Movement;
using Lineage.Debug;
using System.Collections;
using LogCategory = Lineage.Debug.Log.LogCategory;

#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace Lineage.AI
{
    /// <summary>
    /// Refactored PopAIBehavior using IMovementController for abstraction.
    /// Handles autonomous resource gathering, wandering, and survival behaviors.
    /// Works with any movement system (NavMesh, Grid, Simple).
    /// </summary>
    [RequireComponent(typeof(Pop))]
    public class PopAIBehaviorRefactored : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private bool enableAI = true;
        [SerializeField] private float decisionInterval = 2f;
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private float gatherRange = 1.5f;
        [SerializeField] private float gatherDuration = 3f;

        [Header("Behavior Priorities")]
        [SerializeField] private float hungerThreshold = 50f;
        [SerializeField] private float thirstThreshold = 50f;

        private Pop pop;
        private IMovementController movementController;
        private float nextDecisionTime;
        private AIState currentState = AIState.Idle;
        private GameObject currentTarget;
        private float stateStartTime;
        private Vector3 lastPosition;
        private bool isGathering;

        public enum AIState
        {
            Idle,
            Wandering,
            SeekingFood,
            GatheringResource,
            Resting
        }

        private void Awake()
        {
            pop = GetComponent<Pop>();
            movementController = GetComponent<IMovementController>();
            lastPosition = transform.position;

            if (pop == null)
            {
                Log.Error(
                    $"PopAIBehaviorRefactored on {gameObject.name} missing Pop component",
                    LogCategory.AI,
                    this);
                enabled = false;
            }

            if (movementController == null)
            {
                Log.Warning(
                    $"PopAIBehaviorRefactored on {gameObject.name} missing IMovementController",
                    LogCategory.AI,
                    this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!enableAI || pop == null || movementController == null)
                return;

            // Make decisions at intervals
            if (Time.time >= nextDecisionTime)
            {
                MakeDecision();
                nextDecisionTime = Time.time + decisionInterval;
            }

            // Execute current state
            ExecuteState();
        }

        private void MakeDecision()
        {
            // Priority 1: Survival needs
            if (pop.hunger < hungerThreshold)
            {
                ChangeState(AIState.SeekingFood);
                return;
            }

            // Priority 2: If nothing urgent, wander or rest
            if (currentState == AIState.Idle || !movementController.IsMoving)
            {
                if (Random.value > 0.3f)
                {
                    ChangeState(AIState.Wandering);
                }
                else
                {
                    ChangeState(AIState.Idle);
                }
            }
        }

        private void ExecuteState()
        {
            switch (currentState)
            {
                case AIState.Idle:
                    // Just stand still, consuming minimal resources
                    break;

                case AIState.Wandering:
                    ExecuteWandering();
                    break;

                case AIState.SeekingFood:
                    ExecuteSeekingFood();
                    break;

                case AIState.GatheringResource:
                    ExecuteGathering();
                    break;

                case AIState.Resting:
                    // Future implementation for rest/sleep
                    break;
            }
        }

        private void ExecuteWandering()
        {
            // If we've reached our destination or don't have one, pick a new random point
            if (!movementController.IsMoving || movementController.HasReachedDestination)
            {
                Vector3 randomPoint = GetRandomWanderPoint();
                if (randomPoint != Vector3.zero)
                {
                    movementController.SetDestination(randomPoint);
                }
                else
                {
                    ChangeState(AIState.Idle);
                }
            }
        }

        private void ExecuteSeekingFood()
        {
            // Look for food resource nodes
            if (currentTarget == null)
            {
                currentTarget = FindNearestFoodSource();

                if (currentTarget != null)
                {
                    movementController.SetDestination(currentTarget.transform.position);
                }
                else
                {
                    // No food found, wander hoping to find some
                    ChangeState(AIState.Wandering);
                    return;
                }
            }

            // Check if we've reached the food source
            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) < gatherRange)
            {
                ChangeState(AIState.GatheringResource);
            }
            else if (currentTarget == null || !currentTarget.activeInHierarchy)
            {
                currentTarget = null;
                ChangeState(AIState.Idle);
            }
        }

        private void ExecuteGathering()
        {
            if (isGathering) return;

            if (currentTarget != null)
            {
                StartCoroutine(GatherResource());
            }
            else
            {
                ChangeState(AIState.Idle);
            }
        }

        private System.Collections.IEnumerator GatherResource()
        {
            isGathering = true;
            movementController.Stop();

            float gatherTime = 0f;
            float gatherRate = 10f;

            while (gatherTime < gatherDuration && currentTarget != null)
            {
                gatherTime += Time.deltaTime;

                // Gather food incrementally
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.AddFood(gatherRate * Time.deltaTime);
                }

                // Consume the resource from the resource node
                var resourceNode = currentTarget.GetComponent<ResourceNode>();
                if (resourceNode != null)
                {
                    if (!resourceNode.CanHarvest())
                    {
                        break;
                    }
                }

                yield return null;
            }

            // Feed the pop from gathered food
            if (ResourceManager.Instance != null && ResourceManager.Instance.ConsumeFood(5f))
            {
                pop.EatFood(20f);
            }

            isGathering = false;
            currentTarget = null;
            ChangeState(AIState.Idle);
        }

        private GameObject FindNearestFoodSource()
        {
            GameObject[] foodSources = GameObject.FindGameObjectsWithTag("ResourceNode");

            if (foodSources.Length == 0)
            {
                var allObjects = FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
                if (allObjects.Length > 0)
                {
                    return allObjects[0].gameObject;
                }
                return null;
            }

            GameObject nearest = null;
            float nearestDistance = float.MaxValue;

            foreach (var source in foodSources)
            {
                float distance = Vector3.Distance(transform.position, source.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = source;
                }
            }

            return nearest;
        }

        private Vector3 GetRandomWanderPoint()
        {
            // Simple random wander point generator
            // For grid pathfinder: uses grid snapping
            // For simple movement: uses direct position

            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y;

            // Simple validation - just ensure it's reachable distance
            // More sophisticated pathfinding should be implemented in movement controller
            return randomDirection;
        }

        private void ChangeState(AIState newState)
        {
            if (currentState == newState) return;

            OnStateExit(currentState);
            currentState = newState;
            stateStartTime = Time.time;
            OnStateEnter(newState);
        }

        private void OnStateEnter(AIState state)
        {
            switch (state)
            {
                case AIState.GatheringResource:
                    movementController.Stop();
                    break;
            }
        }

        private void OnStateExit(AIState state)
        {
            switch (state)
            {
                case AIState.GatheringResource:
                    isGathering = false;
                    break;
            }
        }

        public void SetAIEnabled(bool enabled)
        {
            enableAI = enabled;
            if (!enabled)
            {
                ChangeState(AIState.Idle);
                if (movementController != null)
                {
                    movementController.Stop();
                }
            }
        }

        public AIState GetCurrentState() => currentState;
    }
}
