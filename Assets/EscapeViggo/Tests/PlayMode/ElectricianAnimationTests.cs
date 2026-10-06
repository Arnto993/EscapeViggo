#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeViggo.Tests
{
    public sealed class ElectricianAnimationTests
    {
        [UnityTest]
        public IEnumerator SharedWalkRetargetsToAProportionChangedTestSkeleton()
        {
            const string model = "Assets/EscapeViggo/Art/Characters/Electrician/Models/Electrician.fbx";
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(model));
            Avatar testAvatar = null;
            try
            {
                var animator = instance.GetComponent<Animator>();
                var description = ((ModelImporter)AssetImporter.GetAtPath(model)).humanDescription;
                var skeleton = description.skeleton;
                for (var i = 0; i < skeleton.Length; i++)
                    if (skeleton[i].name.EndsWith("LowerLeg")) skeleton[i].position *= 1.1f;
                description.skeleton = skeleton;
                testAvatar = AvatarBuilder.BuildHumanAvatar(instance, description);
                Assert.That(testAvatar.isValid && testAvatar.isHuman);
                animator.avatar = testAvatar;
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                    "Assets/EscapeViggo/Art/Characters/Electrician/Electrician.controller");
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.Rebind();
                yield return null;
                animator.SetFloat("Speed", 1);
                animator.Update(.1f);
                var leg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                var first = leg.rotation;
                animator.Update(.25f);
                Assert.That(Quaternion.Angle(first, leg.rotation), Is.GreaterThan(1));
                Assert.That(instance.transform.position.sqrMagnitude, Is.LessThan(.0001f));
            }
            finally
            {
                Object.Destroy(instance);
                if (testAvatar) Object.Destroy(testAvatar);
            }
        }

        [UnityTest]
        public IEnumerator HumanoidRetargetsThreeClipsWithoutMovingThePresentationRoot()
            => CheckPlayback("Electrician");

        [UnityTest]
        public IEnumerator ViggoPlaysDedicatedClipsWithoutMovingThePresentationRoot()
            => CheckPlayback("Viggo");

        [UnityTest]
        public IEnumerator ViggoRestPlacesBothHandsOnBellyAndReturnsToLocomotion()
        {
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/EscapeViggo/Art/Characters/Viggo/PF_Viggo.prefab"));
            try
            {
                var animator = instance.GetComponent<Animator>();
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                yield return null;
                var root = instance.transform.position;
                // Rest must override even a moving blend tree; transitions also handle interruption.
                animator.SetFloat("Speed", 2);
                animator.SetBool("Resting", true);
                for (var i = 0; i < 20; i++) animator.Update(1f / 30);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Javell"), Is.True);
                var chest = animator.GetBoneTransform(HumanBodyBones.Chest);
                var initialChest = chest.rotation;
                for (var frame = 0; frame < 96; frame++)
                {
                    animator.Update(1f / 30);
                    foreach (var bone in new[] { HumanBodyBones.LeftHand, HumanBodyBones.RightHand })
                    {
                        var hand = instance.transform.InverseTransformPoint(animator.GetBoneTransform(bone).position);
                        Assert.That(hand.y, Is.InRange(.90f, 1.15f), "Rest hand height must be belly level.");
                        Assert.That(hand.z, Is.InRange(.27f, .48f), "Rest hands must reach the front of the belly.");
                        Assert.That(Mathf.Abs(hand.x), Is.InRange(.12f, .34f));
                    }
                    if (frame == 15) Assert.That(Quaternion.Angle(initialChest, chest.rotation), Is.GreaterThan(.1f), "Rest must breathe.");
                }
                animator.speed = 0;
                yield return null;
                ElectricianLegRigTests.Render(instance, "Javell");
                animator.speed = 1;
                animator.SetBool("Resting", false);
                for (var i = 0; i < 15; i++) animator.Update(1f / 30);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
                animator.SetBool("Resting", true); animator.Update(.05f);
                animator.SetBool("Resting", false);
                for (var i = 0; i < 20; i++) animator.Update(1f / 30);
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion"), Is.True);
                Assert.That(Vector3.Distance(root, instance.transform.position), Is.LessThan(.0001f));
            }
            finally { Object.Destroy(instance); }
        }

        private static IEnumerator CheckPlayback(string character)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/EscapeViggo/Art/Characters/{character}/PF_{character}.prefab");
            var instance = Object.Instantiate(prefab);
            try
            {
                var animator = instance.GetComponent<Animator>();
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                yield return null;
                var root = instance.transform.position;
                var leg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                foreach (var speed in new[] { 0f, 1f, 2f })
                {
                    animator.SetFloat("Speed", speed); animator.Update(.15f);
                    if (speed == 0)
                    {
                        foreach (var left in new[] { true, false })
                        {
                            var upper = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm);
                            var elbow = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm);
                            var hand = animator.GetBoneTransform(left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand);
                            Assert.That(Vector3.Dot((elbow.position - upper.position).normalized, Vector3.down), Is.GreaterThan(.8f),
                                character + " idle upper arm must share the relaxed C4 reference pose.");
                            Assert.That(hand.position.y, Is.LessThan(elbow.position.y), character + " idle forearm points upward.");
                        }
                    }
                    var first = speed == 0 ? animator.GetBoneTransform(HumanBodyBones.Chest).rotation : leg.rotation;
                    animator.Update(.20f);
                    var second = speed == 0 ? animator.GetBoneTransform(HumanBodyBones.Chest).rotation : leg.rotation;
                    Assert.That(Quaternion.Angle(first, second), Is.GreaterThan(.01f), "No pose playback at speed " + speed);
                    for (var i = 0; i < 100; i++) animator.Update(.033f);
                    Assert.That(Vector3.Distance(root, instance.transform.position), Is.LessThan(.0001f));
                }
                Assert.That(instance.GetComponentsInChildren<SkinnedMeshRenderer>().All(r => r.bounds.size.y > 1));
            }
            finally { Object.Destroy(instance); }
        }

        [UnityTest]
        public IEnumerator OfflineOwnerKeepsCameraAndOnlyCharacterShadows()
        {
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EscapeViggo/Prefabs/Player.prefab"));
            try
            {
                instance.GetComponent<Player.PlayerMovement>().enabled = false;
                instance.GetComponent<Player.PlayerLook>().enabled = false;
                yield return null;
                var visual = instance.transform.Find("ElectricianVisual");
                Assert.That(visual.gameObject.activeSelf);
                Assert.That(visual.GetComponentsInChildren<Renderer>().All(r => r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly));
                Assert.That(instance.GetComponentInChildren<Camera>().enabled);
                Assert.That(instance.transform.Find("Body").GetComponent<Renderer>().enabled, Is.False);
            }
            finally { Object.Destroy(instance); }
        }
    }
}
#endif
