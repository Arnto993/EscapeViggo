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
        private readonly NetworkVariable<float> _stamina = new(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkPlayer _player;
        private LocalNetworkSession _session;
        private Vector3 _lastPosition;
        private double _lastSample;
        public ViggoSettings Settings => _settings;
        public float Value => _stamina.Value;
        public bool Applies => IsSpawned && _player.Role == PlayerRole.Viggo;

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
            if (IsServer) _stamina.Value = Applies ? _settings.MaximumStamina : 0;
        }

        private void LateUpdate()
        {
            if (!IsServer || !Applies) return;
            var now = NetworkManager.ServerTime.Time;
            var seconds = (float)(now - _lastSample);
            if (seconds < 0.1f) return;
            var displacement = transform.position - _lastPosition;
            displacement.y = 0;
            if (_session.Workday != null && _session.Workday.State.Phase == MatchPhase.Workday)
                _stamina.Value = StaminaRules.Advance(Value, displacement.magnitude / seconds, seconds, _settings);
            _lastPosition = transform.position;
            _lastSample = now;
        }

        public float MovementSpeed(bool crouching, bool sprinting) =>
            crouching ? _settings.CrouchSpeed : sprinting ? _settings.ChaseSpeed : _settings.WalkSpeed;
    }
}
