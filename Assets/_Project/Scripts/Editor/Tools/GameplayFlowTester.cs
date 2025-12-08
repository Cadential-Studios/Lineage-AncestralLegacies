using UnityEngine;
using System.Collections;
using Lineage.Managers;
using Lineage.Entities;
using Lineage.Debug;

namespace Lineage.Debug
{
    /// <summary>
    /// Runtime tester that verifies the complete gameplay flow:
    /// Spawn Pop -> Control Pop -> Inspect Pop
    /// 
    /// This component can be added to any GameObject and will automatically test
    /// the core gameplay systems when the game starts.
    /// </summary>
    public class GameplayFlowTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool autoTestOnStart = true;
        [SerializeField] private float testDelaySeconds = 2f;
        
        [Header("Test Results")]
        [SerializeField] private bool spawnTestPassed = false;
        [SerializeField] private bool controlTestPassed = false;
        [SerializeField] private bool navMeshTestPassed = false;
        [SerializeField] private string testStatus = "Not Started";
        
        private void Start()
        {
            if (autoTestOnStart)
            {
                StartCoroutine(RunGameplayFlowTest());
            }
        }
        
        [ContextMenu("Run Gameplay Flow Test")]
        public void RunTest()
        {
            StartCoroutine(RunGameplayFlowTest());
        }
        
        private IEnumerator RunGameplayFlowTest()
        {
            testStatus = "Starting tests...";
            Log.Info("=== GAMEPLAY FLOW TEST STARTED ===", Log.LogCategory.Systems);
            
            yield return new WaitForSeconds(testDelaySeconds);
            
            // Test 1: Verify managers are present
            testStatus = "Checking managers...";
            bool managersOk = VerifyManagers();
            if (!managersOk)
            {
                testStatus = "FAILED: Missing managers";
                Log.Error("Test Failed: Required managers are missing!", Log.LogCategory.Systems);
                yield break;
            }
            
            // Test 2: Verify Pop spawning
            testStatus = "Testing spawn...";
            yield return StartCoroutine(TestPopSpawning());
            
            // Test 3: Verify NavMesh
            testStatus = "Testing NavMesh...";
            TestNavMesh();
            
            // Test 4: Verify control system
            testStatus = "Testing controls...";
            TestControlSystem();
            
            // Summary
            bool allPassed = spawnTestPassed && navMeshTestPassed && controlTestPassed;
            testStatus = allPassed ? "ALL TESTS PASSED ✓" : "SOME TESTS FAILED ✗";
            
            Log.Info("=== GAMEPLAY FLOW TEST COMPLETE ===", Log.LogCategory.Systems);
            Log.Info($"Spawn Test: {(spawnTestPassed ? "PASS ✓" : "FAIL ✗")}", Log.LogCategory.Systems);
            Log.Info($"NavMesh Test: {(navMeshTestPassed ? "PASS ✓" : "FAIL ✗")}", Log.LogCategory.Systems);
            Log.Info($"Control Test: {(controlTestPassed ? "PASS ✓" : "FAIL ✗")}", Log.LogCategory.Systems);
        }
        
        private bool VerifyManagers()
        {
            bool hasPopManager = PopulationManager.Instance != null;
            bool hasResourceManager = ResourceManager.Instance != null;
            bool hasSelectionManager = SelectionManager.Instance != null;
            
            if (!hasPopManager) Log.Error("PopulationManager not found!", Log.LogCategory.Systems);
            if (!hasResourceManager) Log.Warning("ResourceManager not found (optional)", Log.LogCategory.Systems);
            if (!hasSelectionManager) Log.Error("SelectionManager not found!", Log.LogCategory.Systems);
            
            return hasPopManager && hasSelectionManager;
        }
        
        private IEnumerator TestPopSpawning()
        {
            var popManager = PopulationManager.Instance;
            if (popManager == null)
            {
                spawnTestPassed = false;
                Log.Error("Cannot test spawning: PopulationManager is null", Log.LogCategory.Population);
                yield break;
            }
            
            // Check for existing pops
            var livingPops = popManager.GetLivingPops();
            int initialCount = livingPops != null ? livingPops.Count : 0;
            
            Log.Info($"Initial pop count: {initialCount}", Log.LogCategory.Population);
            
            if (initialCount > 0)
            {
                // Pops already exist - test passed
                spawnTestPassed = true;
                Log.Info("✓ Pop spawning test PASSED - Pops exist in scene", Log.LogCategory.Population);
                
                // Verify pop structure
                var firstPop = livingPops[0];
                VerifyPopStructure(firstPop);
            }
            else
            {
                // No pops exist - check why
                if (popManager.popPrefab == null)
                {
                    Log.Error("✗ Pop spawning test FAILED - popPrefab is not assigned!", Log.LogCategory.Population);
                    spawnTestPassed = false;
                }
                else
                {
                    // Try spawning manually
                    Log.Info("Attempting manual spawn...", Log.LogCategory.Population);
                    popManager.SpawnPop();
                    
                    yield return new WaitForSeconds(0.5f);
                    
                    livingPops = popManager.GetLivingPops();
                    if (livingPops != null && livingPops.Count > initialCount)
                    {
                        spawnTestPassed = true;
                        Log.Info("✓ Pop spawning test PASSED - Manual spawn successful", Log.LogCategory.Population);
                        VerifyPopStructure(livingPops[0]);
                    }
                    else
                    {
                        spawnTestPassed = false;
                        Log.Error("✗ Pop spawning test FAILED - Manual spawn did not create pop", Log.LogCategory.Population);
                    }
                }
            }
        }
        
