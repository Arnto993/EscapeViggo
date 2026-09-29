using EscapeViggo.Player;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class CaptureSetup
    {
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/Capture.asset";
        [MenuItem("Escape Viggo/Setup/Apply M13 Capture")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var settings = AssetDatabase.LoadAssetAtPath<CaptureSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<CaptureSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            var root = PrefabUtility.LoadPrefabContents(NetworkSetup.PrefabPath);
            try
            {
                var capture = root.GetComponent<PlayerCapture>() ?? root.AddComponent<PlayerCapture>();
                PlayerBuilder.SetReference(capture, "_settings", settings);
                var collider = root.GetComponent<BoxCollider>();
                collider.isTrigger = true;
                collider.size = new Vector3(0.7f, 1.8f, 0.7f);
                collider.center = Vector3.up * 0.9f;
                PrefabUtility.SaveAsPrefabAsset(root, NetworkSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            EditorUtility.SetDirty(AssetDatabase.LoadAssetAtPath<Scoring.ScoreSettings>(ScoreSetup.SettingsPath));
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M13 Capture")]
        public static void Validate()
        {
            RageSetup.Validate();
            var capture = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkSetup.PrefabPath).GetComponent<PlayerCapture>();
            WorksiteValidation.Require(capture != null && capture.Settings != null && capture.Settings.Range > 0 &&
                capture.Settings.Range <= capture.GetComponent<Interaction.PlayerInteraction>().Range && capture.Settings.DisableSeconds > 0 &&
                capture.GetComponent<BoxCollider>().isTrigger, "Invalid capture settings or selection collider.");
            Debug.Log("M13 ASSET VALIDATION PASSED.");
        }
    }
}
