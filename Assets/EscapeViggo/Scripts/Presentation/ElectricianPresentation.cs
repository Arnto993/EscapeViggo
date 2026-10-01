using EscapeViggo.Networking;
using EscapeViggo.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeViggo.Presentation
{
    // Observes existing local/replicated transforms. Owns no gameplay or network state.
    [DefaultExecutionOrder(100)]
    public sealed class ElectricianPresentation : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Renderer _placeholder;
        private NetworkPlayer _network;
        private PlayerMovement _movement;
        private Renderer[] _renderers;
        private Vector3 _previousPosition;
        private bool _seeded;
        private static readonly int Speed = Animator.StringToHash("Speed");
        public Animator Animator => _animator;

        private void Awake()
        {
            _network = GetComponent<NetworkPlayer>();
            _movement = GetComponent<PlayerMovement>();
            _renderers = _animator.GetComponentsInChildren<Renderer>(true);
            _animator.applyRootMotion = false;
        }

        private void OnEnable() { _seeded = false; }

        private void LateUpdate()
        {
            var active = _network == null || (_network.IsSpawned && _network.Role == PlayerRole.Electrician);
            var local = _network == null || _network.IsOwner;
            _animator.gameObject.SetActive(active);
            _placeholder.enabled = !active;
            foreach (var renderer in _renderers)
                renderer.shadowCastingMode = local ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
            var delta = transform.position - _previousPosition;
            delta.y = 0;
            var speed = _seeded && Time.deltaTime > 0 && delta.sqrMagnitude < 4
                ? delta.magnitude / Time.deltaTime : 0;
            _previousPosition = transform.position;
            _seeded = true;
            if (!active) return;
            // Blend thresholds are normalized against the existing movement configuration.
            var normalized = speed <= _movement.Settings.WalkSpeed
                ? speed / _movement.Settings.WalkSpeed
                : 1 + (speed - _movement.Settings.WalkSpeed) /
                    Mathf.Max(.01f, _movement.Settings.SprintSpeed - _movement.Settings.WalkSpeed);
            _animator.SetFloat(Speed, Mathf.Clamp(normalized, 0, 2), .10f, Time.deltaTime);
            // Minimal crouch accommodation until the later contextual animation pass.
            var crouch = _network != null ? _network.IsCrouching : _movement.IsCrouching;
            _animator.transform.localScale = new Vector3(1,
                crouch ? _movement.Settings.CrouchingHeight / _movement.Settings.StandingHeight : 1, 1);
        }

        private void OnDisable()
        {
            if (_placeholder) _placeholder.enabled = true;
            if (_animator) _animator.gameObject.SetActive(false);
        }
    }
}
