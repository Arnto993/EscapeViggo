using EscapeViggo.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeViggo.Networking
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private Renderer _body;
        [SerializeField] private Camera _camera;
        [SerializeField] private AudioListener _listener;
        [SerializeField] private Material _viggoMaterial;
        [SerializeField] private Material _electricianMaterial;
        [SerializeField] private Vector3 _viggoBodyScale = new(1.6f, 1.15f, 1.6f);
        private readonly NetworkVariable<PlayerRole> _role = new(PlayerRole.Electrician,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public PlayerRole Role => _role.Value;
        internal void AssignRoleOnServer(PlayerRole role) { if (IsServer && IsSpawned) _role.Value = role; }
        private PlayerInputReader _input;
        private PlayerMovement _movement;
        private PlayerLook _look;
        private CharacterController _controller;
        private LocalNetworkSession _session;
        public bool GameplayStopped => IsSpawned && _session != null && _session.Workday != null &&
            _session.Workday.State.Phase == Core.MatchPhase.EndOfDay;
        // Presentation only. Future captures and other outcomes remain server-owned.
        private readonly NetworkVariable<bool> _crouching = new(false,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public bool IsCrouching => _crouching.Value;

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            _movement = GetComponent<PlayerMovement>();
            _look = GetComponent<PlayerLook>();
            _controller = GetComponent<CharacterController>();
            SetLocalControl(false);
        }

        public override void OnNetworkSpawn()
        {
            name = $"Player {OwnerClientId}";
            _session = NetworkManager.GetComponent<LocalNetworkSession>();
            if (IsServer) _role.Value = RoleAssignment.ForClient(OwnerClientId);
            SetLocalControl(IsOwner);
            _role.OnValueChanged += OnRoleChanged;
            OnRoleChanged(_role.Value, _role.Value);
            _crouching.OnValueChanged += OnCrouchChanged;
            OnCrouchChanged(false, _crouching.Value);
            Debug.Log($"Player spawned: owner={OwnerClientId}, local={IsOwner}", this);
        }

        private void LateUpdate()
        {
            if (IsSpawned && IsOwner && _crouching.Value != _movement.IsCrouching)
                _crouching.Value = _movement.IsCrouching;
            if (IsSpawned) ApplyBody();
        }

        private void OnRoleChanged(PlayerRole previous, PlayerRole current)
        {
            _body.sharedMaterial = current == PlayerRole.Viggo ? _viggoMaterial : _electricianMaterial;
            ApplyBody();
        }

        private void OnCrouchChanged(bool previous, bool current) => ApplyBody();

        private void ApplyBody()
        {
            var height = IsCrouching ? _movement.Settings.CrouchingHeight : _movement.Settings.StandingHeight;
            var scale = Role == PlayerRole.Viggo ? _viggoBodyScale : Vector3.one;
            height *= scale.y;
            _body.transform.localPosition = Vector3.up * (height / 2);
            _body.transform.localScale = new Vector3(_controller.radius * 2 * scale.x, height / 2, _controller.radius * 2 * scale.z);
        }

        private void SetLocalControl(bool local)
        {
            _controller.enabled = local;
            _input.enabled = local;
            _movement.enabled = local;
            _look.enabled = local;
            _camera.enabled = local;
            _listener.enabled = local;
            _body.shadowCastingMode = local ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
        }

        public override void OnGainedOwnership() { if (IsSpawned) SetLocalControl(true); }
        public override void OnLostOwnership() { if (IsSpawned) SetLocalControl(false); }

        public override void OnNetworkDespawn()
        {
            _crouching.OnValueChanged -= OnCrouchChanged;
            _role.OnValueChanged -= OnRoleChanged;
            SetLocalControl(false);
        }
    }
}
