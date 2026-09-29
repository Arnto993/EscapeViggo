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
        private PlayerInputReader _input;
        private PlayerMovement _movement;
        private PlayerLook _look;
        private CharacterController _controller;
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
            SetLocalControl(IsOwner);
            var tint = new MaterialPropertyBlock();
            tint.SetColor("_BaseColor", Color.HSVToRGB((OwnerClientId * 0.21f + 0.52f) % 1, 0.65f, 0.95f));
            _body.SetPropertyBlock(tint);
            _crouching.OnValueChanged += OnCrouchChanged;
            OnCrouchChanged(false, _crouching.Value);
            Debug.Log($"Player spawned: owner={OwnerClientId}, local={IsOwner}", this);
        }

        private void LateUpdate()
        {
            if (IsSpawned && IsOwner && _crouching.Value != _movement.IsCrouching)
                _crouching.Value = _movement.IsCrouching;
        }

        private void OnCrouchChanged(bool previous, bool current)
        {
            if (IsOwner) return;
            var height = current ? _movement.Settings.CrouchingHeight : _movement.Settings.StandingHeight;
            _body.transform.localPosition = Vector3.up * (height / 2);
            _body.transform.localScale = new Vector3(_controller.radius * 2, height / 2, _controller.radius * 2);
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
            SetLocalControl(false);
        }
    }
}
