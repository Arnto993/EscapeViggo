using UnityEngine;

namespace EscapeViggo.Player
{
    [DefaultExecutionOrder(-10)]
    [RequireComponent(typeof(PlayerInputReader), typeof(PlayerMovement))]
    public sealed class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Transform _cameraPivot;
        private PlayerInputReader _input;
        private PlayerMovement _movement;
        private float _pitch;
        public float SensitivityMultiplier { get; set; } = 1;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            _movement = GetComponent<PlayerMovement>();
        }

        private void Update() => ApplyLook(_input.Look);

        public void ApplyLook(Vector2 mouseDelta)
        {
            var settings = _movement.Settings;
            var sensitivity = settings.MouseSensitivity * SensitivityMultiplier;
            // Mouse delta already measures displacement per input update; do not scale by deltaTime.
            transform.Rotate(0, mouseDelta.x * sensitivity, 0);
            _pitch = Mathf.Clamp(_pitch - mouseDelta.y * sensitivity, -settings.PitchLimit, settings.PitchLimit);
            _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }

        private void LateUpdate()
        {
            _cameraPivot.localPosition = Vector3.up * (_movement.Height - _movement.Settings.EyeInset);
        }
    }
}
