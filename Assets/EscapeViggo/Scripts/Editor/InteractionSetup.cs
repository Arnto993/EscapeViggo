using System.IO;
using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Networking;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeViggo.Editor
{
    public static class InteractionSetup
    {
        public const string TestPrefabPath = "Assets/EscapeViggo/Prefabs/TestInteraction.prefab";
        [MenuItem("Escape Viggo/Setup/Apply M6 Interaction")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var actions = Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerBuilder.InputPath));
            if (actions.FindAction("Player/Interact") == null)
                actions.FindActionMap("Player").AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            File.WriteAllText(PlayerBuilder.InputPath, actions.ToJson());
            Object.DestroyImmediate(actions);
            AssetDatabase.ImportAsset(PlayerBuilder.InputPath);

            var player = PrefabUtility.LoadPrefabContents(NetworkSetup.PrefabPath);
            try
            {
                var interaction = player.GetComponent<PlayerInteraction>() ?? player.AddComponent<PlayerInteraction>();
                PlayerBuilder.SetReference(interaction, "_camera", player.GetComponentInChildren<Camera>(true));
                PrefabUtility.SaveAsPrefabAsset(player, NetworkSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "Test Interaction";
            cube.transform.localScale = Vector3.one * 0.7f;
            cube.GetComponent<Renderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Task Yellow.mat");
            cube.AddComponent<NetworkObject>();
            var test = cube.AddComponent<DevelopmentTestInteractable>();
            var label = new GameObject("Interaction Label").AddComponent<TextMesh>();
            label.transform.SetParent(cube.transform, false);
            label.transform.localPosition = Vector3.up * 1.4f;
            label.text = "TEST INTERACTION";
            label.fontSize = 48;
            label.characterSize = 0.08f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.gameObject.AddComponent<WorksiteLabel>();
            PlayerBuilder.SetReference(test, "_label", label);
            var prefab = PrefabUtility.SaveAsPrefabAsset(cube, TestPrefabPath);
            Object.DestroyImmediate(cube);
            RegisterPrefab(prefab);
            var session = Object.FindFirstObjectByType<LocalNetworkSession>();
            var spawner = session.GetComponent<NetworkWorldSpawner>() ?? session.gameObject.AddComponent<NetworkWorldSpawner>();
            PlayerBuilder.SetReference(spawner, "_manager", session.Manager);
            var marker = GameObject.Find("Test Interaction Spawn")?.transform ?? new GameObject("Test Interaction Spawn").transform;
            marker.SetParent(session.transform);
            marker.position = new Vector3(-12, 1, -10);
            SetEntry(spawner, 0, prefab, marker, true);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        public static void RegisterPrefab(GameObject prefab)
        {
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            if (!list.Contains(prefab)) list.Add(new NetworkPrefab { Prefab = prefab });
            EditorUtility.SetDirty(list);
        }

        public static void SetEntry(NetworkWorldSpawner spawner, int index, GameObject prefab, Transform location, bool developmentOnly)
        {
            var serialized = new SerializedObject(spawner);
            var entries = serialized.FindProperty("_entries");
            if (entries.arraySize <= index) entries.arraySize = index + 1;
            var entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab.GetComponent<NetworkObject>();
            entry.FindPropertyRelative("Location").objectReferenceValue = location;
            entry.FindPropertyRelative("DevelopmentOnly").boolValue = developmentOnly;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Escape Viggo/Validation/Validate M6 Interaction")]
        public static void Validate()
        {
            WorkdaySetup.Validate();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkSetup.PrefabPath);
            WorksiteValidation.Require(prefab.GetComponent<PlayerInteraction>() != null, "Player has no interaction component.");
            WorksiteValidation.Require(AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerBuilder.InputPath)
                .FindAction("Player/Interact").bindings[0].path == "<Keyboard>/e", "Missing E interaction binding.");
            var spawner = Object.FindFirstObjectByType<NetworkWorldSpawner>();
            WorksiteValidation.Require(spawner != null, "Missing world spawner.");
            var entries = new SerializedObject(spawner).FindProperty("_entries");
            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>("Assets/DefaultNetworkPrefabs.asset");
            for (var i = 0; i < entries.arraySize; i++)
            {
                var entry = entries.GetArrayElementAtIndex(i);
                var networkObject = entry.FindPropertyRelative("Prefab").objectReferenceValue as NetworkObject;
                WorksiteValidation.Require(networkObject != null && list.Contains(networkObject.gameObject) &&
                    entry.FindPropertyRelative("Location").objectReferenceValue != null, "Invalid world spawn entry.");
            }
            Debug.Log("M6 ASSET VALIDATION PASSED.");
        }
    }
}
