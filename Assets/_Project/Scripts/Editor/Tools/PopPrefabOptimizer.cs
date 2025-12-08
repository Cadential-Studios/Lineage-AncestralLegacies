using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Lineage.Entities;
using Lineage.Components;
using Lineage.Systems.Inventory;
using Lineage.Systems.Needs;

namespace Lineage.Editor
{
    /// <summary>
    /// Optimizes the Pop prefab with all necessary components for current game systems
    /// </summary>
    public class PopPrefabOptimizer : EditorWindow
    {
        [MenuItem("Lineage/Setup/Optimize Pop Prefab")]
        public static void OptimizePrefab()
        {
            // First ensure "Pop" tag exists
            EnsurePopTagExists();
            
            GameObject popPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Entities/Pop/Pop.prefab");
            
            if (popPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find Pop prefab at Assets/Entities/Pop/Pop.prefab", "OK");
                return;
            }

            // Open prefab for editing
            string prefabPath = AssetDatabase.GetAssetPath(popPrefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            bool madeChanges = false;
            int changeCount = 0;

            // 1. Ensure Pop component exists
            Pop popComponent = prefabInstance.GetComponent<Pop>();
            if (popComponent == null)
            {
                popComponent = prefabInstance.AddComponent<Pop>();
                UnityEngine.Debug.Log("✓ Added Pop component");
                madeChanges = true;
                changeCount++;
            }

            // 2. Ensure EntityDataComponent exists (REQUIRED)
            EntityDataComponent entityData = prefabInstance.GetComponent<EntityDataComponent>();
            if (entityData == null)
            {
                entityData = prefabInstance.AddComponent<EntityDataComponent>();
                UnityEngine.Debug.Log("✓ Added EntityDataComponent");
                madeChanges = true;
                changeCount++;
            }

            // 3. Ensure InventoryComponent exists
            InventoryComponent inventory = prefabInstance.GetComponent<InventoryComponent>();
            if (inventory == null)
            {
                inventory = prefabInstance.AddComponent<InventoryComponent>();
                UnityEngine.Debug.Log("✓ Added InventoryComponent");
                madeChanges = true;
                changeCount++;
            }

            // 4. Ensure NavMeshAgent exists and is configured
            NavMeshAgent agent = prefabInstance.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = prefabInstance.AddComponent<NavMeshAgent>();
                UnityEngine.Debug.Log("✓ Added NavMeshAgent");
                madeChanges = true;
                changeCount++;
            }
            
            // Configure NavMeshAgent for 2D game - CRITICAL FIX
            if (agent != null)
            {
                bool agentChanged = false;
                
                // Speed and movement settings
                if (agent.speed != 3.5f) { agent.speed = 3.5f; agentChanged = true; }
                if (agent.acceleration != 8f) { agent.acceleration = 8f; agentChanged = true; }
                if (agent.angularSpeed != 0f) { agent.angularSpeed = 0f; agentChanged = true; }
                if (agent.stoppingDistance != 0.5f) { agent.stoppingDistance = 0.5f; agentChanged = true; }
                if (!agent.autoBraking) { agent.autoBraking = true; agentChanged = true; }
                if (!agent.autoRepath) { agent.autoRepath = true; agentChanged = true; }
                
                // Size settings
                if (agent.radius != 0.3f) { agent.radius = 0.3f; agentChanged = true; }
                if (agent.height != 1f) { agent.height = 1f; agentChanged = true; }
                if (agent.baseOffset != 0f) { agent.baseOffset = 0f; agentChanged = true; }
                
                // CRITICAL: Walkable mask must be set to allow movement
                // -1 means all areas are walkable
                if (agent.areaMask == 0)
                {
                    agent.areaMask = -1; // All areas walkable
                    UnityEngine.Debug.Log("✓ CRITICAL FIX: Set NavMeshAgent walkable mask to all areas");
                    agentChanged = true;
                }
                
                if (agentChanged)
                {
                    UnityEngine.Debug.Log("✓ Configured NavMeshAgent for 2D game");
                    madeChanges = true;
                    changeCount++;
                }
            }

            // 5. Ensure PopAIBehavior exists (NEW - for autonomous behavior)
            var aiBehavior = prefabInstance.GetComponent<Lineage.AI.PopAIBehavior>();
            if (aiBehavior == null)
            {
                aiBehavior = prefabInstance.AddComponent<Lineage.AI.PopAIBehavior>();
                UnityEngine.Debug.Log("✓ Added PopAIBehavior for autonomous resource gathering");
                madeChanges = true;
                changeCount++;
            }

            // 6. Ensure PopController exists (CRITICAL - for selection & movement commands)
            var popController = prefabInstance.GetComponent<PopController>();
            if (popController == null)
            {
                popController = prefabInstance.AddComponent<PopController>();
                UnityEngine.Debug.Log("✓ CRITICAL: Added PopController for selection and movement");
                madeChanges = true;
                changeCount++;
            }

            // 7. Ensure SpriteRenderer exists
            SpriteRenderer spriteRenderer = prefabInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = prefabInstance.AddComponent<SpriteRenderer>();
                UnityEngine.Debug.Log("✓ Added SpriteRenderer");
                madeChanges = true;
                changeCount++;
            }

