using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using EscapeViggo.Presentation;

namespace EscapeViggo.Tests
{
    public sealed class ViggoArtTests
    {
        [Test]
        public void ViggoUsesDedicatedClipsAndHasOnlyOneUnmappedSecondaryBone()
        {
            var root = "Assets/EscapeViggo/Art/Characters/";
            var importer = (ModelImporter)AssetImporter.GetAtPath(root + "Viggo/Models/Viggo.fbx");
            Assert.That(importer.importAnimation, Is.True);
            Assert.That(importer.humanDescription.human.Any(b => b.boneName == "Belly"), Is.False);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(root + "Viggo/PF_Viggo.prefab");
            var transforms = model.GetComponentsInChildren<Transform>();
            Assert.That(transforms.Single(t => t.name == "Belly").parent.name, Is.EqualTo("Spine"));
            var worker = (ModelImporter)AssetImporter.GetAtPath(root + "Electrician/Models/Electrician.fbx");
            Assert.That(importer.humanDescription.human.Select(b => b.boneName),
                Is.EquivalentTo(worker.humanDescription.human.Select(b => b.boneName)));
            Assert.That(model.GetComponent<Animator>().runtimeAnimatorController,
                Is.SameAs(AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(root + "Viggo/Viggo.controller")));
            var clips = AssetDatabase.LoadAllAssetsAtPath(root + "Viggo/Models/Viggo.fbx")
                .OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")).ToArray();
            Assert.That(clips.Select(c => c.name), Is.EquivalentTo(new[] { "Idle", "Walk", "Run", "Javell" }));
            Assert.That(clips.All(c => c.isHumanMotion && c.isLooping), Is.True);
            Assert.That(importer.clipAnimations.All(c => c.lockRootRotation && c.lockRootHeightY && c.lockRootPositionXZ), Is.True);
        }

        [TestCase("Player")]
        [TestCase("NetworkPlayer")]
        public void BothRoleVisualsKeepExistingGameplayReferences(string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EscapeViggo/Prefabs/" + name + ".prefab");
            var presentation = prefab.GetComponent<ElectricianPresentation>();
            Assert.That(presentation.Animator, Is.EqualTo(prefab.transform.Find("ElectricianVisual").GetComponent<Animator>()));
            Assert.That(presentation.ViggoAnimator, Is.EqualTo(prefab.transform.Find("ViggoVisual").GetComponent<Animator>()));
            Assert.That(presentation.ViggoAnimator.applyRootMotion, Is.False);
            Assert.That(presentation.ViggoAnimator.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(prefab.GetComponents<CharacterController>().Length, Is.EqualTo(1));
            Assert.That(prefab.GetComponent<CharacterController>().radius, Is.EqualTo(.35f));
            var movement = new SerializedObject(prefab.GetComponent<Player.PlayerMovement>());
            Assert.That(movement.FindProperty("_body").objectReferenceValue, Is.EqualTo(prefab.transform.Find("Body")));
            Assert.That(prefab.GetComponentsInChildren<Component>(true).All(c => c), Is.True);
        }
    }
}
