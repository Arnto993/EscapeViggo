using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace EscapeViggo.Editor
{
    // Deliberately scoped to one test fixture. Production import rules are not changed.
    public static class C0ArtPipeline
    {
        public const string Root = "Assets/EscapeViggo/Art/Tests/C0";
        public const string ModelPath = Root + "/Models/TEST_C0_MetreMesh.fbx";
        public const string MaterialPath = Root + "/Materials/MAT_C0_Test.mat";
        public const string PrefabPath = Root + "/PF_C0_MetreMesh.prefab";
        public const string ScenePath = Root + "/C0Preview.unity";

        [MenuItem("Escape Viggo/Art/C0/Import and Validate")]
        public static void Prepare()
        {
            Require(File.Exists(ModelPath), "Run Tools/Blender/validate_c0.py first.");
            Directory.CreateDirectory(Root + "/Materials");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (!material)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                Require(shader, "URP Lit shader missing.");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            material.SetColor("_BaseColor", new Color(0.12f, 0.55f, 0.8f, 1));
            material.SetFloat("_Smoothness", 0.2f);
            EditorUtility.SetDirty(material);
            var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
            importer.globalScale = 1;
            importer.useFileScale = true;
            importer.bakeAxisConversion = true;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = true; // C0 geometry assertions only; not a production default.
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "MAT_C0_Test"), material);
            importer.SaveAndReimport();
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath));
            try
            {
                instance.name = "PF_C0_MetreMesh";
                PrefabUtility.SaveAsPrefabAsset(instance, PrefabPath);
            }
            finally { Object.DestroyImmediate(instance); }
            AssetDatabase.SaveAssets();
            Validate();
            Debug.Log("C0 import, URP material remap, prefab and geometry validation passed.");
        }

        public static void Validate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Require(prefab, "C0 prefab missing.");
            var instance = Object.Instantiate(prefab);
            try
            {
                var filters = instance.GetComponentsInChildren<MeshFilter>();
                Require(filters.Length == 1, "Expected one mesh.");
                var mesh = filters[0].sharedMesh;
                Require(mesh && mesh.vertexCount >= 16 && mesh.triangles.Length == 72, "Invalid C0 mesh/topology.");
                var points = mesh.vertices.Select(filters[0].transform.TransformPoint).ToArray();
                var bounds = new Bounds(points[0], Vector3.zero);
                foreach (var point in points) bounds.Encapsulate(point);
                Near(bounds.size, new Vector3(1, 2, 1), "Metre scale / axis dimensions");
                Near(bounds.center, new Vector3(0, 1, 0.25f), "Ground pivot / forward axis");
                Require(points.Any(p => p.z > 0.74f && p.y > 1.74f), "Raised nose must face Unity +Z.");
                foreach (var transform in instance.GetComponentsInChildren<Transform>())
                {
                    Near(transform.localScale, Vector3.one, "Applied scale");
                    Require(Quaternion.Angle(transform.localRotation, Quaternion.identity) < 0.01f, "Unexpected import rotation.");
                    Near(transform.localPosition, Vector3.zero, "Ground-centred origin");
                }
                var renderer = filters[0].GetComponent<MeshRenderer>();
                var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                Require(renderer && renderer.sharedMaterials.Length == 1 && renderer.sharedMaterial == material,
                    "Material remap/reference missing.");
                Require(material && material.shader && material.shader.name == "Universal Render Pipeline/Lit", "Expected URP material.");
                Require(instance.GetComponentsInChildren<Component>().All(c => c), "Missing script reference.");
                var dependencies = AssetDatabase.GetDependencies(PrefabPath, true);
                Require(dependencies.Contains(ModelPath) && dependencies.Contains(MaterialPath), "Prefab dependencies missing.");
            }
            finally { Object.DestroyImmediate(instance); }
        }

        [MenuItem("Escape Viggo/Art/C0/Build Isolated Preview")]
        public static void BuildPreview()
        {
            Prepare();
            // Batch starts with an unsaved empty scene, which Unity cannot extend additively.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,
                Application.isBatchMode ? NewSceneMode.Single : NewSceneMode.Additive);
            try
            {
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath), scene);
                var cameraObject = new GameObject("C0 Camera", typeof(Camera));
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
                cameraObject.transform.position = new Vector3(3, 2.5f, 4);
                cameraObject.transform.LookAt(new Vector3(0, 1, 0));
                cameraObject.GetComponent<Camera>().backgroundColor = new Color(0.12f, 0.12f, 0.12f);
                var lightObject = new GameObject("C0 Light", typeof(Light));
                SceneManager.MoveGameObjectToScene(lightObject, scene);
                lightObject.GetComponent<Light>().type = LightType.Directional;
                lightObject.transform.rotation = Quaternion.Euler(40, -30, 0);
                Require(EditorSceneManager.SaveScene(scene, ScenePath), "Could not save C0 preview scene.");
                var dependencies = AssetDatabase.GetDependencies(ScenePath, true);
                Require(dependencies.Contains(ModelPath) && dependencies.Contains(MaterialPath), "Scene art references missing.");
                Directory.CreateDirectory("Builds/C0Preview");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = "Builds/C0Preview/C0Preview.exe",
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development | BuildOptions.DetailedBuildReport
                });
                Require(report.summary.result == BuildResult.Succeeded, "C0 preview build failed.");
                Require(report.packedAssets.SelectMany(a => a.contents).Any(a => a.sourceAssetPath == ModelPath),
                    "Build report does not contain C0 FBX.");
                Debug.Log("C0 Windows preview build passed; imported FBX present in packed build assets.");
            }
            finally
            {
                if (SceneManager.sceneCount > 1) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void Near(Vector3 actual, Vector3 expected, string label)
        {
            Require(Vector3.Distance(actual, expected) < 0.002f, $"{label}: expected {expected}, got {actual}");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
