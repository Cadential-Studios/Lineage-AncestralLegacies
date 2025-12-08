using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Lineage.Managers;
using Lineage.Entities;
using Unity.AI.Navigation;

namespace Lineage.Editor
{
    /// <summary>
    /// Verifies that the game scene is set up correctly for gameplay
    /// </summary>
    public class GameSetupVerifier : EditorWindow
    {
        private static bool autoBakeNavMesh = true;
        
        [MenuItem("Lineage/Setup/Verify Game Setup")]
        public static void ShowWindow()
        {
            GetWindow<GameSetupVerifier>("Game Setup Verifier");
        }

        [MenuItem("Lineage/Setup/Quick Fix All")]
        public static void QuickFixAll()
        {
            int fixedCount = 0;
            
            // Ensure Pop tag exists
            if (!TagExists("Pop"))
            {
                AddTag("Pop");
                UnityEngine.Debug.Log("✓ Created 'Pop' tag");
                fixedCount++;
            }
            
            // Fix PopulationManager
            var popManager = FindFirstObjectByType<PopulationManager>(FindObjectsInactive.Exclude);
            if (popManager != null)
            {
                if (popManager.popPrefab == null)
                {
                    GameObject popPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Entities/Pop/Pop.prefab");
                    if (popPrefab != null)
                    {
                        SerializedObject so = new SerializedObject(popManager);
                        SerializedProperty prefabProp = so.FindProperty("popPrefab");
                        prefabProp.objectReferenceValue = popPrefab;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(popManager);
                        UnityEngine.Debug.Log("✓ Fixed: PopulationManager now has Pop prefab assigned");
                        fixedCount++;
                    }
                }
                
                if (popManager.spawnPoint == null)
                {
                    GameObject chunksObj = GameObject.Find("Chunks");
                    if (chunksObj != null)
                    {
                        SerializedObject so = new SerializedObject(popManager);
                        SerializedProperty spawnProp = so.FindProperty("spawnPoint");
                        spawnProp.objectReferenceValue = chunksObj.transform;
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(popManager);
                        UnityEngine.Debug.Log("✓ Fixed: PopulationManager spawn point set to Chunks");
                        fixedCount++;
                    }
                }
            }
            
            // Bake NavMesh if enabled and not baked
            if (autoBakeNavMesh)
            {
                bool navMeshBaked = BakeNavMeshIfNeeded();
                if (navMeshBaked)
                {
                    fixedCount++;
                }
            }
            
            if (fixedCount > 0)
            {
                EditorUtility.DisplayDialog("Setup Fixed", $"Fixed {fixedCount} issue(s). Try playing the game now!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("No Issues", "No issues found or already fixed!", "OK");
            }
        }

        /// <summary>
        /// Bakes the NavMesh if it hasn't been baked yet
        /// </summary>
        /// <returns>True if NavMesh was baked, false if already baked or error occurred</returns>
        [MenuItem("Lineage/Setup/Bake NavMesh")]
        public static bool BakeNavMeshIfNeeded()
        {
            // Check if NavMesh is already baked
            var navMeshData = UnityEngine.AI.NavMesh.CalculateTriangulation();
            if (navMeshData.vertices.Length > 0)
            {
                UnityEngine.Debug.Log("NavMesh already baked. Skipping.");
                return false;
            }

            // Try to find existing NavMeshSurface
            NavMeshSurface navMeshSurface = FindFirstObjectByType<NavMeshSurface>(FindObjectsInactive.Exclude);
            
            if (navMeshSurface == null)
            {
                // Try to find a suitable ground object to add NavMeshSurface to
                GameObject groundObject = GameObject.Find("Ground");
                if (groundObject == null)
                {
                    groundObject = GameObject.Find("Terrain");
                }
                if (groundObject == null)
                {
                    groundObject = GameObject.Find("Floor");
                }
                
                if (groundObject != null)
                {
                    navMeshSurface = groundObject.AddComponent<NavMeshSurface>();
                    navMeshSurface.collectObjects = CollectObjects.All;
                    UnityEngine.Debug.Log($"✓ Added NavMeshSurface to {groundObject.name}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("⚠ Could not find Ground/Terrain/Floor object to add NavMeshSurface. Please add manually and bake.");
                    return false;
                }
            }

            // Bake the NavMesh
            try
            {
                navMeshSurface.BuildNavMesh();
                UnityEngine.Debug.Log("✓ NavMesh baked successfully!");
                return true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ Failed to bake NavMesh: {ex.Message}");
                return false;
            }
        }

        private void OnGUI()
        {
            GUILayout.Label("Game Setup Verification", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Auto-bake NavMesh toggle
            autoBakeNavMesh = EditorGUILayout.Toggle("Auto-Bake NavMesh", autoBakeNavMesh);
            EditorGUILayout.HelpBox("When enabled, 'Quick Fix All' will automatically bake the NavMesh if needed.", MessageType.Info);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Quick Fix All Issues", GUILayout.Height(40)))
            {
                QuickFixAll();
            }
            
            if (GUILayout.Button("Bake NavMesh Now", GUILayout.Height(30)))
            {
                BakeNavMeshIfNeeded();
            }
            
            EditorGUILayout.Space();
            GUILayout.Label("Manual Checks:", EditorStyles.boldLabel);
            
            // Check PopulationManager
            var popManager = FindFirstObjectByType<PopulationManager>(FindObjectsInactive.Exclude);
            if (popManager == null)
            {
                EditorGUILayout.HelpBox("❌ PopulationManager not found in scene!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ PopulationManager found in scene", MessageType.Info);
                
                if (popManager.popPrefab == null)
                {
                    EditorGUILayout.HelpBox("❌ Pop Prefab is NOT assigned!", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox($"✓ Pop Prefab assigned: {popManager.popPrefab.name}", MessageType.Info);
                }
                
                if (popManager.spawnPoint == null)
                {
                    EditorGUILayout.HelpBox("⚠ Spawn Point is not assigned (will auto-detect)", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox($"✓ Spawn Point: {popManager.spawnPoint.name}", MessageType.Info);
                }
            }
            
            EditorGUILayout.Space();
            
            // Check ResourceManager
            var resourceManager = FindFirstObjectByType<ResourceManager>(FindObjectsInactive.Exclude);
            if (resourceManager == null)
            {
                EditorGUILayout.HelpBox("⚠ ResourceManager not found", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ ResourceManager found", MessageType.Info);
            }
            
            // Check Camera
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                EditorGUILayout.HelpBox("❌ Main Camera not found!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Main Camera found", MessageType.Info);
            }
            
            // Check for Chunks
            var chunks = GameObject.Find("Chunks");
            if (chunks == null)
            {
                EditorGUILayout.HelpBox("⚠ Chunks object not found for spawn location", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Chunks object found for spawning", MessageType.Info);
            }
            
            // Check for Pop prefab
            GameObject popPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Entities/Pop/Pop.prefab");
            if (popPrefab == null)
            {
                EditorGUILayout.HelpBox("❌ Pop prefab not found at Assets/Entities/Pop/Pop.prefab!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Pop prefab exists", MessageType.Info);
                
                // Check Pop prefab has Pop tag
                if (popPrefab.tag != "Pop")
                {
                    EditorGUILayout.HelpBox("⚠ Pop prefab tag is not 'Pop'. Run 'Optimize Pop Prefab' to fix.", MessageType.Warning);
                }
                
                // Check Pop prefab has required components
                var prefabPop = popPrefab.GetComponent<Pop>();
                var prefabController = popPrefab.GetComponent<PopController>();
                if (prefabPop == null || prefabController == null)
                {
                    EditorGUILayout.HelpBox("⚠ Pop prefab missing components. Run 'Optimize Pop Prefab' to fix.", MessageType.Warning);
                }
            }
            
            // Check NavMesh
            EditorGUILayout.Space();
            GUILayout.Label("NavMesh Status:", EditorStyles.boldLabel);
            
            var navMeshData = UnityEngine.AI.NavMesh.CalculateTriangulation();
            if (navMeshData.vertices.Length == 0)
            {
                EditorGUILayout.HelpBox("❌ NavMesh not baked! Go to Window > AI > Navigation and bake.", MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox($"✓ NavMesh baked ({navMeshData.vertices.Length} vertices)", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Open NavMesh Window"))
            {
                EditorApplication.ExecuteMenuItem("Window/AI/Navigation");
            }
            
            if (GUILayout.Button("Optimize Pop Prefab"))
            {
                PopPrefabOptimizer.OptimizePrefab();
            }
        }
        
        private static bool TagExists(string tag)
        {
            foreach (string t in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (t == tag) return true;
            }
            return false;
        }
        
        private static void AddTag(string tag)
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            // Add the tag
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTagProp.stringValue = tag;
            
            tagManager.ApplyModifiedProperties();
        }
    }
}
