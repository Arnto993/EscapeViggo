using EscapeViggo.Core;
using EscapeViggo.Networking;
using EscapeViggo.Scoring;
using EscapeViggo.Tasks;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class ElectricalTaskSetup
    {
        public const string VoltagePath = "Assets/EscapeViggo/Prefabs/VoltageTask.prefab";
        public const string OutletPath = "Assets/EscapeViggo/Prefabs/OutletTask.prefab";
        public const string BreakerPath = "Assets/EscapeViggo/Prefabs/BreakerTask.prefab";
        [MenuItem("Escape Viggo/Setup/Apply M19 Electrical Tasks")]
        public static void Apply()
        {
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var voltage = Create(VoltagePath, ElectricalTaskKind.VoltageMeasurement, ElectricalTaskKind.OutletReplacement);
            var outlet = Create(OutletPath, ElectricalTaskKind.OutletReplacement, ElectricalTaskKind.VoltageMeasurement);
            var breaker = Create(BreakerPath, ElectricalTaskKind.BreakerInstallation);
            var locations = new[] { 1, 3, 4, 5 };
            for (var i = 0; i < locations.Length; i++)
            {
                var marker = GameObject.Find("Task Location " + locations[i]);
                marker.GetComponent<Renderer>().enabled = false;
                marker.GetComponent<Collider>().enabled = false;
                GameObject.Find("Task Location " + locations[i] + " Label")?.SetActive(false);
                InteractionSetup.SetEntry(Object.FindFirstObjectByType<NetworkWorldSpawner>(), 3 + i,
                    i == 2 ? breaker : i % 2 == 0 ? voltage : outlet, marker.transform, false);
            }
            var scores = AssetDatabase.LoadAssetAtPath<ScoreSettings>("Assets/EscapeViggo/ScriptableObjects/Scoring.asset");
            EditorUtility.SetDirty(scores);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }
        private static GameObject Create(string path, params ElectricalTaskKind[] layout)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = layout[0].ToString();
            root.transform.localScale = new Vector3(0.8f, 1.4f, 0.4f);
            root.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Task Yellow.mat");
            root.AddComponent<NetworkObject>();
            var task = root.AddComponent<ElectricalWorkTask>();
            var serialized = new SerializedObject(task);
            var types = serialized.FindProperty("_dailyTypes"); types.arraySize = layout.Length;
            for (var i = 0; i < layout.Length; i++) types.GetArrayElementAtIndex(i).enumValueIndex = (int)layout[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var panel = root.AddComponent<ElectricalTaskPanel>();
            var label = new GameObject("Task Status").AddComponent<TextMesh>();
            label.transform.SetParent(root.transform, false);
            label.transform.localPosition = Vector3.up * 1.3f;
            label.transform.localScale = new Vector3(1.25f, 1 / 1.4f, 2.5f);
            label.text = layout[0].ToString(); label.fontSize = 48; label.characterSize = 0.065f;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center;
            label.gameObject.AddComponent<WorksiteLabel>();
            PlayerBuilder.SetReference(panel, "_label", label);
            PlayerBuilder.SetReference(panel, "_panel", root.GetComponent<Renderer>());
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            InteractionSetup.RegisterPrefab(prefab);
            return prefab;
        }
        [MenuItem("Escape Viggo/Validation/Validate M19 Electrical Tasks")]
        public static void Validate()
        {
            WeekSetup.Validate();
            foreach (var path in new[] { VoltagePath, OutletPath, BreakerPath })
            {
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                WorksiteValidation.Require(root != null && root.GetComponent<ElectricalWorkTask>().HasValidLayout && root.GetComponent<ElectricalTaskPanel>() != null,
                    "Electrical task prefab/layout/UI missing: " + path);
                var panel = new SerializedObject(root.GetComponent<ElectricalTaskPanel>());
                WorksiteValidation.Require(panel.FindProperty("_label").objectReferenceValue != null && panel.FindProperty("_panel").objectReferenceValue != null, "Task presentation references missing.");
            }
            var entries = new SerializedObject(Object.FindFirstObjectByType<NetworkWorldSpawner>()).FindProperty("_entries");
            var taskCount = 0;
            for (var i = 0; i < entries.arraySize; i++)
            {
                var prefab = entries.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab").objectReferenceValue as NetworkObject;
                if (prefab != null && prefab.GetComponent<WorkTask>() != null) taskCount++;
            }
            WorksiteValidation.Require(taskCount == 5, "Expected five wired work tasks in the saved scene.");
            Debug.Log("M19 ASSET VALIDATION PASSED.");
        }
    }
}
