using EscapeViggo.Battery;
using EscapeViggo.Networking;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class BatterySetup
    {
        public const string PrefabPath = "Assets/EscapeViggo/Prefabs/BatteryStation.prefab";
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/Battery.asset";
        [MenuItem("Escape Viggo/Setup/Apply M11 Battery Theft")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var settings = AssetDatabase.LoadAssetAtPath<BatterySettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<BatterySettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            var root = new GameObject("Battery Station");
            root.AddComponent<NetworkObject>();
            var station = root.AddComponent<BatteryStation>();
            root.GetComponent<BoxCollider>().size = new Vector3(2, 1.4f, 1);
            root.GetComponent<BoxCollider>().center = Vector3.up * 0.2f;
            PlayerBuilder.SetReference(station, "_settings", settings);
            var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Station";
            table.transform.SetParent(root.transform, false);
            table.transform.localScale = new Vector3(2, 1, 1);
            Object.DestroyImmediate(table.GetComponent<Collider>());
            table.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Viggo Orange.mat");
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Battery";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localPosition = Vector3.up * 0.7f;
            visual.transform.localScale = new Vector3(0.5f, 0.4f, 0.4f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Task Yellow.mat");
            PlayerBuilder.SetReference(root.AddComponent<BatteryPresentation>(), "_batteryVisual", visual);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            InteractionSetup.RegisterPrefab(prefab);
            var marker = GameObject.Find("Battery Station");
            marker.GetComponent<Renderer>().enabled = false;
            marker.GetComponent<Collider>().enabled = false;
            InteractionSetup.SetEntry(Object.FindFirstObjectByType<NetworkWorldSpawner>(), 2, prefab, marker.transform, false);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M11 Battery")]
        public static void Validate()
        {
            ViggoSetup.Validate();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            var station = prefab != null ? prefab.GetComponent<BatteryStation>() : null;
            WorksiteValidation.Require(station != null && station.Settings != null && station.Settings.TheftSeconds >= 0.5f &&
                station.Settings.HoldTimeoutSeconds >= 0.2f && station.Settings.HoldTimeoutSeconds <= 1, "Invalid battery setup.");
            var entries = new SerializedObject(Object.FindFirstObjectByType<NetworkWorldSpawner>()).FindProperty("_entries");
            var count = 0;
            for (var i = 0; i < entries.arraySize; i++)
                if (entries.GetArrayElementAtIndex(i).FindPropertyRelative("Prefab").objectReferenceValue == prefab.GetComponent<NetworkObject>()) count++;
            WorksiteValidation.Require(count == 1, "Exactly one gameplay battery must be registered in the world.");
            var marker = GameObject.Find("Battery Station");
            WorksiteValidation.Require(!marker.GetComponent<Collider>().enabled && !marker.GetComponent<Renderer>().enabled,
                "Battery station placeholder overlaps runtime station.");
            WorksiteValidation.Require(new SerializedObject(prefab.GetComponent<BatteryPresentation>()).FindProperty("_batteryVisual")
                .objectReferenceValue != null, "Battery visual is missing.");
            Debug.Log("M11 ASSET VALIDATION PASSED.");
        }
    }
}
