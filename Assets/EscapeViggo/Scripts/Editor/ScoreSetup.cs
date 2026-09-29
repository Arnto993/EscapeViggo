using EscapeViggo.Scoring;
using EscapeViggo.Tasks;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class ScoreSetup
    {
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/Scoring.asset";
        [MenuItem("Escape Viggo/Setup/Apply M8 Scoring")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var settings = AssetDatabase.LoadAssetAtPath<ScoreSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<ScoreSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            var player = PrefabUtility.LoadPrefabContents(NetworkSetup.PrefabPath);
            try
            {
                if (player.GetComponent<PlayerScore>() == null) player.AddComponent<PlayerScore>();
                PrefabUtility.SaveAsPrefabAsset(player, NetworkSetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(player); }
            var workday = PrefabUtility.LoadPrefabContents(WorkdaySetup.PrefabPath);
            try
            {
                var scoring = workday.GetComponent<ScoreService>() ?? workday.AddComponent<ScoreService>();
                PlayerBuilder.SetReference(scoring, "_settings", settings);
                PrefabUtility.SaveAsPrefabAsset(workday, WorkdaySetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(workday); }
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M8 Scoring")]
        public static void Validate()
        {
            TaskSetup.Validate();
            WorksiteValidation.Require(AssetDatabase.LoadAssetAtPath<GameObject>(NetworkSetup.PrefabPath).GetComponent<PlayerScore>() != null,
                "Missing player score component.");
            var service = AssetDatabase.LoadAssetAtPath<GameObject>(WorkdaySetup.PrefabPath).GetComponent<ScoreService>();
            WorksiteValidation.Require(service != null && service.Settings != null && service.Settings.CircuitTaskPoints >= 0,
                "Missing scoring configuration or invalid reward.");
            var panel = AssetDatabase.LoadAssetAtPath<GameObject>(TaskSetup.PrefabPath).GetComponent<SimpleCircuitTaskPanel>();
            var serialized = new SerializedObject(panel);
            WorksiteValidation.Require(serialized.FindProperty("_label").objectReferenceValue != null &&
                serialized.FindProperty("_panel").objectReferenceValue != null, "Missing task presentation references.");
            Debug.Log("M8 ASSET VALIDATION PASSED.");
        }
    }
}
