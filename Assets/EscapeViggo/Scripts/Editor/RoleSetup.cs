using EscapeViggo.Networking;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class RoleSetup
    {
        [MenuItem("Escape Viggo/Setup/Apply M4 Roles")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var prefab = PrefabUtility.LoadPrefabContents(NetworkSetup.PrefabPath);
            try
            {
                var player = prefab.GetComponent<NetworkPlayer>();
                PlayerBuilder.SetReference(player, "_viggoMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Viggo Orange.mat"));
                PlayerBuilder.SetReference(player, "_electricianMaterial", AssetDatabase.LoadAssetAtPath<Material>("Assets/EscapeViggo/Materials/Worker Blue.mat"));
                PrefabUtility.SaveAsPrefabAsset(prefab, NetworkSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
            var session = Object.FindFirstObjectByType<LocalNetworkSession>();
            var slots = new SerializedObject(session).FindProperty("_spawnPoints");
            var viggo = GameObject.Find("Viggo Spawn").transform.position;
            var electrician = GameObject.Find("Electrician Spawn").transform.position;
            for (var i = 0; i < slots.arraySize; i++)
            {
                var point = (Transform)slots.GetArrayElementAtIndex(i).objectReferenceValue;
                point.name = i == 0 ? "Viggo Network Spawn" : $"Electrician Network Spawn {i}";
                point.position = i == 0 ? viggo : electrician + new Vector3(((i - 1) % 4) * 2, 0, ((i - 1) / 4) * 2);
                point.position = new Vector3(point.position.x, 0.1f, point.position.z);
                point.rotation = i == 0 ? Quaternion.Euler(0, 270, 0) : Quaternion.identity;
            }
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M4 Roles")]
        public static void Validate()
        {
            NetworkSetup.Validate();
            var player = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkSetup.PrefabPath).GetComponent<NetworkPlayer>();
            var serialized = new SerializedObject(player);
            WorksiteValidation.Require(serialized.FindProperty("_viggoMaterial").objectReferenceValue != null &&
                serialized.FindProperty("_electricianMaterial").objectReferenceValue != null, "Missing role materials.");
            var slots = new SerializedObject(Object.FindFirstObjectByType<LocalNetworkSession>()).FindProperty("_spawnPoints");
            WorksiteValidation.Require(slots.arraySize == 8, "Expected eight role spawn slots.");
            for (var i = 0; i < slots.arraySize; i++)
            {
                var point = (Transform)slots.GetArrayElementAtIndex(i).objectReferenceValue;
                var marker = GameObject.Find(i == 0 ? "Viggo Spawn" : "Electrician Spawn").transform.position;
                WorksiteValidation.Require(point != null && Vector3.Distance(point.position, marker) < 7, "Role spawn outside designated area.");
            }
            Debug.Log("M4 ASSET VALIDATION PASSED.");
        }
    }
}
