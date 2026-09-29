using EscapeViggo.Core;
using EscapeViggo.Networking;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class WorkdaySetup
    {
        public const string PrefabPath = "Assets/EscapeViggo/Prefabs/Workday.prefab";
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/Workday.asset";

        [MenuItem("Escape Viggo/Setup/Apply M5 Workday")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var settings = AssetDatabase.LoadAssetAtPath<WorkdaySettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<WorkdaySettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            var root = new GameObject("Workday");
            root.AddComponent<NetworkObject>();
            var workday = root.AddComponent<NetworkWorkday>();
            PlayerBuilder.SetReference(workday, "_settings", settings);
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            if (!list.Contains(prefab)) list.Add(new NetworkPrefab { Prefab = prefab });
            EditorUtility.SetDirty(list);
            PlayerBuilder.SetReference(Object.FindFirstObjectByType<LocalNetworkSession>(), "_workdayPrefab", prefab.GetComponent<NetworkWorkday>());
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M5 Workday")]
        public static void Validate()
        {
            RoleSetup.Validate();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            WorksiteValidation.Require(prefab != null && prefab.GetComponent<NetworkWorkday>().Settings.DayDurationMinutes > 0,
                "Missing workday prefab or invalid duration.");
            WorksiteValidation.Require(AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset").Contains(prefab),
                "Workday prefab is not registered.");
            WorksiteValidation.Require(new SerializedObject(Object.FindFirstObjectByType<LocalNetworkSession>())
                .FindProperty("_workdayPrefab").objectReferenceValue != null, "Session has no workday prefab.");
            Debug.Log("M5 ASSET VALIDATION PASSED.");
        }
    }
}
