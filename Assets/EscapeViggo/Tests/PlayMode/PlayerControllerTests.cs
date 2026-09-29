#if UNITY_EDITOR
using System.Collections;
using EscapeViggo.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EscapeViggo.Tests
{
    public sealed class PlayerControllerTests
    {
        private GameObject _player;
        private PlayerMovement _movement;
        private PlayerLook _look;
        private CharacterController _controller;
        private const float Step = 1f / 60;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            foreach (var session in Object.FindObjectsByType<EscapeViggo.Networking.LocalNetworkSession>(FindObjectsSortMode.None))
                Object.Destroy(session.gameObject);
            yield return null;
            yield return SceneManager.LoadSceneAsync("PrototypeWorksite");
            var previous = GameObject.Find("Local Player");
            if (previous != null) Object.Destroy(previous);
            yield return null;
            _player = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EscapeViggo/Prefabs/Player.prefab"));
            _movement = _player.GetComponent<PlayerMovement>();
            _look = _player.GetComponent<PlayerLook>();
            _controller = _player.GetComponent<CharacterController>();
            _movement.enabled = false;
            _look.enabled = false;
            _player.GetComponent<PlayerInputReader>().enabled = false;
            Place(new Vector3(-17, 0.05f, -12));
            Simulate(default, 30);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_player);

        [Test]
        public void WalkSprintAndDiagonalHaveExpectedSpeeds()
        {
            var start = _player.transform.position;
            Simulate(new MovementCommand { Move = Vector2.up }, 60);
            Assert.That(_player.transform.position.z - start.z, Is.EqualTo(_movement.Settings.WalkSpeed).Within(0.1f));
            Place(start);
            Simulate(new MovementCommand { Move = Vector2.up, Sprint = true }, 60);
            Assert.That(_player.transform.position.z - start.z, Is.EqualTo(_movement.Settings.SprintSpeed).Within(0.1f));
            Place(start);
            Simulate(new MovementCommand { Move = Vector2.one }, 30);
            var distance = Vector3.Distance(start, _player.transform.position);
            Assert.That(distance, Is.EqualTo(_movement.Settings.WalkSpeed / 2).Within(0.1f));
        }

        [Test]
        public void JumpLeavesGroundAndLandsWithoutAirJump()
        {
            Assert.That(_movement.IsGrounded, Is.True);
            var floorY = _player.transform.position.y;
            Simulate(new MovementCommand { Jump = true }, 1);
            Assert.That(_movement.IsGrounded, Is.False);
            var peak = floorY;
            for (var i = 0; i < 100; i++)
            {
                Simulate(new MovementCommand { Jump = i == 10 }, 1);
                peak = Mathf.Max(peak, _player.transform.position.y);
            }
            Assert.That(peak - floorY, Is.InRange(0.9f, 1.2f));
            Assert.That(_movement.IsGrounded, Is.True);
            Assert.That(_player.transform.position.y, Is.EqualTo(floorY).Within(0.05f));
        }

        [Test]
        public void WallsStopMovement()
        {
            Place(new Vector3(-18, 0.05f, -12));
            Simulate(new MovementCommand { Move = Vector2.left, Sprint = true }, 120);
            Assert.That(_player.transform.position.x, Is.GreaterThan(-19.6f));
        }

        [Test]
        public void CrouchPassesDuctAndCannotStandInsideIt()
        {
            Place(new Vector3(-5, 0.05f, -8));
            Simulate(new MovementCommand { Move = Vector2.up, Crouch = true, Sprint = true }, 50);
            Assert.That(_movement.IsCrouching, Is.True);
            Assert.That(_player.transform.position.z, Is.EqualTo(-8 + _movement.Settings.CrouchSpeed * Step * 50).Within(0.1f));
            Simulate(default, 10);
            Assert.That(_movement.IsCrouching, Is.True, "Must not stand through ceiling.");
            Simulate(new MovementCommand { Move = Vector2.up, Crouch = true }, 60);
            Simulate(default, 1);
            Assert.That(_movement.IsCrouching, Is.False);
            Assert.That(_movement.Height, Is.EqualTo(_movement.Settings.StandingHeight));
        }

        [Test]
        public void LookRotatesYawClampsPitchAndUsesSensitivity()
        {
            _look.ApplyLook(new Vector2(100, 0));
            Assert.That(_player.transform.eulerAngles.y, Is.EqualTo(100 * _movement.Settings.MouseSensitivity).Within(0.01f));
            _look.ApplyLook(new Vector2(0, 10000));
            var angle = Mathf.DeltaAngle(0, _player.GetComponentInChildren<Camera>().transform.localEulerAngles.x);
            Assert.That(angle, Is.EqualTo(-_movement.Settings.PitchLimit).Within(0.01f));
        }

        [Test]
        public void ControllerTraversesRoomsAndReturnsToMainArea()
        {
            Place(new Vector3(0, 0.05f, -10));
            foreach (var destination in new[] { new Vector2(13, -10), new Vector2(13, 10),
                         new Vector2(0, 10), new Vector2(0, -10) })
            {
                for (var i = 0; i < 600; i++)
                {
                    var offset = destination - new Vector2(_player.transform.position.x, _player.transform.position.z);
                    if (offset.magnitude < 0.1f) break;
                    Simulate(new MovementCommand { Move = offset.normalized }, 1);
                }
                Assert.That(Vector2.Distance(destination, new Vector2(_player.transform.position.x, _player.transform.position.z)),
                    Is.LessThan(0.15f), "Could not reach " + destination);
            }
        }

        [Test]
        public void InputAssetMapsMovementAndBothCrouchBindings()
        {
            var background = InputSystem.settings.backgroundBehavior;
            var editorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var actions = Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/EscapeViggo/UI/PlayerControls.inputactions"));
            try
            {
                actions.Enable();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift, Key.Space, Key.LeftCtrl));
                InputSystem.Update();
                Assert.That(actions.FindAction("Move").ReadValue<Vector2>(), Is.EqualTo(Vector2.up));
                Assert.That(actions.FindAction("Sprint").IsPressed(), Is.True);
                Assert.That(actions.FindAction("Jump").WasPressedThisFrame(), Is.True);
                Assert.That(actions.FindAction("Crouch").IsPressed(), Is.True);
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.C, Key.E));
                InputSystem.Update();
                Assert.That(actions.FindAction("Crouch").IsPressed(), Is.True);
                Assert.That(actions.FindAction("Interact").WasPressedThisFrame(), Is.True);
            }
            finally
            {
                actions.Disable();
                Object.DestroyImmediate(actions);
                InputSystem.RemoveDevice(keyboard);
                InputSystem.settings.backgroundBehavior = background;
                InputSystem.settings.editorInputBehaviorInPlayMode = editorInput;
            }
        }

        private void Place(Vector3 position)
        {
            _controller.enabled = false;
            _player.transform.SetPositionAndRotation(position, Quaternion.identity);
            _controller.enabled = true;
            Physics.SyncTransforms();
        }

        private void Simulate(MovementCommand command, int frames)
        {
            for (var i = 0; i < frames; i++) _movement.Simulate(command, Step);
        }
    }
}
#endif
