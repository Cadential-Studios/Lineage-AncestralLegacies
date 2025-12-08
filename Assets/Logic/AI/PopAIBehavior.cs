using UnityEngine;
using UnityEngine.AI;
using Lineage.Entities;
using Lineage.Managers;
using Lineage.Systems.ResourceNodes;

#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace Lineage.AI
{
    /// <summary>
    /// AI behavior system for Pops - handles autonomous resource gathering, wandering, and survival behaviors
    /// </summary>
    [RequireComponent(typeof(Pop))]
    public class PopAIBehavior : MonoBehaviour
    {
        [Header("AI Settings")]
        [SerializeField] private bool enableAI = true;
        [SerializeField] private float decisionInterval = 2f; // How often AI makes decisions
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private float gatherRange = 1.5f;
        [SerializeField] private float gatherDuration = 3f;

        [Header("Behavior Priorities")]
        [SerializeField] private float hungerThreshold = 50f;
        [SerializeField] private float thirstThreshold = 50f;

        private Pop pop;
        private NavMeshAgent agent;
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
            agent = GetComponent<NavMeshAgent>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            if (!enableAI || pop == null) return;

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
            if (currentState == AIState.Idle || !agent.hasPath)
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
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint = GetRandomWanderPoint();
                if (randomPoint != Vector3.zero)
                {
                    agent.SetDestination(randomPoint);
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
                    agent.SetDestination(currentTarget.transform.position);
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
            agent.isStopped = true;

            float gatherTime = 0f;
            float gatherRate = 10f; // Amount of food gathered per second

            while (gatherTime < gatherDuration && currentTarget != null)
            {
                gatherTime += Time.deltaTime;

                // Gather food incrementally
                if (ResourceManager.Instance != null)
                {
                    ResourceManager.Instance.AddFood(gatherRate * Time.deltaTime);
                }

                // Consume the resource from the resource node (if it has a component)
                var resourceNode = currentTarget.GetComponent<ResourceNode>();
                if (resourceNode != null)
                {
                    if (!resourceNode.CanHarvest())
                    {
                        break; // Resource depleted
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
            agent.isStopped = false;
            currentTarget = null;
            ChangeState(AIState.Idle);
        }

        private GameObject FindNearestFoodSource()
        {
            // Find all resource nodes tagged appropriately
            GameObject[] foodSources = GameObject.FindGameObjectsWithTag("ResourceNode");

            if (foodSources.Length == 0)
            {
                // Fallback: find any objects with "berry" in name
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
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;
            randomDirection.y = transform.position.y; // Keep on same Y level

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }

            return Vector3.zero;
        }

        private void ChangeState(AIState newState)
        {
            if (currentState == newState) return;

            // Exit current state
            OnStateExit(currentState);

            // Enter new state
            currentState = newState;
            stateStartTime = Time.time;
            OnStateEnter(newState);
        }

        private void OnStateEnter(AIState state)
        {
            switch (state)
            {
                case AIState.GatheringResource:
                    if (agent != null) agent.isStopped = true;
                    break;
            }
        }

        private void OnStateExit(AIState state)
        {
            switch (state)
            {
                case AIState.GatheringResource:
                    if (agent != null) agent.isStopped = false;
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
                if (agent != null)
                {
                    agent.ResetPath();
                }
            }
        }

        public AIState GetCurrentState() => currentState;
    }
}
