using System;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Editor
{
    // Focused material edit: does not rebuild prefabs or change Avatar/import settings.
    public static class WorkwearArtUpdate
    {
        [MenuItem("Escape Viggo/Art/Apply C4-C5 workwear colors")]
        public static void Apply()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            SetColor(ViggoArtPipeline.Root + "/Materials/MAT_Viggo_Orange.mat", new Color(.85f, .92f, .025f));
            SetColor(ElectricianArtPipeline.Root + "/Materials/MAT_Electrician_Yellow.mat", new Color(.95f, .205f, .012f));
            AssetDatabase.SaveAssets();
            ViggoArtPipeline.Validate();
            ElectricianArtPipeline.Validate();
            Debug.Log("WORKWEAR COLORS AND CHARACTER REFERENCES VALIDATED");
        }

        private static void SetColor(string path, Color color)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material) throw new InvalidOperationException("Missing workwear material: " + path);
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            EditorUtility.SetDirty(material);
            if (material.GetColor("_BaseColor") != color)
                throw new InvalidOperationException("Workwear color mismatch: " + path);
        }
    }
}
