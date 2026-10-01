using System;
using System.IO;
using System.Linq;
using EscapeViggo.Presentation;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EscapeViggo.Editor
{
    public static class ElectricianArtPipeline
    {
        public const string Root = "Assets/EscapeViggo/Art/Characters/Electrician";
        public const string Model = Root + "/Models/Electrician.fbx";
        public const string Prefab = Root + "/PF_Electrician.prefab";
        public const string Controller = Root + "/Electrician.controller";
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }

        [MenuItem("Escape Viggo/Art/C1-C4/Prepare and validate electrician")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true; importer.importCameras = false; importer.importLights = false;
            importer.optimizeBones = false; // Keep the named Humanoid toe endpoints, even without mesh weights.
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            var description = importer.humanDescription;
            description.human = HumanTrait.BoneName.Where(n => model.GetComponentsInChildren<Transform>().Any(t => t.name == n.Replace(" ", "")))
                .Select(n => new HumanBone { humanName = n, boneName = n.Replace(" ", ""), limit = new HumanLimit { useDefaultValues = true } }).ToArray();
            // A regenerated rig must not retain an enforced pose from an older skeleton.
            description.skeleton = SourceSkeleton(model);
            importer.humanDescription = description;
            foreach (var material in AssetDatabase.LoadAllAssetsAtPath(Model).OfType<Material>())
            {
                var path = Root + "/Materials/" + material.name + ".mat";
                var target = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (!target) { target = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(target, path); }
                target.SetColor("_BaseColor", material.color); target.SetFloat("_Smoothness", .15f);
                EditorUtility.SetDirty(target);
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), material.name), target);
            }
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = new[] { "Idle", "Walk", "Run" }.First(n => clip.name.EndsWith(n, StringComparison.Ordinal));
                clip.loopTime = true; clip.loopPose = true;
                clip.lockRootRotation = true; clip.lockRootHeightY = true; clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true; clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
            if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(Controller);
            foreach (var parameter in controller.parameters) controller.RemoveParameter(parameter);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var machine = controller.layers[0].stateMachine;
            foreach (var state in machine.states) machine.RemoveState(state.state);
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(Controller).OfType<BlendTree>()) Object.DestroyImmediate(old, true);
            var blend = new BlendTree { name = "InPlaceLocomotion", blendType = BlendTreeType.Simple1D, blendParameter = "Speed", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(blend, controller);
            var importedClips = AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")).ToArray();
            for (var i = 0; i < 3; i++) blend.AddChild(importedClips.Single(c => c.name == new[] { "Idle", "Walk", "Run" }[i]), i);
            machine.AddState("Locomotion").motion = blend;
            EditorUtility.SetDirty(controller);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
            instance.name = "ElectricianVisual";
            var animator = instance.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller; animator.applyRootMotion = false;
            PrefabUtility.SaveAsPrefabAsset(instance, Prefab); Object.DestroyImmediate(instance);
            foreach (var path in new[] { "Assets/EscapeViggo/Prefabs/Player.prefab", "Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab" })
            {
                var player = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    var old = player.transform.Find("ElectricianVisual");
                    if (old) Object.DestroyImmediate(old.gameObject);
                    var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), player.transform);
                    visual.name = "ElectricianVisual";
                    var presentation = player.GetComponent<ElectricianPresentation>() ?? player.AddComponent<ElectricianPresentation>();
                    PlayerBuilder.SetReference(presentation, "_animator", visual.GetComponent<Animator>());
                    PlayerBuilder.SetReference(presentation, "_placeholder", player.transform.Find("Body").GetComponent<Renderer>());
                    PrefabUtility.SaveAsPrefabAsset(player, path);
                }
                finally { PrefabUtility.UnloadPrefabContents(player); }
            }
            AssetDatabase.SaveAssets(); Validate();
        }

        [MenuItem("Escape Viggo/Art/C1-C4/Refresh Avatar from source rig")]
        public static void RefreshRigReferencePose()
        {
            AssetDatabase.ImportAsset(Model, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            importer.optimizeBones = false;
            var description = importer.humanDescription;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Model);
            var names = model.GetComponentsInChildren<Transform>().Select(t => t.name).ToArray();
            description.human = HumanTrait.BoneName.Where(n => names.Contains(n.Replace(" ", "")))
                .Select(n => new HumanBone { humanName = n, boneName = n.Replace(" ", ""), limit = new HumanLimit { useDefaultValues = true } }).ToArray();
            description.skeleton = SourceSkeleton(model);
            importer.humanDescription = description;
            importer.SaveAndReimport();
            Validate();
        }

        private static SkeletonBone[] SourceSkeleton(GameObject model) =>
            model.GetComponentsInChildren<Transform>().Where(t => !t.GetComponent<Renderer>()).Select(t => new SkeletonBone
            {
                name = t.name, position = t.localPosition,
                rotation = t.localRotation, scale = t.localScale
            }).ToArray();

        public static void Validate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab);
            Require(prefab, "Missing electrician prefab");
            var animator = prefab.GetComponent<Animator>();
            Require(animator && animator.avatar && animator.avatar.isValid && animator.avatar.isHuman, "Invalid Humanoid Avatar");
            Require(!animator.applyRootMotion && animator.runtimeAnimatorController, "Missing in-place controller");
            var meshes = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            Require(meshes.Length == 1, "Expected one combined skinned mesh");
            foreach (var mesh in meshes)
            {
                Require(mesh.sharedMesh && mesh.bones.All(b => b) && mesh.sharedMaterials.All(m => m && m.shader.name == "Universal Render Pipeline/Lit"), "Broken mesh/bone/material reference");
                Require(mesh.sharedMesh.triangles.Length / 3 < 8000, "Unexpected geometry density");
            }
            foreach (var name in new[] { "Idle", "Walk", "Run" })
            {
                var clip = AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Single(c => c.name == name);
                Require(clip.isHumanMotion && clip.isLooping && clip.length > .5f, "Invalid Humanoid clip " + name);
            }
            Require(prefab.GetComponentsInChildren<Collider>().Length == 0, "Visual must not own colliders");
            Require(prefab.GetComponentsInChildren<Component>(true).All(c => c), "Missing component");
            Debug.Log("C1-C4 HUMANOID ASSET VALIDATION PASSED");
        }
    }
}
