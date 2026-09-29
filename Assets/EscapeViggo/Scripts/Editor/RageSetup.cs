using EscapeViggo.Viggo;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class RageSetup
    {
        [MenuItem("Escape Viggo/Setup/Apply M12 Discovery and Rage")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var settings = AssetDatabase.LoadAssetAtPath<ViggoSettings>(ViggoSetup.SettingsPath);
            var root = PrefabUtility.LoadPrefabContents(WorkdaySetup.PrefabPath);
            try
            {
                var rage = root.GetComponent<ViggoRage>() ?? root.AddComponent<ViggoRage>();
                PlayerBuilder.SetReference(rage, "_settings", settings);
                if (root.GetComponent<RagePresentation>() == null) root.AddComponent<RagePresentation>();
                PrefabUtility.SaveAsPrefabAsset(root, WorkdaySetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M12 Discovery and Rage")]
        public static void Validate()
        {
            BatterySetup.Validate();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(WorkdaySetup.PrefabPath);
            var rage = root.GetComponent<ViggoRage>();
            WorksiteValidation.Require(rage != null && rage.Settings != null && rage.Settings.RageSpeed > 0 &&
                root.GetComponent<RagePresentation>() != null, "Missing Rage configuration/presentation.");
            Debug.Log("M12 ASSET VALIDATION PASSED.");
        }
    }
}
