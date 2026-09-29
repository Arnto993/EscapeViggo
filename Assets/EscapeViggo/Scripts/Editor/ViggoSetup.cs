using EscapeViggo.Viggo;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class ViggoSetup
    {
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/Viggo.asset";
        [MenuItem("Escape Viggo/Setup/Apply M10 Viggo Rest")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var settings = AssetDatabase.LoadAssetAtPath<ViggoSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ViggoSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            var player = PrefabUtility.LoadPrefabContents(NetworkSetup.PrefabPath);
            try
            {
                var stamina = player.GetComponent<ViggoStamina>() ?? player.AddComponent<ViggoStamina>();
                PlayerBuilder.SetReference(stamina, "_settings", settings);
                if (player.GetComponent<ViggoRestFeedback>() == null) player.AddComponent<ViggoRestFeedback>();
                PrefabUtility.SaveAsPrefabAsset(player, NetworkSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M10 Viggo Rest")]
        public static void Validate()
        {
            ScoreSetup.Validate();
            var stamina = AssetDatabase.LoadAssetAtPath<GameObject>(NetworkSetup.PrefabPath).GetComponent<ViggoStamina>();
            WorksiteValidation.Require(stamina != null && stamina.Settings != null, "Missing Viggo stamina/configuration.");
            var settings = stamina.Settings;
            WorksiteValidation.Require(settings.MaximumStamina > 0 && settings.DrainPerSecond > 0 &&
                settings.RecoveryPerSecond > 0 && settings.MovementThreshold > 0 && settings.WalkSpeed > 0 &&
                settings.ChaseSpeed >= settings.WalkSpeed, "Invalid Viggo tuning.");
            Debug.Log("M9 ASSET VALIDATION PASSED.");
            WorksiteValidation.Require(settings.RestSeconds > 0 && settings.RestRecoveryPerSecond > 0 &&
                settings.RestMovementMultiplier >= 0 && settings.RestMovementMultiplier <= 0.25f &&
                stamina.GetComponent<ViggoRestFeedback>() != null, "Missing or invalid rest setup.");
            Debug.Log("M10 ASSET VALIDATION PASSED.");
        }
    }
}
