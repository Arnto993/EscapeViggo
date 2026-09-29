using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeViggo.Editor
{
    /// <summary>Explicit, repeatable greybox creation. Never runs automatically on import.</summary>
    public static class WorksiteBuilder
    {
        public const string ScenePath = "Assets/EscapeViggo/Scenes/PrototypeWorksite.unity";
        private const string MaterialPath = "Assets/EscapeViggo/Materials/";

        [MenuItem("Escape Viggo/Setup/Create M1 Worksite (replaces scene)")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var geometry = new GameObject("Worksite Geometry").transform;
            var markers = new GameObject("Development Markers").transform;
            var floor = Material("Concrete", new Color(0.32f, 0.36f, 0.39f));
            var wall = Material("Walls", new Color(0.62f, 0.66f, 0.68f));
            var cover = Material("Equipment", new Color(0.25f, 0.32f, 0.36f));
            var yellow = Material("Task Yellow", new Color(1f, 0.7f, 0.12f));
            var blue = Material("Worker Blue", new Color(0.15f, 0.65f, 0.95f));
            var orange = Material("Viggo Orange", new Color(0.95f, 0.3f, 0.1f));
            var green = Material("Office Green", new Color(0.25f, 0.55f, 0.4f));

            Box("Continuous Foundation", new Vector3(0, -0.25f, 0), new Vector3(40, 0.5f, 32), floor, geometry);
            Box("North Boundary", new Vector3(0, 1.5f, 16), new Vector3(40, 3, 0.4f), wall, geometry);
            Box("South Boundary", new Vector3(0, 1.5f, -16), new Vector3(40, 3, 0.4f), wall, geometry);
            Box("West Boundary", new Vector3(-20, 1.5f, 0), new Vector3(0.4f, 3, 32), wall, geometry);
            Box("East Boundary", new Vector3(20, 1.5f, 0), new Vector3(0.4f, 3, 32), wall, geometry);

            // Three broad doorways connect the main hall, corridor, and eastern rooms.
            foreach (var x in new[] { 3f, 7f })
            {
                foreach (var segment in new[] { new Vector2(-16, -11.5f), new Vector2(-8.5f, -1.5f),
                             new Vector2(1.5f, 8.5f), new Vector2(11.5f, 16) })
                    Box("Corridor Wall", new Vector3(x, 1.5f, (segment.x + segment.y) / 2),
                        new Vector3(0.35f, 3, segment.y - segment.x), wall, geometry);
            }
            // Interconnecting rooms provide a second loop rather than three dead ends.
            foreach (var z in new[] { -6f, 5f })
            {
                Box("Room Partition West", new Vector3(9.25f, 1.5f, z), new Vector3(4.5f, 3, 0.35f), wall, geometry);
                Box("Room Partition East", new Vector3(17.25f, 1.5f, z), new Vector3(5.5f, 3, 0.35f), wall, geometry);
            }

            Box("Central Machinery", new Vector3(-9, 1.2f, 0), new Vector3(5, 2.4f, 4), cover, geometry);
            Box("North Screen", new Vector3(-4, 1.3f, 7), new Vector3(2, 2.6f, 4), cover, geometry);
            Box("North Pallets", new Vector3(-13, 1, 9), new Vector3(3, 2, 3), cover, geometry);
            Box("South Screen", new Vector3(-11, 1.2f, -7), new Vector3(4, 2.4f, 2), cover, geometry);
            Box("Storage Shelving", new Vector3(17.5f, 1.25f, 1), new Vector3(2, 2.5f, 4), cover, geometry);
            Box("Office Desk", new Vector3(17, 0.5f, -9), new Vector3(3, 1, 1.5f), green, geometry);
            // A safe, optional crouch route with 1.3 m of headroom.
            Box("Low Service Duct", new Vector3(-5, 1.5f, -6), new Vector3(3, 0.4f, 2), cover, geometry);
            Box("Duct Support Left", new Vector3(-6.65f, 0.75f, -6), new Vector3(0.3f, 1.5f, 2), cover, geometry);
            Box("Duct Support Right", new Vector3(-3.35f, 0.75f, -6), new Vector3(0.3f, 1.5f, 2), cover, geometry);

            Area("Main Work Area", new Vector3(-8, 0, 13), floor, markers);
            Area("Corridor", new Vector3(5, 0, 3), blue, markers);
            Area("Electrical Room", new Vector3(12, 0, 13), yellow, markers);
            Area("Storage Room", new Vector3(11, 0, 2), cover, markers);
            Area("Small Office", new Vector3(12, 0, -13), green, markers);
            Area("Electrician Spawn Area", new Vector3(-15, 0, -12), blue, markers);
            Area("Viggo Spawn", new Vector3(16, 0, 11), orange, markers);
            var spawn = new GameObject("Electrician Spawn").transform;
            spawn.SetParent(markers);
            spawn.position = new Vector3(-15, 0.1f, -12);

            var battery = Box("Battery Station", new Vector3(17, 0.5f, 7.5f), new Vector3(2, 1, 1), orange, markers);
            Label("BATTERY STATION\nplaceholder only", battery.transform, new Vector3(0, 1.4f, 0));
            var taskPositions = new[] { new Vector3(-17, 0, 6), new Vector3(-5, 0, -13),
                new Vector3(18, 0, 13), new Vector3(18, 0, -3), new Vector3(18, 0, -13) };
            for (var i = 0; i < taskPositions.Length; i++)
            {
                var task = Box($"Task Location {i + 1}", taskPositions[i] + Vector3.up * 0.7f,
                    new Vector3(0.8f, 1.4f, 0.4f), yellow, markers);
                Label($"TASK {i + 1}\nfuture work", task.transform, new Vector3(0, 1.3f, 0));
            }

            var light = new GameObject("Worksite Sun").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.5f;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.65f, 0.68f, 0.72f);
            var camera = new GameObject("Worksite Overview Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0, 34, -27);
            camera.transform.LookAt(Vector3.zero);
            camera.gameObject.AddComponent<AudioListener>();
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 150;

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save PrototypeWorksite.");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            WorksiteValidation.Validate();
        }

        public static Material Material(string name, Color color)
        {
            var path = MaterialPath + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is missing.");
            material = new Material(shader) { name = name };
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static GameObject Box(string name, Vector3 position, Vector3 size, Material material, Transform parent)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.position = position;
            box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box;
        }

        private static void Area(string name, Vector3 position, Material material, Transform parent)
        {
            var area = new GameObject(name).transform;
            area.SetParent(parent);
            area.position = position;
            var pad = Box(name + " Floor Marker", position + Vector3.up * 0.015f,
                new Vector3(2.5f, 0.02f, 2), material, area);
            UnityEngine.Object.DestroyImmediate(pad.GetComponent<Collider>());
            Label(name.ToUpperInvariant(), area, Vector3.up * 2.8f);
        }

        private static void Label(string text, Transform parent, Vector3 position)
        {
            var label = new GameObject("Label").AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            // Keep world text independent of the dimensions of the primitive parent.
            label.transform.position = parent.position + position;
            label.transform.localScale = new Vector3(1 / parent.lossyScale.x, 1 / parent.lossyScale.y, 1 / parent.lossyScale.z);
            label.text = text;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.fontSize = 48;
            label.characterSize = 0.075f;
            label.color = Color.white;
        }
    }
}
