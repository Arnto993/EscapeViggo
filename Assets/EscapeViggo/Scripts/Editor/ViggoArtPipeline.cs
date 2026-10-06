using System;
using System.IO;
using System.Linq;
using EscapeViggo.Presentation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EscapeViggo.Editor
{
    // C5 owns only Viggo assets and the added visual reference on existing players.
    public static class ViggoArtPipeline
    {
        public const string Root = "Assets/EscapeViggo/Art/Characters/Viggo";
        public const string Model = Root + "/Models/Viggo.fbx";
        public const string Prefab = Root + "/PF_Viggo.prefab";

        [MenuItem("Escape Viggo/Art/C5/Prepare and validate Viggo base")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = false; importer.importCameras = false; importer.importLights = false;
            importer.optimizeBones = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            var transforms = model.GetComponentsInChildren<Transform>();
            var description = importer.humanDescription;
            description.human = HumanTrait.BoneName.Where(n => transforms.Any(t => t.name == n.Replace(" ", "")))
                .Select(n => new HumanBone { humanName = n, boneName = n.Replace(" ", ""), limit = new HumanLimit { useDefaultValues = true } }).ToArray();
            // Always refresh the source reference pose, including the corrected leg axes.
            description.skeleton = transforms.Where(t => !t.GetComponent<Renderer>()).Select(t => new SkeletonBone
            { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray();
            // C4's FBX reference contains the shared Idle arm pose. Its Humanoid clips
            // are calibrated against that pose, not BaseHuman's unposed T-pose.
            // Match only arm reference rotations; keep Viggo joint positions and leg axes.
            var sharedReference = ((ModelImporter)AssetImporter.GetAtPath(ElectricianArtPipeline.Model)).humanDescription.skeleton;
            for (var i = 0; i < description.skeleton.Length; i++)
            {
                var name = description.skeleton[i].name;
                if (name.EndsWith("Shoulder", StringComparison.Ordinal) || name.EndsWith("UpperArm", StringComparison.Ordinal) ||
                    name.EndsWith("LowerArm", StringComparison.Ordinal) || name.EndsWith("Hand", StringComparison.Ordinal))
                    description.skeleton[i].rotation = sharedReference.Single(b => b.name == name).rotation;
            }
            importer.humanDescription = description;
            foreach (var material in AssetDatabase.LoadAllAssetsAtPath(Model).OfType<Material>())
            {
                var path = Root + "/Materials/" + material.name + ".mat";
                var target = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!target)
                {
                    target = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    target.SetColor("_BaseColor", material.color); target.SetFloat("_Smoothness", .15f);
                    AssetDatabase.CreateAsset(target, path);
                }
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), material.name), target);
            }
            importer.SaveAndReimport();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
            try
            {
                instance.name = "ViggoVisual";
                var animator = instance.GetComponent<Animator>();
                // Existing electrician clips are a retargeting smoke test, not final Viggo locomotion.
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ElectricianArtPipeline.Controller);
                animator.applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(instance, Prefab);
            }
            finally { Object.DestroyImmediate(instance); }
            foreach (var path in new[] { "Assets/EscapeViggo/Prefabs/Player.prefab", "Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab" })
            {
                var player = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var old = player.transform.Find("ViggoVisual");
                    if (old) Object.DestroyImmediate(old.gameObject);
                    var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), player.transform);
                    visual.name = "ViggoVisual";
                    var presentation = player.GetComponent<ElectricianPresentation>();
                    if (!presentation) throw new InvalidOperationException("C1-C4 presentation must already exist.");
                    PlayerBuilder.SetReference(presentation, "_viggoAnimator", visual.GetComponent<Animator>());
                    visual.SetActive(false);
                    PrefabUtility.SaveAsPrefabAsset(player, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(player); }
            }
            AssetDatabase.SaveAssets(); Validate();
        }

        public static void Validate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            Require(prefab, "Missing Viggo prefab");
            var animator = prefab.GetComponent<Animator>();
            Require(animator && animator.avatar && animator.avatar.isValid && animator.avatar.isHuman, "Invalid Viggo Humanoid");
            Require(!animator.applyRootMotion && animator.runtimeAnimatorController ==
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ElectricianArtPipeline.Controller), "Shared controller/root motion mismatch");
            var meshes = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            Require(meshes.Length == 1 && meshes[0].sharedMesh, "Expected one combined Viggo mesh");
            Require(meshes[0].bones.All(b => b) && meshes[0].sharedMaterials.All(m => m && m.shader.name == "Universal Render Pipeline/Lit"), "Broken Viggo references");
            Require(prefab.GetComponentsInChildren<Collider>().Length == 0, "Presentation must not own colliders");
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            Require(!importer.importAnimation && importer.humanDescription.human.All(b => b.boneName != "Belly"), "C5 secondary bone/animation scope");
            foreach (var path in new[] { "Assets/EscapeViggo/Prefabs/Player.prefab", "Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab" })
            {
                var player = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Require(player.GetComponent<ElectricianPresentation>().ViggoAnimator == player.transform.Find("ViggoVisual").GetComponent<Animator>(), "Missing role visual reference");
            }
            Debug.Log("C5 VIGGO HUMANOID ASSET VALIDATION PASSED");
        }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