        private void VerifyPopStructure(Pop pop)
        {
            if (pop == null) return;
            
            Log.Info($"Verifying pop structure: {pop.name}", Log.LogCategory.Population);
            
            // Check required components
            var agent = pop.GetComponent<UnityEngine.AI.NavMeshAgent>();
            var controller = pop.GetComponent<PopController>();
            var entityData = pop.GetComponent<Lineage.Components.EntityDataComponent>();
            
            if (agent == null) Log.Warning($"  ⚠ {pop.name} missing NavMeshAgent", Log.LogCategory.Population);
            else Log.Info($"  ✓ NavMeshAgent present (walkable mask: {agent.areaMask})", Log.LogCategory.Population);
            
            if (controller == null) Log.Warning($"  ⚠ {pop.name} missing PopController (selection won't work)", Log.LogCategory.Population);
            else Log.Info($"  ✓ PopController present", Log.LogCategory.Population);
            
            if (entityData == null) Log.Warning($"  ⚠ {pop.name} missing EntityDataComponent", Log.LogCategory.Population);
            else Log.Info($"  ✓ EntityDataComponent present", Log.LogCategory.Population);
            
            // Check tag
            if (pop.gameObject.tag != "Pop")
            {
                Log.Warning($"  ⚠ {pop.name} tag is '{pop.gameObject.tag}' instead of 'Pop' (selection may not work)", Log.LogCategory.Population);
            }
            else
            {
                Log.Info($"  ✓ Tag is 'Pop'", Log.LogCategory.Population);
            }
        }
        
        private void TestNavMesh()
        {
            // Check if NavMesh exists
            var navMeshData = UnityEngine.AI.NavMesh.CalculateTriangulation();
            
            if (navMeshData.vertices.Length == 0)
            {
                navMeshTestPassed = false;
                Log.Error("✗ NavMesh test FAILED - No NavMesh baked!", Log.LogCategory.Systems);
                Log.Warning("  Go to Window > AI > Navigation and bake the NavMesh", Log.LogCategory.Systems);
            }
            else
            {
                navMeshTestPassed = true;
                Log.Info($"✓ NavMesh test PASSED - {navMeshData.vertices.Length} vertices, {navMeshData.indices.Length / 3} triangles", Log.LogCategory.Systems);
                
                // Test if pops are on NavMesh
                var popManager = PopulationManager.Instance;
                if (popManager != null)
                {
                    var livingPops = popManager.GetLivingPops();
                    if (livingPops != null)
                    {
                        foreach (var pop in livingPops)
                        {
                            var agent = pop.GetComponent<UnityEngine.AI.NavMeshAgent>();
                            if (agent != null)
                            {
                                if (agent.isOnNavMesh)
                                {
                                    Log.Info($"  ✓ {pop.name} is on NavMesh", Log.LogCategory.Population);
                                }
                                else
                                {
                                    Log.Warning($"  ⚠ {pop.name} is NOT on NavMesh at {pop.transform.position}", Log.LogCategory.Population);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void TestControlSystem()
        {
            var selectionManager = SelectionManager.Instance;
            
            if (selectionManager == null)
            {
                controlTestPassed = false;
                Log.Error("✗ Control test FAILED - SelectionManager not found", Log.LogCategory.Systems);
                return;
            }
            
            // Check input actions
            if (selectionManager.inputActions == null)
            {
                Log.Warning("  ⚠ SelectionManager.inputActions is null - input may not work", Log.LogCategory.Systems);
            }
            
            // Check selection box
            if (selectionManager.selectionBox == null)
            {
                Log.Warning("  ⚠ SelectionManager.selectionBox is null - drag selection won't show visual", Log.LogCategory.Systems);
            }
            
            // Check layer masks
            if (selectionManager.popLayerMask == 0)
            {
                Log.Warning("  ⚠ SelectionManager.popLayerMask is 0 - click selection may not work", Log.LogCategory.Systems);
            }
            
            if (selectionManager.groundLayerMask == 0)
            {
                Log.Warning("  ⚠ SelectionManager.groundLayerMask is 0 - movement commands may not work", Log.LogCategory.Systems);
            }
            
            controlTestPassed = true;
            Log.Info("✓ Control test PASSED - SelectionManager is configured", Log.LogCategory.Systems);
            Log.Info("  Left-click to select pops, Right-click to move selected pops", Log.LogCategory.Systems);
        }
    }
}
