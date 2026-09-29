using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EscapeViggo.Editor
{
    public static class WorksiteValidation
    {
        [MenuItem("Escape Viggo/Validation/Validate M1 Worksite")]
        public static void Validate()
        {
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
                Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                    "Missing script on " + child.name);

            foreach (var name in new[] { "Main Work Area", "Corridor", "Electrical Room", "Storage Room", "Small Office",
                         "Electrician Spawn", "Viggo Spawn", "Battery Station", "Task Location 1", "Task Location 2",
                         "Task Location 3", "Task Location 4", "Task Location 5" })
                Require(GameObject.Find(name) != null, "Missing worksite location: " + name);

            Physics.SyncTransforms();
            var floor = GameObject.Find("Continuous Foundation").GetComponent<Collider>();
            for (var x = -19f; x <= 19; x += 0.5f)
            for (var z = -15f; z <= 15; z += 0.5f)
                Require(floor.Raycast(new Ray(new Vector3(x, 0.2f, z), Vector3.down), out _, 0.5f),
                    $"Floor hole at {x}, {z}");

            // Sweeps a 1.8 m tall, 0.7 m wide capsule around both loops and into each room.
            var loop = new[] { new Vector2(-15, -12), new Vector2(-15, -4), new Vector2(-15, 4),
                new Vector2(-18.5f, 4), new Vector2(-18.5f, 13), new Vector2(0, 13), new Vector2(0, 10),
                new Vector2(10, 10), new Vector2(13, 10), new Vector2(13, 0), new Vector2(13, -10),
                new Vector2(0, -10), new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 0), new Vector2(5, -10),
                new Vector2(0, -10), new Vector2(0, -14.5f), new Vector2(-15, -14.5f), new Vector2(-15, -12) };
            for (var i = 1; i < loop.Length; i++)
            {
                var start = new Vector3(loop[i - 1].x, 0.4f, loop[i - 1].y);
                var end = new Vector3(loop[i].x, 0.4f, loop[i].y);
                var direction = end - start;
                Require(!Physics.CapsuleCast(start, start + Vector3.up * 1.1f, 0.35f,
                    direction.normalized, out var hit, direction.magnitude, ~0, QueryTriggerInteraction.Ignore),
                    $"Capsule route {i} obstructed by {(hit.collider == null ? "unknown" : hit.collider.name)}");
            }
            Require(Physics.Linecast(new Vector3(-15, 1.6f, 0), new Vector3(0, 1.6f, 0)), "Central cover does not break sight.");
            Debug.Log("M1 VALIDATION PASSED: saved scene, references, 4697 floor samples, capsule routes, locations and cover.");
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
