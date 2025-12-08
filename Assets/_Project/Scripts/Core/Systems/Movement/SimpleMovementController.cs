// File: Assets/_Project/Scripts/Core/Systems/Movement/SimpleMovementController.cs
using UnityEngine;
using Lineage.Debug;
using LogCategory = Lineage.Debug.Log.LogCategory;

namespace Lineage.Systems.Movement
{
    /// <summary>
    /// Simple direct movement controller using Vector3.Lerp for smooth movement.
    /// Efficient for large numbers of entities - no pathfinding overhead.
    /// Good for open areas or procedural terrain without complex obstacles.
    /// </summary>
    public class SimpleMovementController : MonoBehaviour, IMovementController
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float maxSpeed = 10f;

        private Vector3 targetPosition;
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
            if (!isEnabled || !isMoving)
            {
                currentVelocity = Vector3.zero;
                return;
            }

            // Calculate distance to target
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            // Stop if reached destination
            if (distanceToTarget <= stoppingDistance)
            {
                isMoving = false;
                currentVelocity = Vector3.zero;
                transform.position = targetPosition;
                return;
            }

            // Calculate desired direction
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 desiredVelocity = direction * moveSpeed;

            // Smoothly accelerate toward desired velocity
            currentVelocity = Vector3.Lerp(currentVelocity, desiredVelocity, Time.deltaTime * acceleration);
            currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxSpeed);

            // Move toward target
            transform.position += currentVelocity * Time.deltaTime;
        }

        public void SetDestination(Vector3 targetPos)
        {
            if (!isEnabled)
            {
                Log.Warning(
                    $"{gameObject.name}: Cannot set destination - movement controller is disabled",
                    LogCategory.AI,
                    this);
                return;
            }

            targetPosition = targetPos;
            isMoving = true;
            distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        }

        public void Stop()
        {
            isMoving = false;
            currentVelocity = Vector3.zero;
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
    }
}
