using EscapeViggo.Core;
using EscapeViggo.Networking;
using EscapeViggo.Scoring;
using EscapeViggo.Viggo;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class DistractionSetup
    {
        public const string PrefabPath = "Assets/EscapeViggo/Prefabs/PowerControl.prefab";
        [MenuItem("Escape Viggo/Setup/Apply M20 Distractions")]
        public static void Apply()
        {
            const string settingsPath = "Assets/EscapeViggo/ScriptableObjects/Distractions.asset";
            var settings = AssetDatabase.LoadAssetAtPath<DistractionSettings>(settingsPath);
            if (settings == null) { settings = ScriptableObject.CreateInstance<DistractionSettings>(); AssetDatabase.CreateAsset(settings, settingsPath); }
            var root = PrefabUtility.LoadPrefabContents(WorkdaySetup.PrefabPath);
            try
            {
                var service = root.GetComponent<WorkDistractions>() ?? root.AddComponent<WorkDistractions>();
                PlayerBuilder.SetReference(service, "_settings", settings);
                PrefabUtility.SaveAsPrefabAsset(root, WorkdaySetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                root = GameObject.CreatePrimitive(PrimitiveType.Cube);
                root.name = "Power Control";
                root.transform.localScale = new Vector3(0.8f, 1.4f, 0.4f);
                root.AddComponent<NetworkObject>(); root.AddComponent<PowerControl>();
                var label = new GameObject("Power Label").AddComponent<TextMesh>();
                label.transform.SetParent(root.transform, false);
                label.transform.localPosition = Vector3.up * 1.3f;
                label.text = "POWER CONTROL"; label.fontSize = 48; label.characterSize = 0.08f;
                label.anchor = TextAnchor.MiddleCenter;
                label.gameObject.AddComponent<WorksiteLabel>();
                prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                Object.DestroyImmediate(root);
            }
            InteractionSetup.RegisterPrefab(prefab);
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var marker = GameObject.Find("Power Control Spawn") ?? new GameObject("Power Control Spawn");
            marker.transform.position = new Vector3(-1, 0.8f, -10);
            InteractionSetup.SetEntry(Object.FindFirstObjectByType<NetworkWorldSpawner>(), 7, prefab, marker.transform, false);
            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<ScoreSettings>("Assets/EscapeViggo/ScriptableObjects/Scoring.asset"));
            EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets(); Validate();
        }
        [MenuItem("Escape Viggo/Validation/Validate M20 Distractions")]
        public static void Validate()
        {
            ElectricalTaskSetup.Validate();
            var service = AssetDatabase.LoadAssetAtPath<GameObject>(WorkdaySetup.PrefabPath).GetComponent<WorkDistractions>();
            WorksiteValidation.Require(service != null && service.Settings != null, "Distraction service/configuration missing.");
            var entries = new SerializedObject(Object.FindFirstObjectByType<NetworkWorldSpawner>()).FindProperty("_entries");
            var power = entries.arraySize > 7 ? entries.GetArrayElementAtIndex(7).FindPropertyRelative("Prefab").objectReferenceValue as NetworkObject : null;
            WorksiteValidation.Require(power != null && power.GetComponent<PowerControl>() != null, "Power control not wired into worksite.");
            Debug.Log("M20 ASSET VALIDATION PASSED.");
        }
    }
}