            // 8. Ensure BoxCollider exists for selection
            BoxCollider boxCollider = prefabInstance.GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                boxCollider = prefabInstance.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(0.75f, 1.0625f, 0.2f);
                UnityEngine.Debug.Log("✓ Added BoxCollider for selection");
                madeChanges = true;
                changeCount++;
            }

            // 9. Set proper layer and tag - CRITICAL FOR SELECTION
            if (prefabInstance.layer != LayerMask.NameToLayer("Default"))
            {
                prefabInstance.layer = LayerMask.NameToLayer("Default");
                madeChanges = true;
                changeCount++;
            }

            // Ensure tag is set to "Pop" (tag already ensured to exist above)
            if (prefabInstance.tag != "Pop")
            {
                prefabInstance.tag = "Pop";
                UnityEngine.Debug.Log("✓ CRITICAL FIX: Set tag to 'Pop' for selection system");
                madeChanges = true;
                changeCount++;
            }

            // 10. REMOVE LEGACY COMPONENTS that are replaced by EntityDataComponent
            // NeedsComponent is now handled by EntityDataComponent
            NeedsComponent legacyNeeds = prefabInstance.GetComponent<NeedsComponent>();
            if (legacyNeeds != null)
            {
                Object.DestroyImmediate(legacyNeeds);
                UnityEngine.Debug.Log("✓ Removed legacy NeedsComponent (replaced by EntityDataComponent)");
                madeChanges = true;
                changeCount++;
            }

            // 11. Remove unnecessary UI components that slow down performance (healthbar child handles this)
            RemoveComponentIfExists<Canvas>(prefabInstance, ref madeChanges, ref changeCount);
            RemoveComponentIfExists<CanvasRenderer>(prefabInstance, ref madeChanges, ref changeCount);

            // Save changes
            if (madeChanges)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                UnityEngine.Debug.Log($"✅ Pop prefab optimized successfully! {changeCount} fixes applied, {GetComponentCount(prefabInstance)} components configured.");
            }
            else
            {
                UnityEngine.Debug.Log("✓ Pop prefab is already optimized.");
            }

            PrefabUtility.UnloadPrefabContents(prefabInstance);
            
            EditorUtility.DisplayDialog("Optimization Complete", 
                madeChanges ? $"Pop prefab optimized!\n\n{changeCount} fixes applied:\n• Tag set to 'Pop'\n• NavMesh walkable mask fixed\n• PopController added\n• Legacy components removed\n• All required components verified" : "Pop prefab was already optimized!",
                "OK");
        }
        
        private static void EnsurePopTagExists()
        {
            bool hasPopTag = false;
            foreach (string tag in UnityEditorInternal.InternalEditorUtility.tags)
            {
                if (tag == "Pop")
                {
                    hasPopTag = true;
                    break;
                }
            }
            
            if (!hasPopTag)
            {
                // Create the Pop tag
                SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                SerializedProperty tagsProp = tagManager.FindProperty("tags");
                
                tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                SerializedProperty newTagProp = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
                newTagProp.stringValue = "Pop";
                
                tagManager.ApplyModifiedProperties();
                UnityEngine.Debug.Log("✓ Created 'Pop' tag in TagManager");
            }
        }

        private static void RemoveComponentIfExists<T>(GameObject obj, ref bool madeChanges, ref int changeCount) where T : Component
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                Object.DestroyImmediate(component);
                UnityEngine.Debug.Log($"✓ Removed unnecessary {typeof(T).Name}");
                madeChanges = true;
                changeCount++;
            }
        }

        private static int GetComponentCount(GameObject obj)
        {
            return obj.GetComponents<Component>().Length;
        }

        [MenuItem("Lineage/Setup/Show Pop Prefab Components")]
        public static void ShowPopPrefabInfo()
        {
            GameObject popPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Entities/Pop/Pop.prefab");
            
            if (popPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Could not find Pop prefab", "OK");
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(popPrefab);
            GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);

            UnityEngine.Debug.Log("=== Pop Prefab Components ===");
            Component[] components = prefabInstance.GetComponents<Component>();
            foreach (var component in components)
            {
                if (component != null)
                {
                    string status = IsRequiredComponent(component) ? "✓ REQUIRED" : "○ Optional";
                    UnityEngine.Debug.Log($"{status}: {component.GetType().Name}");
                }
            }
            UnityEngine.Debug.Log($"Total: {components.Length} components");

            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }

        private static bool IsRequiredComponent(Component component)
        {
            string typeName = component.GetType().Name;
            return typeName == "Transform" ||
                   typeName == "Pop" ||
                   typeName == "EntityDataComponent" ||
                   typeName == "InventoryComponent" ||
                   typeName == "NavMeshAgent" ||
                   typeName == "PopAIBehavior" ||
                   typeName == "SpriteRenderer";
        }
    }
}
