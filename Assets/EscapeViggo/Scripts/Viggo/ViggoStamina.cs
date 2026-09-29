using EscapeViggo.Core;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    [RequireComponent(typeof(NetworkPlayer))]
    public sealed class ViggoStamina : NetworkBehaviour
    {
        [SerializeField] private ViggoSettings _settings;
        private readonly NetworkVariable<ViggoMobilityState> _stamina = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkPlayer _player;
        private LocalNetworkSession _session;
        private Vector3 _lastPosition;
        private double _lastSample;
        public ViggoSettings Settings => _settings;
        public float Value => _stamina.Value.Stamina;
        public ViggoMobilityState State => _stamina.Value;
        public bool Applies => IsSpawned && _player.Role == PlayerRole.Viggo;
        public bool IsRaging => Applies && _session.Workday != null && _session.Workday.GetComponent<ViggoRage>().Active;
        public bool IsResting => Applies && !IsRaging && State.Phase == ViggoMobility.Resting;
        public bool CanCapture => Applies && !IsResting;
        public event System.Action RestStarted;

        private void Awake()
        {
            _player = GetComponent<NetworkPlayer>();
            if (_settings == null)
            {
                Debug.LogError("ViggoStamina requires ViggoSettings.", this);
                enabled = false;
            }
        }

        public override void OnNetworkSpawn()
        {
            _session = NetworkManager.GetComponent<LocalNetworkSession>();
            _lastPosition = transform.position;
            _lastSample = NetworkManager.ServerTime.Time;
            if (IsServer) _stamina.Value = new ViggoMobilityState { Stamina = Applies ? _settings.MaximumStamina : 0 };
            _stamina.OnValueChanged += OnStateChanged;
        }

        private void OnStateChanged(ViggoMobilityState previous, ViggoMobilityState current)
        {
            if (current.RestCount > previous.RestCount) RestStarted?.Invoke();
        }

        public override void OnNetworkDespawn() => _stamina.OnValueChanged -= OnStateChanged;

        private void LateUpdate()
        {
            if (!IsServer || !Applies) return;
            var now = NetworkManager.ServerTime.Time;
            var seconds = (float)(now - _lastSample);
            if (seconds < 0.1f) return;
            var displacement = transform.position - _lastPosition;
            displacement.y = 0;
            if (_session.Workday != null && _session.Workday.State.Phase == MatchPhase.Workday)
                _stamina.Value = ViggoMobilityState.Advance(State, displacement.magnitude / seconds, seconds, _settings, IsRaging);
            _lastPosition = transform.position;
            _lastSample = now;
        }

        public float MovementSpeed(bool crouching, bool sprinting) =>
            (IsRaging ? _settings.RageSpeed : crouching ? _settings.CrouchSpeed : sprinting ? _settings.ChaseSpeed : _settings.WalkSpeed) *
            (IsResting ? Mathf.Clamp(_settings.RestMovementMultiplier, 0, 0.25f) : 1);
    }
}
