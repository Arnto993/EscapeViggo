using UnityEngine;
using UnityEngine.InputSystem;

namespace EscapeViggo.Player
{
    [DefaultExecutionOrder(-30)]
    public sealed class PlayerInputReader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset _actions;
        private InputActionAsset _instance;
        private InputAction _move, _look, _sprint, _jump, _crouch, _release;
        private InputAction _interact;
        public bool InteractPressed => enabled && IsCaptured && _interact != null && _interact.WasPressedThisFrame();
        public bool IsCaptured { get; private set; }
        public bool IsTaskModal { get; private set; }
        public Vector2 Look => enabled && IsCaptured ? _look.ReadValue<Vector2>() : Vector2.zero;

        private void OnEnable()
        {
            if (_actions == null)
            {
                Debug.LogError("PlayerInputReader requires a movement Input Actions asset.", this);
                enabled = false;
                return;
            }
            if (_instance == null)
            {
                _instance = Instantiate(_actions);
                _move = _instance.FindAction("Player/Move", true);
                _look = _instance.FindAction("Player/Look", true);
                _sprint = _instance.FindAction("Player/Sprint", true);
                _jump = _instance.FindAction("Player/Jump", true);
                _crouch = _instance.FindAction("Player/Crouch", true);
                _release = _instance.FindAction("Player/ReleaseCursor", true);
                _interact = _instance.FindAction("Player/Interact", true);
            }
            _instance.Enable();
            CaptureCursor();
        }

        private void Update()
        {
            if (_release.WasPressedThisFrame()) ReleaseCursor();
        }

        public MovementCommand ReadMovement()
        {
            if (!enabled || !IsCaptured) return default;
            return new MovementCommand { Move = _move.ReadValue<Vector2>(), Sprint = _sprint.IsPressed(),
                Jump = _jump.WasPressedThisFrame(), Crouch = _crouch.IsPressed() };
        }

        public void CaptureCursor()
        {
            if (!enabled || IsTaskModal) return;
            IsCaptured = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void ReleaseCursor()
        {
            IsCaptured = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void SetTaskModal(bool active)
        {
            IsTaskModal = active;
            if (active) ReleaseCursor();
            else if (Application.isFocused) CaptureCursor();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused && enabled) ReleaseCursor();
        }

        private void OnGUI()
        {
            if (!IsTaskModal && !IsCaptured && GUI.Button(new Rect(Screen.width / 2f - 90, Screen.height - 65, 180, 40), "Resume controls"))
                CaptureCursor();
        }

        private void OnDisable()
        {
            if (_instance != null) _instance.Disable();
            if (IsCaptured) ReleaseCursor();
        }

        private void OnDestroy()
        {
            if (_instance != null) Destroy(_instance);
        }
    }
}
