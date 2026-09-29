using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Networking;
using EscapeViggo.Scoring;
using EscapeViggo.Viggo;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace EscapeViggo.Player
{
    public sealed class PlayerCapture : NetworkInteractable
    {
        [SerializeField] private CaptureSettings _settings;
        private readonly NetworkVariable<CaptureState> _state = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private LocalNetworkSession _session;
        private NetworkPlayer _player;
        private BoxCollider _captureCollider;
        public CaptureSettings Settings => _settings;
        public CaptureState State => _state.Value;
        public bool IsCaptured => IsSpawned && State.Phase == CapturePhase.Captured;
        public override Collider InteractionCollider => _captureCollider != null ? _captureCollider : _captureCollider = GetComponent<BoxCollider>();
        public override string Prompt => "Capture electrician";
        private void Awake() => _player = GetComponent<NetworkPlayer>();
        public override void OnNetworkSpawn() => _session = NetworkManager.GetComponent<LocalNetworkSession>();

        public override bool CanInteract(PlayerInteraction actor) => IsSpawned && actor != null && actor.CanAct &&
            _session.Workday != null && CaptureRules.CanCapture(actor.Player.Role, _player.Role, _session.Workday.State.Phase,
                !actor.GetComponent<ViggoStamina>().CanCapture, State, NetworkManager.ServerTime.Time,
                Vector3.Distance(actor.transform.position, transform.position), _settings.Range);

        protected override void OnServerInteract(PlayerInteraction actor)
        {
            var next = State;
            next.Phase = CapturePhase.Captured;
            next.Sequence++;
            next.CaptorId = actor.OwnerClientId;
            next.ReleaseAt = NetworkManager.ServerTime.Time + _settings.DisableSeconds;
            _state.Value = next; // Lock before rewards or any repeated request.
            var wasCarrier = _session.Battery != null && _session.Battery.RecoverCapturedCarrier(OwnerClientId);
            _session.Workday.GetComponent<ScoreService>().AwardCapture(this, actor.Player, wasCarrier);
        }

        private void Update()
        {
            if (!IsServer || !IsCaptured || NetworkManager.ServerTime.Time < State.ReleaseAt) return;
            var spawn = _session.SpawnPointFor(OwnerClientId);
            RespawnRpc(spawn.position, spawn.rotation);
            var next = State;
            next.Phase = CapturePhase.Active;
            next.ProtectedUntil = NetworkManager.ServerTime.Time + _settings.RespawnProtectionSeconds;
            _state.Value = next;
        }

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
        private void RespawnRpc(Vector3 position, Quaternion rotation, RpcParams rpcParams = default)
        {
            if (!IsOwner || rpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) return;
            var controller = GetComponent<CharacterController>();
            controller.enabled = false;
            GetComponent<PlayerMovement>().ResetVerticalSpeed();
            GetComponent<NetworkTransform>().Teleport(position, rotation, transform.localScale);
            controller.enabled = true;
        }

        private void OnGUI()
        {
            if (IsCaptured && IsOwner)
                GUI.Box(new Rect(Screen.width / 2f - 175, 190, 350, 35), "Captured! Returning to worker spawn shortly.");
        }
    }
}
