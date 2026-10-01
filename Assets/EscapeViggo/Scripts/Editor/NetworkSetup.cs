using System;
using System.IO;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class NetworkSetup
    {
        public const string PrefabPath = "Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab";
        // NGO's importer maintains this standard path; moving it makes a second registry.
        private const string ListPath = "Assets/DefaultNetworkPrefabs.asset";

        [MenuItem("Escape Viggo/Setup/Create M3 Local Network (replaces network setup)")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            foreach (var label in UnityEngine.Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
            {
                if (label.transform.parent != null && label.transform.parent.GetComponent<Renderer>() != null)
                {
                    label.name = label.transform.parent.name + " Label";
                    label.transform.SetParent(label.transform.parent.parent, true);
                    label.transform.localScale = Vector3.one;
                }
                if (label.GetComponent<EscapeViggo.Core.WorksiteLabel>() == null)
                    label.gameObject.AddComponent<EscapeViggo.Core.WorksiteLabel>();
            }
            var local = GameObject.Find("Local Player");
            if (local != null) UnityEngine.Object.DestroyImmediate(local);
            var previous = GameObject.Find("Local Network");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PrefabPath);
            var player = UnityEngine.Object.Instantiate(source);
            player.name = "NetworkPlayer";
            player.AddComponent<NetworkObject>();
            var sync = player.AddComponent<NetworkTransform>();
            sync.AuthorityMode = NetworkTransform.AuthorityModes.Owner;
            sync.SyncRotAngleX = false;
            sync.SyncRotAngleZ = false;
            sync.SyncScaleX = sync.SyncScaleY = sync.SyncScaleZ = false;
            sync.Interpolate = true;
            var networkPlayer = player.AddComponent<NetworkPlayer>();
            var camera = player.GetComponentInChildren<Camera>();
            var listener = camera.GetComponent<AudioListener>();
            PlayerBuilder.SetReference(networkPlayer, "_body", player.transform.Find("Body").GetComponent<Renderer>());
            PlayerBuilder.SetReference(networkPlayer, "_camera", camera);
            PlayerBuilder.SetReference(networkPlayer, "_listener", listener);
            player.GetComponent<PlayerInputReader>().enabled = false;
            player.GetComponent<PlayerMovement>().enabled = false;
            player.GetComponent<PlayerLook>().enabled = false;
            player.GetComponent<CharacterController>().enabled = false;
            camera.enabled = listener.enabled = false;
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            UnityEngine.Object.DestroyImmediate(player);

            var list = AssetDatabase.LoadAssetAtPath<NetworkPrefabsList>(ListPath);
            if (list == null)
            {
                list = ScriptableObject.CreateInstance<NetworkPrefabsList>();
                AssetDatabase.CreateAsset(list, ListPath);
            }
            if (!list.Contains(prefab)) list.Add(new NetworkPrefab { Prefab = prefab });
            EditorUtility.SetDirty(list);
            var root = new GameObject("Local Network");
            var transport = root.AddComponent<UnityTransport>();
            var manager = root.AddComponent<NetworkManager>();
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport, PlayerPrefab = prefab, EnableSceneManagement = false,
                ConnectionApproval = true, TickRate = 30
            };
            manager.NetworkConfig.Prefabs.NetworkPrefabsLists.Add(list);
            var session = root.AddComponent<LocalNetworkSession>();
            PlayerBuilder.SetReference(session, "_manager", manager);
            PlayerBuilder.SetReference(session, "_transport", transport);
            var overview = new GameObject("Connection Overview Camera").AddComponent<Camera>();
            overview.tag = "MainCamera";
            overview.transform.SetParent(root.transform);
            overview.transform.position = new Vector3(-15, 9, -15);
            overview.transform.rotation = Quaternion.Euler(27, 30, 0);
            overview.gameObject.AddComponent<AudioListener>();
            PlayerBuilder.SetReference(session, "_overview", overview);
            var serialized = new SerializedObject(session);
            var slots = serialized.FindProperty("_spawnPoints");
            slots.arraySize = 8;
            for (var i = 0; i < slots.arraySize; i++)
            {
                var spawn = new GameObject($"Development Spawn {i + 1}").transform;
                spawn.SetParent(root.transform);
                spawn.position = new Vector3(-15 + (i % 4) * 2, 0.1f, -12 + (i / 4) * 2);
                slots.GetArrayElementAtIndex(i).objectReferenceValue = spawn;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var panel = root.AddComponent<DevelopmentNetworkPanel>();
            PlayerBuilder.SetReference(panel, "_session", session);
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.defaultScreenWidth = 1280;
            PlayerSettings.defaultScreenHeight = 720;
            EditorSceneManager.SaveScene(scene);
            // Remove the superseded explicit list after the scene points at NGO's default registry.
            AssetDatabase.DeleteAsset("Assets/EscapeViggo/ScriptableObjects/NetworkPrefabs.asset");
            AssetDatabase.DeleteAsset("Assets/EscapeViggo/ScriptableObjects/DefaultNetworkPrefabs.asset");
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M3 Network Setup")]
        public static void Validate()
        {
            WorksiteValidation.Validate();
            PlayerBuilder.Validate();
            var manager = UnityEngine.Object.FindFirstObjectByType<NetworkManager>();
            WorksiteValidation.Require(manager != null && manager.NetworkConfig.NetworkTransport is UnityTransport, "Missing network transport.");
            WorksiteValidation.Require(!manager.NetworkConfig.EnableSceneManagement && manager.NetworkConfig.ConnectionApproval,
                "Local scene/approval configuration changed.");
            var prefab = manager.NetworkConfig.PlayerPrefab;
            WorksiteValidation.Require(prefab != null && prefab.GetComponent<NetworkObject>() != null, "Missing network player.");
            WorksiteValidation.Require(prefab.GetComponent<NetworkTransform>().AuthorityMode == NetworkTransform.AuthorityModes.Owner,
                "Expected owner movement synchronization.");
            WorksiteValidation.Require(!prefab.GetComponent<PlayerInputReader>().enabled && !prefab.GetComponent<PlayerMovement>().enabled &&
                !prefab.GetComponent<PlayerLook>().enabled && !prefab.GetComponentInChildren<Camera>(true).enabled &&
                !prefab.GetComponentInChildren<AudioListener>(true).enabled, "Network prefab must start with local components disabled.");
            WorksiteValidation.Require(GameObject.Find("Local Player") == null, "Offline player must not coexist with network players.");
            Debug.Log("M3 ASSET VALIDATION PASSED.");
        }

        [MenuItem("Escape Viggo/Build/Windows Development Player")]
        public static void Build()
        {
            WeekSetup.Validate();
            Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { WorksiteBuilder.ScenePath },
                locationPathName = "Builds/Windows/EscapeViggo.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (result.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Windows development build failed: " + result.summary.result);
            CleanupBuildTestData();
            Debug.Log("WINDOWS DEVELOPMENT BUILD PASSED.");
        }

        public static void CleanupBuildTestData()
        {
            // The transitive Performance Testing package writes these on ordinary builds.
            AssetDatabase.DeleteAsset("Assets/Resources/PerformanceTestRunInfo.json");
            AssetDatabase.DeleteAsset("Assets/Resources/PerformanceTestRunSettings.json");
            if (Directory.Exists("Assets/Resources") && Directory.GetFileSystemEntries("Assets/Resources").Length == 0)
                AssetDatabase.DeleteAsset("Assets/Resources");
        }
    }
}
