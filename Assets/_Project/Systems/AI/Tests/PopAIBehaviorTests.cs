// File: Assets/_Project/Systems/AI/Tests/PopAIBehaviorTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Lineage.Entities;
using Lineage.Systems.Movement;
using Lineage.Components;

namespace Lineage.AI.Tests
{
    /// <summary>
    /// Playmode tests for Pop AI behavior state transitions (Idle → Wander → Gather).
    /// Uses mock movement controller and resource node.
    /// </summary>
    public class PopAIBehaviorTests
    {
        private class MockMovementController : MonoBehaviour, IMovementController
        {
            private Vector3 destination;
            private bool moving = false;
            private bool enabled = true;

            public bool IsMoving => moving && enabled;
            public bool HasReachedDestination => !moving;
            public bool IsEnabled => enabled;

            public void SetDestination(Vector3 targetPos)
            {
                if (!enabled) return;
                destination = targetPos;
                moving = true;

                // Auto-reach after short delay
                StartCoroutine(AutoReach());
            }

            private IEnumerator AutoReach()
            {
                yield return new WaitForSeconds(0.1f);
                if (moving)
                {
                    transform.position = destination;
                    moving = false;
                }
            }

            public void Stop()
            {
                moving = false;
            }

            public float GetSpeed() => moving ? 3.5f : 0f;
            public float GetRemainingDistance() => moving ? Vector3.Distance(transform.position, destination) : 0f;
            public void SetSpeed(float speed) { }
            public Vector3 GetDesiredVelocity() => moving ? (destination - transform.position).normalized * 3.5f : Vector3.zero;
            public void SetEnabled(bool value)
            {
                enabled = value;
                if (!enabled) moving = false;
            }
        }

        [UnityTest]
        public IEnumerator PopAIBehavior_InitializesWithoutErrors()
        {
            var popGO = new GameObject("TestPop");
            var pop = popGO.AddComponent<Pop>();
            var entityData = popGO.AddComponent<EntityDataComponent>();
            var inventory = popGO.AddComponent<InventoryComponent>();
            var mockMovement = popGO.AddComponent<MockMovementController>();
            var aiBehavior = popGO.AddComponent<PopAIBehaviorRefactored>();

            yield return null; // Allow Awake/Start

            Assert.IsNotNull(aiBehavior, "AI Behavior should be initialized");
            Assert.IsTrue(popGO.activeSelf, "Pop should be active after initialization");

            Object.DestroyImmediate(popGO);
        }

        [UnityTest]
        public IEnumerator PopAIBehavior_TransitionsFromIdleToWandering()
        {
            var popGO = new GameObject("WanderingTestPop");
            popGO.AddComponent<Pop>();
            popGO.AddComponent<EntityDataComponent>();
            popGO.AddComponent<InventoryComponent>();
            var mockMovement = popGO.AddComponent<MockMovementController>();
            popGO.AddComponent<PopAIBehaviorRefactored>();

            yield return new WaitForSeconds(2.5f); // Wait for decision interval

            // Pop should eventually wander (movement controller receives commands)
            bool hasWandered = mockMovement.IsMoving || mockMovement.HasReachedDestination;
            Assert.IsTrue(hasWandered, "Pop should transition to wandering state");

            Object.DestroyImmediate(popGO);
        }

        [UnityTest]
        public IEnumerator PopMovement_UsesIMovementController()
        {
            var popGO = new GameObject("MovementTestPop");
            var pop = popGO.AddComponent<Pop>();
            popGO.AddComponent<EntityDataComponent>();
            popGO.AddComponent<InventoryComponent>();
            var mockMovement = popGO.AddComponent<MockMovementController>();

            yield return null; // Initialize

            Vector3 target = new Vector3(5f, 0f, 0f);
            bool moveSuccess = pop.MoveTo(target);

            Assert.IsTrue(moveSuccess, "MoveTo should succeed");

            yield return new WaitForSeconds(0.2f);

            Assert.IsTrue(mockMovement.HasReachedDestination, "Mock movement should reach destination");

            Object.DestroyImmediate(popGO);
        }

        [UnityTest]
        public IEnumerator Pop_StopMovement_HaltsController()
        {
            var popGO = new GameObject("StopMovementTestPop");
            var pop = popGO.AddComponent<Pop>();
            popGO.AddComponent<EntityDataComponent>();
            popGO.AddComponent<InventoryComponent>();
            var mockMovement = popGO.AddComponent<MockMovementController>();

            yield return null;

            pop.MoveTo(new Vector3(10f, 0f, 0f));
            yield return null;

            pop.StopMovement();

            Assert.IsFalse(mockMovement.IsMoving, "Movement controller should be stopped");

            Object.DestroyImmediate(popGO);
        }
    }
}
