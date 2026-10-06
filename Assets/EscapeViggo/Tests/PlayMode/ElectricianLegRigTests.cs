#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace EscapeViggo.Tests
{
    public sealed class ElectricianLegRigTests
    {
        [UnityTest]
        public IEnumerator HumanoidKeepsKneesAndFeetInTheForwardBendPlane()
            => CheckBendPlane("Electrician");

        [UnityTest]
        public IEnumerator ViggoSharedClipsKeepKneesAndFeetInTheForwardBendPlane()
            => CheckBendPlane("Viggo");

        private static IEnumerator CheckBendPlane(string character)
        {
            var obj = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(
                $"Assets/EscapeViggo/Art/Characters/{character}/PF_{character}.prefab"));
            obj.name = character;
            try
            {
                var animator = obj.GetComponent<Animator>();
                animator.enabled = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                foreach (var renderer in obj.GetComponentsInChildren<SkinnedMeshRenderer>()) renderer.updateWhenOffscreen = true;
                var knees = new[] { HumanBodyBones.LeftLowerLeg, HumanBodyBones.RightLowerLeg }
                    .Select(animator.GetBoneTransform).ToArray();
                var feet = new[] { HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot }
                    .Select(animator.GetBoneTransform).ToArray();
                var toes = new[] { HumanBodyBones.LeftToes, HumanBodyBones.RightToes }
                    .Select(animator.GetBoneTransform).ToArray();
                var normals = knees.Select(t => t.InverseTransformDirection(Vector3.forward)).ToArray();
                var transforms = obj.GetComponentsInChildren<Transform>();
                var positions = transforms.Select(t => t.localPosition).ToArray();
                var rotations = transforms.Select(t => t.localRotation).ToArray();
                yield return null;
                Render(obj, "Bind");
                animator.enabled = true;
                animator.Rebind();
                float maxSideways = 0, minForward = 1, maxFootSideways = 0;
                for (var state = 0; state < 3; state++)
                {
                    animator.SetFloat("Speed", state);
                    animator.Play("Locomotion", 0, 0);
                    animator.Update(0);
                    for (var frame = 0; frame < 61; frame++)
                    {
                        animator.Update(1f / 30);
                        for (var side = 0; side < 2; side++)
                        {
                            var front = knees[side].TransformDirection(normals[side]);
                            var foot = (toes[side].position - feet[side].position).normalized;
                            maxSideways = Mathf.Max(maxSideways, Mathf.Abs(front.x));
                            minForward = Mathf.Min(minForward, front.z);
                            maxFootSideways = Mathf.Max(maxFootSideways, Mathf.Abs(foot.x));
                            if (frame == 0) Debug.Log($"C4 LEG {state} {knees[side].name}: knee front={front:F4}, foot={foot:F4}");
                        }
                        if (frame == 7)
                        {
                            // Hold an exact quarter-cycle pose while graphics consumes
                            // it; variable editor frame time must not change the review.
                            animator.Play("Locomotion", 0, .25f);
                            animator.speed = 0;
                            animator.Update(0);
                            yield return null; // Let runtime skinning consume the evaluated Animator pose.
                            Render(obj, new[] { "Idle", "Walk", "Run" }[state]);
                            animator.speed = 1;
                        }
                    }
                }
                // Disabling Animator freezes the current pose; it does not restore bind pose.
                animator.enabled = false;
                for (var i = 0; i < transforms.Length; i++)
                { transforms[i].localPosition = positions[i]; transforms[i].localRotation = rotations[i]; }
                for (var side = 0; side < 2; side++)
                    Assert.That(Vector3.Dot(knees[side].TransformDirection(normals[side]), Vector3.forward), Is.GreaterThan(.999f));
                Debug.Log($"C4 LEG maxima: knee sideways={maxSideways}, minimum forward={minForward}, foot sideways={maxFootSideways}");
                Assert.That(maxSideways, Is.LessThan(.05f), "Humanoid introduced axial knee twist outside the sagittal bend plane.");
                // Deep run flexion approaches 90 degrees in the sagittal plane.
                Assert.That(minForward, Is.GreaterThan(0f), "Kneecaps turned away from forward.");
                Assert.That(maxFootSideways, Is.LessThan(.05f), "Feet turned sideways.");
            }
            finally { Object.Destroy(obj); }
        }

        private static void Render(GameObject obj, string pose)
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) return;
            var cameraObject = new GameObject("C4 Review Camera");
            var lightObject = new GameObject("C4 Review Light");
            var target = new RenderTexture(720, 900, 24);
            var image = new Texture2D(720, 900, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.5f;
                light.transform.rotation = Quaternion.Euler(30, 150, 0);
                var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 1.06f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.17f,.20f,.24f);
                camera.targetTexture = target;
                var directory = obj.name == "Viggo" ? "Logs/C5RigRuntime" : "Logs/C4RigRuntime";
                Directory.CreateDirectory(directory);
                foreach (var view in new[] { "Front", "ThreeQuarter" })
                {
                    camera.transform.position = obj.transform.position + (view == "Front" ? new Vector3(0,1,4) : new Vector3(2.8f,1.7f,4));
                    camera.transform.LookAt(obj.transform.position + Vector3.up * .94f);
                    camera.Render(); RenderTexture.active = target;
                    image.ReadPixels(new Rect(0,0,720,900),0,0); image.Apply();
                    File.WriteAllBytes(directory + "/" + pose + view + ".png", image.EncodeToPNG());
                }
            }
            finally
            {
                RenderTexture.active = previous;
                cameraObject.GetComponent<Camera>().targetTexture = null;
                Object.DestroyImmediate(image); Object.DestroyImmediate(target);
                Object.DestroyImmediate(cameraObject); Object.DestroyImmediate(lightObject);
            }
        }
    }
}
#endif
