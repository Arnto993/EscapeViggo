using EscapeViggo.Core;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class WeekSetup
    {
        [MenuItem("Escape Viggo/Setup/Apply M18 Work Week")]
        public static void Apply()
        {
            var root = PrefabUtility.LoadPrefabContents(WorkdaySetup.PrefabPath);
            try
            {
                if (root.GetComponent<WorkWeek>() == null) root.AddComponent<WorkWeek>();
                PrefabUtility.SaveAsPrefabAsset(root, WorkdaySetup.PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            AssetDatabase.SaveAssets();
            Validate();
        }
        [MenuItem("Escape Viggo/Validation/Validate M18 Work Week")]
        public static void Validate()
        {
            OnlineSessionSetup.Validate();
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(WorkdaySetup.PrefabPath);
            WorksiteValidation.Require(root.GetComponent<WorkWeek>() != null, "WorkWeek missing from saved workday prefab.");
            Debug.Log("M18 ASSET VALIDATION PASSED.");
        }
    }
}
