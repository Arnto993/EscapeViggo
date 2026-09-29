using EscapeViggo.Core;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class ResultsSetup
    {
        [MenuItem("Escape Viggo/Setup/Apply M14 End of Day")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var root = PrefabUtility.LoadPrefabContents(WorkdaySetup.PrefabPath);
            try
            {
                if (root.GetComponent<DailyResults>() == null) root.AddComponent<DailyResults>();
                if (root.GetComponent<DailyResultsPanel>() == null) root.AddComponent<DailyResultsPanel>();
                PrefabUtility.SaveAsPrefabAsset(root, WorkdaySetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            Validate();
        }

        [MenuItem("Escape Viggo/Validation/Validate M14 End of Day")]
        public static void Validate()
        {
            CaptureSetup.Validate();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(WorkdaySetup.PrefabPath);
            WorksiteValidation.Require(root.GetComponent<DailyResults>() != null && root.GetComponent<DailyResultsPanel>() != null,
                "Workday results/panel are not wired.");
            Debug.Log("M14 ASSET VALIDATION PASSED.");
        }
    }
}
