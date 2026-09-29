using UnityEngine;

namespace EscapeViggo.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private MovementSettings _settings;
        [SerializeField] private Transform _body;
        private CharacterController _controller;
        private PlayerInputReader _input;
        private Viggo.ViggoStamina _stamina;
        private PlayerCapture _capture;
        private float _verticalSpeed;
        private readonly Collider[] _ceilingHits = new Collider[16];
        public bool IsCrouching { get; private set; }
        public bool IsGrounded => _controller != null && _controller.isGrounded;
        public float Height => _controller.height;
        public MovementSettings Settings => _settings;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputReader>();
            _stamina = GetComponent<Viggo.ViggoStamina>();
            _capture = GetComponent<PlayerCapture>();
            if (_settings == null)
            {
                Debug.LogError("PlayerMovement requires MovementSettings.", this);
                enabled = false;
            }
        }

        private void Update() => Simulate(_input.ReadMovement(), Time.deltaTime);

        // One movement path for input and deterministic physics checks; no gameplay outcomes.
        public void Simulate(MovementCommand command, float deltaTime)
        {
            if (_settings == null || !_controller.enabled || deltaTime <= 0) return;
            var crouch = command.Crouch || (IsCrouching && !CanStand());
            if (crouch != IsCrouching)
            {
                IsCrouching = crouch;
                _controller.height = crouch ? _settings.CrouchingHeight : _settings.StandingHeight;
                _controller.center = Vector3.up * (_controller.height / 2);
                _body.localPosition = _controller.center;
                _body.localScale = new Vector3(_controller.radius * 2, _controller.height / 2, _controller.radius * 2);
            }
            var move = Vector2.ClampMagnitude(command.Move, 1);
            var speed = IsCrouching ? _settings.CrouchSpeed : command.Sprint ? _settings.SprintSpeed : _settings.WalkSpeed;
            if (_stamina != null && _stamina.Applies) speed = _stamina.MovementSpeed(IsCrouching, command.Sprint);
            if (_capture != null && _capture.IsCaptured) speed = 0;
            if (IsGrounded && _verticalSpeed < 0) _verticalSpeed = -_settings.GroundStickSpeed;
            if (command.Jump && IsGrounded && !IsCrouching && (_stamina == null || !_stamina.IsResting) &&
                (_capture == null || !_capture.IsCaptured))
                _verticalSpeed = Mathf.Sqrt(2 * _settings.Gravity * _settings.JumpHeight);
            _verticalSpeed -= _settings.Gravity * deltaTime;
            var velocity = (transform.right * move.x + transform.forward * move.y) * speed;
            velocity.y = _verticalSpeed;
            var collision = _controller.Move(velocity * deltaTime);
            if ((collision & CollisionFlags.Above) != 0 && _verticalSpeed > 0) _verticalSpeed = 0;
        }

        private bool CanStand()
        {
            var radius = _controller.radius - 0.02f;
            var bottom = transform.position + Vector3.up * (_settings.CrouchingHeight - radius);
            var top = transform.position + Vector3.up * (_settings.StandingHeight - radius);
            var count = Physics.OverlapCapsuleNonAlloc(bottom, top, radius, _ceilingHits, ~0, QueryTriggerInteraction.Ignore);
            if (count == _ceilingHits.Length) return false;
            for (var i = 0; i < count; i++)
                if (_ceilingHits[i] != _controller && !_ceilingHits[i].transform.IsChildOf(transform)) return false;
            return true;
        }

        internal void ResetVerticalSpeed() => _verticalSpeed = 0;
    }
}
