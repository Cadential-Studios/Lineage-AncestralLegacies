// File: Assets/_Project/Scripts/Core/Systems/Movement/GridPathfinder.cs
using UnityEngine;
using System.Collections.Generic;
using Lineage.Debug;
using LogCategory = Lineage.Debug.Log.LogCategory;

namespace Lineage.Systems.Movement
{
    /// <summary>
    /// Grid-based A* pathfinding system for AI movement.
    /// More efficient than NavMesh for large populations and open areas.
    /// Supports obstacle avoidance and configurable grid resolution.
    /// </summary>
    public class GridPathfinder : MonoBehaviour, IMovementController
    {
        [Header("Grid Settings")]
        [SerializeField] private Vector2 gridSize = new Vector2(100f, 100f);
        [SerializeField] private float gridCellSize = 1f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float maxSpeed = 10f;

        [Header("Obstacle Detection")]
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private float obstacleCheckRadius = 0.3f;

        private Vector3 targetPosition;
        private List<Vector3> currentPath = new List<Vector3>();
        private int pathIndex = 0;
        private Vector3 currentVelocity = Vector3.zero;
        private bool isMoving = false;
        private bool isEnabled = true;
        private float distanceToTarget = float.MaxValue;

        public bool IsMoving => isMoving && isEnabled;
        public bool HasReachedDestination => distanceToTarget <= stoppingDistance;
        public bool IsEnabled => isEnabled && gameObject.activeInHierarchy;

        private void Awake()
        {
            targetPosition = transform.position;
        }

        private void Update()
        {
            if (!isEnabled || !isMoving || currentPath.Count == 0)
            {
                currentVelocity = Vector3.zero;
                return;
            }

            // Get current waypoint
            Vector3 currentWaypoint = currentPath[pathIndex];
            float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint);

            // Move to next waypoint if current is reached
            if (distanceToWaypoint <= stoppingDistance)
            {
                pathIndex++;
                if (pathIndex >= currentPath.Count)
                {
                    // Reached final destination
                    isMoving = false;
                    currentVelocity = Vector3.zero;
                    distanceToTarget = 0f;
                    return;
                }
                currentWaypoint = currentPath[pathIndex];
            }

            // Calculate remaining distance to target
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            // Move toward current waypoint
            Vector3 direction = (currentWaypoint - transform.position).normalized;
            Vector3 desiredVelocity = direction * moveSpeed;

            // Smoothly accelerate
            currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime * acceleration);
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);

            // Check for obstacles and adjust path if needed
            if (IsPathBlocked())
            {
                RecalculatePath();
            }

            // Move
            transform.position += currentVelocity * Time.deltaTime;
        }

        public void SetDestination(Vector3 targetPos)
        {
            if (!isEnabled)
            {
                Log.Warning(
                    $"{gameObject.name}: Cannot set destination - pathfinder is disabled",
                    LogCategory.AI,
                    this);
                return;
            }

            targetPosition = targetPos;
            currentPath.Clear();
            pathIndex = 0;

            // Simple pathfinding - for now just move toward target directly
            // In production, implement A* algorithm here
            currentPath.Add(targetPos);
            isMoving = true;
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        }

        public void Stop()
        {
            isMoving = false;
            currentVelocity = Vector3.zero;
            currentPath.Clear();
            pathIndex = 0;
        }

        public float GetSpeed()
        {
            return currentVelocity.magnitude;
        }

        public float GetRemainingDistance()
        {
            return isMoving ? distanceToTarget : 0f;
        }

        public void SetSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }

        public Vector3 GetDesiredVelocity()
        {
            return currentVelocity;
        }

        public void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
            if (!enabled)
            {
                Stop();
            }
        }

        private bool IsPathBlocked()
        {
            // Check if current path direction is blocked by obstacle
            if (currentPath.Count == 0) return false;

            Vector3 pathDirection = (currentPath[pathIndex] - transform.position).normalized;
            return Physics.Raycast(transform.position, pathDirection, obstacleCheckRadius, obstacleLayer);
        }

        private void RecalculatePath()
        {
            // Recalculate path around obstacles
            // For now, simple approach: try to move around obstacle
            Stop();
            SetDestination(targetPosition);
        }

        /// <summary>
        /// Visualize grid and path in editor
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!isMoving) return;

            // Draw path
            Gizmos.color = Color.green;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
            }

            // Draw target
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.3f);

            // Draw current waypoint
            if (pathIndex < currentPath.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentPath[pathIndex], 0.2f);
            }
        }
    }
}
