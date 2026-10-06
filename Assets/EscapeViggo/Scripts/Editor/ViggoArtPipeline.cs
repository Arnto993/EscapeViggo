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
    // Owns Viggo art assets and visual references, never gameplay configuration.
    public static class ViggoArtPipeline
    {
        public const string Root = "Assets/EscapeViggo/Art/Characters/Viggo";
        public const string Model = Root + "/Models/Viggo.fbx";
        public const string Prefab = Root + "/PF_Viggo.prefab";
        public const string Controller = Root + "/Viggo.controller";

        [MenuItem("Escape Viggo/Art/C6-C7/Prepare and validate Viggo animations")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            importer.globalScale = 1; importer.useFileScale = true; importer.bakeAxisConversion = true;
            importer.animationType = ModelImporterAnimationType.Human;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation = true; importer.importCameras = false; importer.importLights = false;
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
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                clip.name = new[] { "Idle", "Walk", "Run", "Javell" }.Single(n => clip.name.EndsWith(n, StringComparison.Ordinal));
                clip.loopTime = true; clip.loopPose = true;
                clip.lockRootRotation = true; clip.lockRootHeightY = true; clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true; clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
            var controller = CreateController();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Model));
            try
            {
                instance.name = "ViggoVisual";
                var animator = instance.GetComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                PrefabUtility.SaveAsPrefabAsset(instance, Prefab);
            }
            finally { Object.DestroyImmediate(instance); }
            foreach (var path in new[] { "Assets/EscapeViggo/Prefabs/Player.prefab", "Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab" })
            {
                var player = PrefabUtility.LoadPrefabContents(path);
                try
                {
                    // Existing nested prefabs inherit the new controller; preserve their file IDs.
                    var existing = player.transform.Find("ViggoVisual");
                    var visual = existing ? existing.gameObject :
                        (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefab), player.transform);
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
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(Controller), "Viggo controller/root motion mismatch");
            var meshes = prefab.GetComponentsInChildren<SkinnedMeshRenderer>();
            Require(meshes.Length == 1 && meshes[0].sharedMesh, "Expected one combined Viggo mesh");
            Require(meshes[0].bones.All(b => b) && meshes[0].sharedMaterials.All(m => m && m.shader.name == "Universal Render Pipeline/Lit"), "Broken Viggo references");
            Require(prefab.GetComponentsInChildren<Collider>().Length == 0, "Presentation must not own colliders");
            var importer = (ModelImporter)AssetImporter.GetAtPath(Model);
            Require(importer.importAnimation && importer.humanDescription.human.All(b => b.boneName != "Belly"), "Secondary bone/animation configuration");
            foreach (var name in new[] { "Idle", "Walk", "Run", "Javell" })
            {
                var clip = AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Single(c => c.name == name);
                Require(clip.isHumanMotion && clip.isLooping && clip.length > .5f, "Invalid Viggo clip " + name);
            }
            foreach (var path in new[] { "Assets/EscapeViggo/Prefabs/Player.prefab", "Assets/EscapeViggo/Prefabs/NetworkPlayer.prefab" })
            {
                var player = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Require(player.GetComponent<ElectricianPresentation>().ViggoAnimator == player.transform.Find("ViggoVisual").GetComponent<Animator>(), "Missing role visual reference");
            }
            Debug.Log("C6-C7 VIGGO HUMANOID ASSET VALIDATION PASSED");
        }

        private static AnimatorController CreateController()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(Controller);
            if (!controller) controller = AnimatorController.CreateAnimatorControllerAtPath(Controller);
            foreach (var parameter in controller.parameters) controller.RemoveParameter(parameter);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Resting", AnimatorControllerParameterType.Bool);
            var machine = controller.layers[0].stateMachine;
            foreach (var state in machine.states) machine.RemoveState(state.state);
            foreach (var old in AssetDatabase.LoadAllAssetsAtPath(Controller).OfType<BlendTree>()) Object.DestroyImmediate(old, true);
            var blend = new BlendTree { name = "HeavyLocomotion", blendType = BlendTreeType.Simple1D,
                blendParameter = "Speed", useAutomaticThresholds = false };
            AssetDatabase.AddObjectToAsset(blend, controller);
            var clips = AssetDatabase.LoadAllAssetsAtPath(Model).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")).ToArray();
            for (var i = 0; i < 3; i++) blend.AddChild(clips.Single(c => c.name == new[] { "Idle", "Walk", "Run" }[i]), i);
            var locomotion = machine.AddState("Locomotion"); locomotion.motion = blend;
            var rest = machine.AddState("Javell"); rest.motion = clips.Single(c => c.name == "Javell");
            machine.defaultState = locomotion;
            var enter = locomotion.AddTransition(rest);
            enter.hasExitTime = false; enter.hasFixedDuration = true; enter.duration = .28f;
            enter.interruptionSource = TransitionInterruptionSource.Destination;
            enter.AddCondition(AnimatorConditionMode.If, 0, "Resting");
            var exit = rest.AddTransition(locomotion);
            exit.hasExitTime = false; exit.hasFixedDuration = true; exit.duration = .22f;
            exit.interruptionSource = TransitionInterruptionSource.Destination;
            exit.AddCondition(AnimatorConditionMode.IfNot, 0, "Resting");
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
    }
}
