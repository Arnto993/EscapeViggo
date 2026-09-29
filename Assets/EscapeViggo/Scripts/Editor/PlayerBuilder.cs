using System.IO;
using EscapeViggo.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace EscapeViggo.Editor
{
    public static class PlayerBuilder
    {
        public const string PrefabPath = "Assets/EscapeViggo/Prefabs/Player.prefab";
        public const string InputPath = "Assets/EscapeViggo/UI/PlayerControls.inputactions";
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/PlayerMovement.asset";

        [MenuItem("Escape Viggo/Setup/Create M2 Player (replaces prefab)")]
        public static void Create()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var scene = EditorSceneManager.OpenScene(WorksiteBuilder.ScenePath);
            var settings = AssetDatabase.LoadAssetAtPath<MovementSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<MovementSettings>();
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            CreateInput();
            var player = new GameObject("Player");
            var controller = player.AddComponent<CharacterController>();
            controller.height = settings.StandingHeight;
            controller.radius = 0.35f;
            controller.center = Vector3.up * (controller.height / 2);
            controller.skinWidth = 0.03f;
            controller.stepOffset = 0.25f;
            controller.minMoveDistance = 0;
            controller.slopeLimit = 50;
            var input = player.AddComponent<PlayerInputReader>();
            SetReference(input, "_actions", AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath));
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = controller.center;
            body.transform.localScale = new Vector3(controller.radius * 2, controller.height / 2, controller.radius * 2);
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().sharedMaterial = WorksiteBuilder.Material("Worker Blue", Color.cyan);
            body.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            var movement = player.AddComponent<PlayerMovement>();
            SetReference(movement, "_settings", settings);
            SetReference(movement, "_body", body.transform);
            var camera = new GameObject("Player Camera").AddComponent<Camera>();
            camera.transform.SetParent(player.transform, false);
            camera.transform.localPosition = Vector3.up * (settings.StandingHeight - settings.EyeInset);
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 150;
            camera.fieldOfView = 75;
            camera.gameObject.AddComponent<AudioListener>();
            var look = player.AddComponent<PlayerLook>();
            SetReference(look, "_cameraPivot", camera.transform);
            var prefab = PrefabUtility.SaveAsPrefabAsset(player, PrefabPath);
            Object.DestroyImmediate(player);

            var previous = GameObject.Find("Local Player");
            if (previous != null) Object.DestroyImmediate(previous);
            var overview = GameObject.Find("Worksite Overview Camera");
            if (overview != null) Object.DestroyImmediate(overview);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = "Local Player";
            instance.transform.position = GameObject.Find("Electrician Spawn").transform.position;
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Validate();
        }

        private static void CreateInput()
        {
            if (File.Exists(InputPath)) return;
            var asset = ScriptableObject.CreateInstance<InputActionAsset>();
            asset.name = "PlayerControls";
            var map = asset.AddActionMap("Player");
            map.AddAction("Move", InputActionType.Value, expectedControlLayout: "Vector2")
                .AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            map.AddAction("Look", InputActionType.Value, "<Mouse>/delta", expectedControlLayout: "Vector2");
            map.AddAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
            map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            var crouch = map.AddAction("Crouch", InputActionType.Button, "<Keyboard>/leftCtrl");
            crouch.AddBinding("<Keyboard>/c");
            map.AddAction("ReleaseCursor", InputActionType.Button, "<Keyboard>/escape");
            File.WriteAllText(InputPath, asset.ToJson());
            Object.DestroyImmediate(asset);
            AssetDatabase.ImportAsset(InputPath);
        }

        public static void SetReference(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(field).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [MenuItem("Escape Viggo/Validation/Validate M2 Player")]
        public static void Validate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            WorksiteValidation.Require(prefab != null, "Missing player prefab.");
            WorksiteValidation.Require(prefab.GetComponent<CharacterController>() != null, "Missing character collider.");
            WorksiteValidation.Require(prefab.GetComponent<PlayerMovement>().Settings != null, "Missing tuning.");
            WorksiteValidation.Require(prefab.GetComponentInChildren<Camera>(true) != null, "Missing camera.");
            foreach (var name in new[] { "Move", "Look", "Sprint", "Jump", "Crouch", "ReleaseCursor" })
                WorksiteValidation.Require(AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputPath).FindAction(name) != null,
                    "Missing action " + name);
            Debug.Log("M2 ASSET VALIDATION PASSED.");
        }
    }
}
