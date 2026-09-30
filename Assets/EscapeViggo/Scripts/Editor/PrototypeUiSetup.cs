using EscapeViggo.Core;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class PrototypeUiSetup
    {
        [MenuItem("Escape Viggo/Setup/Apply M15 Prototype UI")]
        public static void Apply()
        {
            var root = PrefabUtility.LoadPrefabContents(NetworkSetup.PrefabPath);
            try
            {
                if (root.GetComponent<PrototypeHud>() == null) root.AddComponent<PrototypeHud>();
                PrefabUtility.SaveAsPrefabAsset(root, NetworkSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            Validate();
        }
        [MenuItem("Escape Viggo/Validation/Validate M15 Prototype UI")]
        public static void Validate()
        {
            ResultsSetup.Validate();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkSetup.PrefabPath);
            WorksiteValidation.Require(root.GetComponent<PrototypeHud>() != null, "Local HUD missing from network player.");
            Debug.Log("M15 ASSET VALIDATION PASSED.");
        }

        [MenuItem("Escape Viggo/Validation/Validate M16 Local Playtest")]
        public static void ValidatePlaytest()
        {
            Validate();
            var session = Object.FindFirstObjectByType<Networking.LocalNetworkSession>();
            var panel = session.GetComponent<Networking.DevelopmentNetworkPanel>();
            WorksiteValidation.Require(panel != null && new SerializedObject(panel).FindProperty("_session").objectReferenceValue == session,
                "Connection menu is not wired to the saved local session.");
            var scenes = EditorBuildSettings.scenes;
            WorksiteValidation.Require(scenes.Length > 0 && scenes[0].enabled && scenes[0].path == WorksiteBuilder.ScenePath,
                "Windows player must launch the local worksite connection flow.");
            Debug.Log("M16 ASSET VALIDATION PASSED.");
        }
    }
}
