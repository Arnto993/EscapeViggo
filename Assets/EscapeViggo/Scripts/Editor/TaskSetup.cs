using EscapeViggo.Core;
using EscapeViggo.Networking;
using EscapeViggo.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class TaskSetup
    {
        public const string PrefabPath = "Assets/EscapeViggo/Prefabs/SimpleCircuitTask.prefab";
        [MenuItem("Escape Viggo/Setup/Apply M7 Simple Circuit Task")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "Simple Circuit Task";
            root.transform.localScale = new Vector3(0.8f, 1.4f, 0.4f);
            root.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Task Yellow.mat");
            root.AddComponent<NetworkObject>();
            root.AddComponent<SimpleCircuitTask>();
            var panel = root.AddComponent<SimpleCircuitTaskPanel>();
            var label = new GameObject("Circuit Status").AddComponent<TextMesh>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = Vector3.up * 1.3f;
            label.transform.localScale = new Vector3(1.25f, 1 / 1.4f, 2.5f);
            label.text = "SIMPLE CIRCUIT\nAvailable";
            label.fontSize = 48;
            label.characterSize = 0.075f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.gameObject.AddComponent<WorksiteLabel>();
            PlayerBuilder.SetReference(panel, "_label", label);
            PlayerBuilder.SetReference(panel, "_panel", root.GetComponent<Renderer>());
            // Reapplying integration preserves the existing configurable sequence.
            var previous = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (previous != null)
                EditorUtility.CopySerialized(previous.GetComponent<SimpleCircuitTask>(), root.GetComponent<SimpleCircuitTask>());
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            InteractionSetup.RegisterPrefab(prefab);
            var marker = GameObject.Find("Task Location 2");
            marker.GetComponent<Renderer>().enabled = false;
            marker.GetComponent<Collider>().enabled = false;
            GameObject.Find("Task Location 2 Label")?.SetActive(false);
            InteractionSetup.SetEntry(Object.FindFirstObjectByType<NetworkWorldSpawner>(), 1, prefab, marker.transform, false);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M7 Task")]
        public static void Validate()
        {
            InteractionSetup.Validate();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            WorksiteValidation.Require(prefab != null && prefab.GetComponent<SimpleCircuitTask>().HasValidSequence &&
                prefab.GetComponent<SimpleCircuitTaskPanel>() != null, "Missing task configuration or UI.");
            var marker = GameObject.Find("Task Location 2");
            WorksiteValidation.Require(!marker.GetComponent<Collider>().enabled && !marker.GetComponent<Renderer>().enabled,
                "Task placeholder must not overlap the runtime task.");
            Debug.Log("M7 ASSET VALIDATION PASSED.");
        }
    }
}
