using UnityEngine;
using UnityEditor;
using Lineage.Managers;

namespace Lineage.Editor
{
    /// <summary>
    /// Editor utility to help set up PopulationManager with required references
    /// </summary>
    [CustomEditor(typeof(PopulationManager))]
    public class PopulationManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            PopulationManager manager = (PopulationManager)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Setup Helpers", EditorStyles.boldLabel);
            
            if (manager.popPrefab == null)
            {
                EditorGUILayout.HelpBox("Pop Prefab is not assigned! Click the button below to auto-assign it.", MessageType.Warning);
                
                if (GUILayout.Button("Auto-Assign Pop Prefab"))
                {
                    AssignPopPrefab(manager);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("✓ Pop Prefab is assigned correctly.", MessageType.Info);
            }
        }

        private static void AssignPopPrefab(PopulationManager manager)
        {
            GameObject popPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Entities/Pop/Pop.prefab");
            if (popPrefab != null)
            {
                SerializedObject so = new SerializedObject(manager);
                SerializedProperty prefabProp = so.FindProperty("popPrefab");
                prefabProp.objectReferenceValue = popPrefab;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(manager);
                UnityEngine.Debug.Log("✓ Pop prefab assigned successfully!");
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Could not find Pop prefab at Assets/Entities/Pop/Pop.prefab", "OK");
            }
        }

        [MenuItem("Lineage/Fix/Setup PopulationManager")]
        public static void SetupPopulationManagerFromMenu()
        {
            PopulationManager manager = FindFirstObjectByType<PopulationManager>();
            if (manager == null)
            {
                EditorUtility.DisplayDialog("Not Found", "PopulationManager not found in the scene!", "OK");
                return;
            }

            if (manager.popPrefab == null)
            {
                AssignPopPrefab(manager);
            }
            else
            {
                UnityEngine.Debug.Log("PopulationManager already has Pop prefab assigned.");
            }
        }
    }
}
