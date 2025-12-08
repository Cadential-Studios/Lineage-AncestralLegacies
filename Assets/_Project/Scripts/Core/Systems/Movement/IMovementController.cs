// File: Assets/_Project/Scripts/Core/Systems/Movement/IMovementController.cs
using UnityEngine;

namespace Lineage.Systems.Movement
{
    /// <summary>
    /// Abstraction layer for AI movement systems.
    /// Allows switching between different pathfinding/movement implementations without changing dependent code.
    /// </summary>
    public interface IMovementController
    {
        /// <summary>
        /// Set the destination for the agent to move to
        /// </summary>
        void SetDestination(Vector3 targetPosition);

        /// <summary>
        /// Stop all movement immediately
        /// </summary>
        void Stop();

        /// <summary>
        /// Check if the agent is currently moving
        /// </summary>
        bool IsMoving { get; }

        /// <summary>
        /// Check if the agent has reached its destination
        /// </summary>
        bool HasReachedDestination { get; }

        /// <summary>
        /// Get the current movement speed magnitude
        /// </summary>
        float GetSpeed();

        /// <summary>
        /// Get remaining distance to destination
        /// </summary>
        float GetRemainingDistance();

        /// <summary>
        /// Set the movement speed
        /// </summary>
        void SetSpeed(float speed);

        /// <summary>
        /// Get the current desired velocity (for animation blending)
        /// </summary>
        Vector3 GetDesiredVelocity();

        /// <summary>
        /// Enable or disable the movement system
        /// </summary>
        void SetEnabled(bool enabled);

        /// <summary>
        /// Check if the movement system is enabled and functional
        /// </summary>
        bool IsEnabled { get; }
    }
}
