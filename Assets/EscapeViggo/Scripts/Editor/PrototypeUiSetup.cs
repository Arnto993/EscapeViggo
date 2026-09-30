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
    }
}
