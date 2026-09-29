#if UNITY_EDITOR
using EscapeViggo.Interaction;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Tests
{
    public sealed class InteractionQueryTests
    {
        private GameObject _target, _wall;
        private readonly Vector3 _origin = new(100, 1, 100);

        [SetUp]
        public void SetUp()
        {
            _target = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EscapeViggo/Prefabs/TestInteraction.prefab"));
            _target.transform.position = _origin + Vector3.forward * 2;
            Physics.SyncTransforms();
        }
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_target);
            if (_wall != null) Object.DestroyImmediate(_wall);
        }
        [Test]
        public void SelectsOnlyAimedObjectWithinRange()
        {
            Assert.That(InteractionQuery.Raycast(_origin, Vector3.forward, 3), Is.EqualTo(_target.GetComponent<NetworkInteractable>()));
            Assert.That(InteractionQuery.Raycast(_origin, Vector3.right, 3), Is.Null);
            Assert.That(InteractionQuery.Raycast(_origin, Vector3.forward, 1), Is.Null);
        }
        [Test]
        public void WallOccludesInteractable()
        {
            _wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _wall.transform.position = _origin + Vector3.forward;
            Physics.SyncTransforms();
            Assert.That(InteractionQuery.Raycast(_origin, Vector3.forward, 3), Is.Null);
        }
        [Test]
        public void InvalidDirectionCannotSelectAnObject()
        {
            Assert.That(InteractionQuery.Raycast(_origin, Vector3.zero, 3), Is.Null);
            Assert.That(InteractionQuery.Raycast(_origin, new Vector3(float.NaN, 0, 1), 3), Is.Null);
        }
    }
}
#endif
