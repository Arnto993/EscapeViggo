using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Tests
{
    public sealed class ElectricianArtTests
    {
        private const string Root = "Assets/EscapeViggo/Art/Characters/Electrician/";
        [Test]
        public void HumanoidHasRequiredBonesMetreScaleForwardFeetAndGroundPivot()
        {
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "PF_Electrician.prefab"));
            try
            {
                var animator = instance.GetComponent<Animator>();
                Assert.That(animator.avatar.isHuman && animator.avatar.isValid);
                foreach (var bone in new[] { HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.Neck,
                    HumanBodyBones.Head, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
                    HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
                    HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
                    HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot })
                    Assert.That(animator.GetBoneTransform(bone), Is.Not.Null, bone.ToString());
                var renderer = instance.GetComponentInChildren<SkinnedMeshRenderer>();
                var mesh = new Mesh(); renderer.BakeMesh(mesh);
                var points = mesh.vertices.Select(renderer.transform.TransformPoint).ToArray();
                Object.DestroyImmediate(mesh);
                Assert.That(points.Max(p => p.y), Is.InRange(1.70f,1.90f));
                Assert.That(points.Min(p => p.y), Is.EqualTo(0).Within(.012f));
                Assert.That(animator.GetBoneTransform(HumanBodyBones.LeftToes).position.z,
                    Is.GreaterThan(animator.GetBoneTransform(HumanBodyBones.LeftFoot).position.z));
                Assert.That(renderer.sharedMaterials.All(m => m && m.shader.name == "Universal Render Pipeline/Lit"));
                Assert.That(renderer.bones.All(b => b));
                Assert.That(instance.GetComponentsInChildren<Collider>(), Is.Empty);
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void ThreeLoopingHumanClipsBakeRootMotionAndHaveDistinctDurations()
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(Root + "Models/Electrician.fbx");
            Assert.That(importer.clipAnimations.Length, Is.EqualTo(3));
            foreach (var clip in importer.clipAnimations)
                Assert.That(clip.loopTime && clip.lockRootRotation && clip.lockRootHeightY && clip.lockRootPositionXZ);
            var clips = AssetDatabase.LoadAllAssetsAtPath(Root + "Models/Electrician.fbx").OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__")).ToArray();
            Assert.That(clips.All(c => c.isHumanMotion && c.isLooping));
            Assert.That(clips.Select(c => c.name), Is.EquivalentTo(new[] { "Idle", "Walk", "Run" }));
        }

        [TestCase("Player")]
        [TestCase("NetworkPlayer")]
        public void PresentationIsSeparateFromExistingControllerAndCapsule(string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EscapeViggo/Prefabs/" + name + ".prefab");
            var visual = prefab.transform.Find("ElectricianVisual");
            Assert.That(visual, Is.Not.Null);
            Assert.That(prefab.GetComponent<CharacterController>().radius, Is.EqualTo(.35f));
            Assert.That(visual.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(prefab.GetComponents<CharacterController>().Length, Is.EqualTo(1));
            Assert.That(prefab.GetComponent<Presentation.ElectricianPresentation>(), Is.Not.Null);
            Assert.That(visual.GetComponent<Animator>().applyRootMotion, Is.False);
            Assert.That(prefab.transform.Find("Body"), Is.Not.Null);
            var movement = new SerializedObject(prefab.GetComponent<Player.PlayerMovement>());
            Assert.That(movement.FindProperty("_body").objectReferenceValue, Is.EqualTo(prefab.transform.Find("Body")));
        }
    }
}
