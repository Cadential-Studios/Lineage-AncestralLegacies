// File: Assets/_Project/Systems/Movement/Tests/MovementControllerTests.cs
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lineage.Systems.Movement.Tests
{
    public class MovementControllerTests
    {
        [UnityTest]
        public IEnumerator SimpleMovementController_MovesTowardTarget()
        {
            var go = new GameObject("SimpleMovementTest");
            var controller = go.AddComponent<SimpleMovementController>();

            Vector3 target = new Vector3(3f, 0f, 0f);
            controller.SetDestination(target);

            float timeout = 5f;
            while (timeout > 0f && Vector3.Distance(go.transform.position, target) > 0.6f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.LessOrEqual(Vector3.Distance(go.transform.position, target), 0.6f, "SimpleMovementController failed to reach target");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator SimpleMovementController_StopsWhenRequested()
        {
            var go = new GameObject("SimpleMovementStopTest");
            var controller = go.AddComponent<SimpleMovementController>();

            controller.SetDestination(new Vector3(5f, 0f, 0f));
            yield return null; // allow one update to start movement

            controller.Stop();
            Vector3 stoppedPosition = go.transform.position;

            yield return new WaitForSeconds(0.1f);

            Assert.Less(Vector3.Distance(go.transform.position, stoppedPosition), 0.05f, "SimpleMovementController did not stop as expected");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator GridPathfinder_MovesTowardTarget()
        {
            var go = new GameObject("GridPathfinderTest");
            var controller = go.AddComponent<GridPathfinder>();

            Vector3 target = new Vector3(3f, 0f, 0f);
            controller.SetDestination(target);

            float timeout = 5f;
            while (timeout > 0f && Vector3.Distance(go.transform.position, target) > 0.6f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.LessOrEqual(Vector3.Distance(go.transform.position, target), 0.6f, "GridPathfinder failed to reach target");

            Object.DestroyImmediate(go);
        }

        [UnityTest]
        public IEnumerator GridPathfinder_StopsWhenRequested()
        {
            var go = new GameObject("GridPathfinderStopTest");
            var controller = go.AddComponent<GridPathfinder>();

            controller.SetDestination(new Vector3(5f, 0f, 0f));
            yield return null; // allow one update to start movement

            controller.Stop();
            Vector3 stoppedPosition = go.transform.position;

            yield return new WaitForSeconds(0.1f);

            Assert.Less(Vector3.Distance(go.transform.position, stoppedPosition), 0.05f, "GridPathfinder did not stop as expected");

            Object.DestroyImmediate(go);
        }
    }
}
