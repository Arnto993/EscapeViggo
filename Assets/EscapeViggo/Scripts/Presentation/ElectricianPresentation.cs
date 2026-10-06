using EscapeViggo.Networking;
using EscapeViggo.Player;
using EscapeViggo.Viggo;
using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeViggo.Presentation
{
    // Observes existing local/replicated transforms. Owns no gameplay or network state.
    [DefaultExecutionOrder(100)]
    public sealed class ElectricianPresentation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Animator _viggoAnimator;
        [SerializeField] private Renderer _placeholder;
        private NetworkPlayer _network;
        private PlayerMovement _movement;
        private ViggoStamina _stamina;
        private Renderer[] _renderers;
        private Renderer[] _viggoRenderers;
        private Vector3 _previousPosition;
        private bool _seeded;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Resting = Animator.StringToHash("Resting");
        public Animator Animator => _animator;
        public Animator ViggoAnimator => _viggoAnimator;

        private void Awake()
        {
            _network = GetComponent<NetworkPlayer>();
            _movement = GetComponent<PlayerMovement>();
            _stamina = GetComponent<ViggoStamina>();
            _renderers = _animator.GetComponentsInChildren<Renderer>(true);
            _animator.applyRootMotion = false;
            _viggoRenderers = _viggoAnimator ? _viggoAnimator.GetComponentsInChildren<Renderer>(true) : System.Array.Empty<Renderer>();
            if (_viggoAnimator) _viggoAnimator.applyRootMotion = false;
        }

        private void OnEnable() { _seeded = false; }

        private void LateUpdate()
        {
            var active = _network == null || (_network.IsSpawned && _network.Role == PlayerRole.Electrician);
            var local = _network == null || _network.IsOwner;
            var viggoActive = _viggoAnimator && _network != null && _network.IsSpawned && _network.Role == PlayerRole.Viggo;
            _animator.gameObject.SetActive(active);
            if (_viggoAnimator) _viggoAnimator.gameObject.SetActive(viggoActive);
            _placeholder.enabled = !active && !viggoActive;
            foreach (var renderer in _renderers)
                renderer.shadowCastingMode = local ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            foreach (var renderer in _viggoRenderers)
                renderer.shadowCastingMode = local ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            var delta = transform.position - _previousPosition;
            delta.y = 0;
            var speed = _seeded && Time.deltaTime > 0 && delta.sqrMagnitude < 4
                ? delta.magnitude / Time.deltaTime : 0;
            _previousPosition = transform.position;
            _seeded = true;
            if (!active && !viggoActive) return;
            var visibleAnimator = viggoActive ? _viggoAnimator : _animator;
            // Blend thresholds are normalized against the existing movement configuration.
            var walkSpeed = viggoActive && _stamina ? _stamina.Settings.WalkSpeed : _movement.Settings.WalkSpeed;
            var runSpeed = viggoActive && _stamina ? _stamina.Settings.ChaseSpeed : _movement.Settings.SprintSpeed;
            var resting = viggoActive && _stamina && _stamina.IsResting;
            if (viggoActive) visibleAnimator.SetBool(Resting, resting);
            var normalized = resting ? 0 : speed <= walkSpeed
                ? speed / walkSpeed
                : 1 + (speed - walkSpeed) / Mathf.Max(.01f, runSpeed - walkSpeed);
            visibleAnimator.SetFloat(Speed, Mathf.Clamp(normalized, 0, 2), .10f, Time.deltaTime);
            // Minimal crouch accommodation until the later contextual animation pass.
            var crouch = _network != null ? _network.IsCrouching : _movement.IsCrouching;
            visibleAnimator.transform.localScale = new Vector3(1,
                crouch ? _movement.Settings.CrouchingHeight / _movement.Settings.StandingHeight : 1, 1);
        }

        private void OnDisable()
        {
            if (_placeholder) _placeholder.enabled = true;
            if (_animator) _animator.gameObject.SetActive(false);
            if (_viggoAnimator) _viggoAnimator.gameObject.SetActive(false);
        }
    }
}
