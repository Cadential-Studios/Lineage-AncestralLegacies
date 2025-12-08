// File: Assets/_Project/Systems/Movement/Tests/MovementEdgeCaseTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lineage.Systems.Movement.Tests
{
    /// <summary>
    /// Edge case tests for movement controllers: disabled state, rapid retargeting, zero speed, stopping distance compliance.
    /// </summary>
    public class MovementEdgeCaseTests
    {
        [UnityTest]
        public IEnumerator SimpleMovementController_DisabledState_DoesNotMove()
        {
            var go = new GameObject("DisabledMovementTest");
            var controller = go.AddComponent<SimpleMovementController>();

            controller.SetEnabled(false);
            Vector3 startPos = go.transform.position;
            controller.SetDestination(new Vector3(5f, 0f, 0f));

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(startPos, go.transform.position, "Controller should not move when disabled");
            Assert.IsFalse(controller.IsMoving, "IsMoving should be false when disabled");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator SimpleMovementController_RapidRetargeting_FollowsLastTarget()
        {
            var go = new GameObject("RapidRetargetTest");
            var controller = go.AddComponent<SimpleMovementController>();

            controller.SetDestination(new Vector3(3f, 0f, 0f));
            yield return null;
            controller.SetDestination(new Vector3(6f, 0f, 0f));
            yield return null;
            controller.SetDestination(new Vector3(2f, 0f, 0f));

            float timeout = 5f;
            Vector3 finalTarget = new Vector3(2f, 0f, 0f);
            while (timeout > 0f && Vector3.Distance(go.transform.position, finalTarget) > 0.6f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.LessOrEqual(Vector3.Distance(go.transform.position, finalTarget), 0.6f, "Should reach the final retargeted destination");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator SimpleMovementController_ZeroSpeed_DoesNotMove()
        {
            var go = new GameObject("ZeroSpeedTest");
            var controller = go.AddComponent<SimpleMovementController>();

            controller.SetSpeed(0f);
            Vector3 startPos = go.transform.position;
            controller.SetDestination(new Vector3(5f, 0f, 0f));

            yield return new WaitForSeconds(0.3f);

            Assert.Less(Vector3.Distance(go.transform.position, startPos), 0.1f, "Controller with zero speed should not move significantly");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator SimpleMovementController_StoppingDistance_Compliance()
        {
            var go = new GameObject("StoppingDistanceTest");
            var controller = go.AddComponent<SimpleMovementController>();

            Vector3 target = new Vector3(3f, 0f, 0f);
            controller.SetDestination(target);

            float timeout = 5f;
            while (timeout > 0f && !controller.HasReachedDestination)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            float actualDistance = Vector3.Distance(go.transform.position, target);
            Assert.LessOrEqual(actualDistance, 0.5f, "Should stop within stopping distance threshold");
            Assert.IsFalse(controller.IsMoving, "IsMoving should be false when destination reached");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator GridPathfinder_DisabledState_DoesNotMove()
        {
            var go = new GameObject("DisabledGridTest");
            var controller = go.AddComponent<GridPathfinder>();

            controller.SetEnabled(false);
            Vector3 startPos = go.transform.position;
            controller.SetDestination(new Vector3(5f, 0f, 0f));

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(startPos, go.transform.position, "GridPathfinder should not move when disabled");
            Assert.IsFalse(controller.IsMoving, "IsMoving should be false when disabled");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator GridPathfinder_RapidRetargeting_FollowsLastTarget()
        {
            var go = new GameObject("GridRapidRetargetTest");
            var controller = go.AddComponent<GridPathfinder>();

            controller.SetDestination(new Vector3(3f, 0f, 0f));
            yield return null;
            controller.SetDestination(new Vector3(6f, 0f, 0f));
            yield return null;
            controller.SetDestination(new Vector3(2f, 0f, 0f));

            float timeout = 5f;
            Vector3 finalTarget = new Vector3(2f, 0f, 0f);
            while (timeout > 0f && Vector3.Distance(go.transform.position, finalTarget) > 0.6f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.LessOrEqual(Vector3.Distance(go.transform.position, finalTarget), 0.6f, "GridPathfinder should reach the final retargeted destination");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator GridPathfinder_StoppingDistance_Compliance()
        {
            var go = new GameObject("GridStoppingDistanceTest");
            var controller = go.AddComponent<GridPathfinder>();

            Vector3 target = new Vector3(3f, 0f, 0f);
            controller.SetDestination(target);

            float timeout = 5f;
            while (timeout > 0f && !controller.HasReachedDestination)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            float actualDistance = Vector3.Distance(go.transform.position, target);
            Assert.LessOrEqual(actualDistance, 0.5f, "GridPathfinder should stop within stopping distance threshold");
            Assert.IsFalse(controller.IsMoving, "IsMoving should be false when destination reached");

            Object.DestroyImmediate(go);
        }
    }
}
