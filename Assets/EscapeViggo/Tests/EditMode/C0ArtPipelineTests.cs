using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EscapeViggo.Tests
{
    public class C0ArtPipelineTests
    {
        private const string Root = "Assets/EscapeViggo/Art/Tests/C0";

        [Test]
        public void ImportedFixtureHasMetreScaleGroundPivotAndForwardNose()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/PF_C0_MetreMesh.prefab");
            Assert.That(prefab, Is.Not.Null, "Run C0ArtPipeline.Prepare first.");
            var instance = Object.Instantiate(prefab);
            try
            {
                var renderer = instance.GetComponentInChildren<MeshRenderer>();
                Assert.That(renderer.bounds.size.x, Is.EqualTo(1).Within(0.002));
                Assert.That(renderer.bounds.size.y, Is.EqualTo(2).Within(0.002));
                Assert.That(renderer.bounds.min.y, Is.EqualTo(0).Within(0.002));
                Assert.That(renderer.bounds.max.z, Is.EqualTo(0.75f).Within(0.002));
                Assert.That(renderer.bounds.min.z, Is.EqualTo(-0.25f).Within(0.002));
                Assert.That(renderer.sharedMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Lit"));
                Assert.That(instance.GetComponentInChildren<MeshFilter>().sharedMesh.triangles.Length, Is.EqualTo(72));
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [Test]
        public void PreviewSceneReferencesExportAndExternalMaterial()
        {
            var dependencies = AssetDatabase.GetDependencies(Root + "/C0Preview.unity", true);
            Assert.That(dependencies, Does.Contain(Root + "/Models/TEST_C0_MetreMesh.fbx"));
            Assert.That(dependencies, Does.Contain(Root + "/Materials/MAT_C0_Test.mat"));
        }
    }
}
