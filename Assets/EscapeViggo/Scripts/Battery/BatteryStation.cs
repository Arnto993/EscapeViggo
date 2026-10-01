using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Battery
{
    public sealed class BatteryStation : NetworkInteractable
    {
        [SerializeField] private BatterySettings _settings;
        private readonly NetworkVariable<BatteryState> _state = new(BatteryState.Initial,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private LocalNetworkSession _session;
        private double _startedAt, _lastPulse, _nextUpdate;
        public BatteryState State => _state.Value;
        public BatterySettings Settings => _settings;
        public bool IsWorkday => _session != null && _session.Workday != null && _session.Workday.State.Phase == MatchPhase.Workday;
        public override string Prompt => "Hold to steal battery";
        public override string GetPrompt(PlayerInteraction player) => player.Player.Role == PlayerRole.Viggo ? "Check battery station" : Prompt;
        public override bool RequiresHold(PlayerInteraction player) => player.Player.Role == PlayerRole.Electrician;
        public override bool CanInteract(PlayerInteraction player) => IsSpawned && IsWorkday && player != null && player.CanAct &&
            (player.Player.Role == PlayerRole.Viggo || (State.Phase == BatteryPhase.AtDrill ||
                (State.Phase == BatteryPhase.BeingStolen && State.ActorId == player.OwnerClientId)));

        public override void OnNetworkSpawn()
        {
            _session = NetworkManager.GetComponent<LocalNetworkSession>();
            _session.RegisterBattery(this);
            if (_settings == null) Debug.LogError("BatteryStation requires BatterySettings.", this);
        }

        protected override void OnServerInteract(PlayerInteraction player)
        {
            var next = State;
            if (player.Player.Role == PlayerRole.Viggo)
            {
                if (BatteryRules.Discover(ref next, player.Player.Role))
                {
                    _state.Value = next;
                    _session.Workday.GetComponent<Viggo.ViggoRage>().BeginOnServer(this);
                }
                return;
            }
            if (BatteryRules.TryBegin(ref next, player.OwnerClientId))
            {
                _state.Value = next;
                _startedAt = NetworkManager.ServerTime.Time;
            }
            if (State.Phase == BatteryPhase.BeingStolen && State.ActorId == player.OwnerClientId)
                _lastPulse = NetworkManager.ServerTime.Time;
        }

        internal override void EndOnServer(PlayerInteraction player)
        {
            if (!IsServer) return;
            var next = State;
            if (BatteryRules.Cancel(ref next, player.OwnerClientId)) _state.Value = next;
        }

        internal bool RecoverCapturedCarrier(ulong carrier)
        {
            if (!IsServer || !IsSpawned || !IsWorkday || State.Phase != BatteryPhase.Carried || State.CarrierId != carrier) return false;
            var next = State;
            next.Phase = BatteryPhase.Secured;
            next.CarrierId = BatteryState.Nobody;
            _state.Value = next;
            var rage = _session.Workday.GetComponent<Viggo.ViggoRage>();
            if (rage.Settings.EndRageOnRecovery || rage.Settings.EndRageOnCarrierCapture) rage.EndOnServer();
            return true;
        }

        private void Update()
        {
            if (!IsSpawned || !IsServer || _settings == null || !IsWorkday) return;
            var now = NetworkManager.ServerTime.Time;
            if (now < _nextUpdate) return;
            _nextUpdate = now + 0.05;
            var next = State;
            if (next.Phase == BatteryPhase.Carried)
            {
                // Disconnect safely returns the single battery; no orphaned carrier or second object.
                if (!NetworkManager.ConnectedClients.ContainsKey(next.CarrierId))
                {
                    next.Phase = BatteryPhase.AtDrill;
                    next.CarrierId = BatteryState.Nobody;
                    next.Progress = 0;
                    _state.Value = next;
                    var rage = _session.Workday.GetComponent<Viggo.ViggoRage>();
                    if (rage.Settings.EndRageOnRecovery) rage.EndOnServer();
                }
                return;
            }
            if (next.Phase != BatteryPhase.BeingStolen) return;
            var valid = IsWorkday && now - _lastPulse <= _settings.HoldTimeoutSeconds &&
                NetworkManager.ConnectedClients.TryGetValue(next.ActorId, out var client) && client.PlayerObject != null;
            if (valid)
            {
                var player = NetworkManager.ConnectedClients[next.ActorId].PlayerObject.GetComponent<PlayerInteraction>();
                valid = CanInteract(player) && player.ServerCanReach(this);
            }
            if (!valid) BatteryRules.Cancel(ref next, next.ActorId);
            else
            {
                next.Progress = Mathf.Clamp01((float)((now - _startedAt) / _settings.TheftSeconds));
                BatteryRules.Complete(ref next);
            }
            _state.Value = next;
        }

        internal void StopTheftOnServer()
        {
            if (!IsServer || !IsSpawned) return;
            var next = State;
            if (BatteryRules.Cancel(ref next, next.ActorId)) _state.Value = next;
        }

        internal void ResetDayOnServer()
        {
            if (!IsServer || !IsSpawned) return;
            var initial = BatteryState.Initial;
            initial.TheftId = State.TheftId;
            _state.Value = initial;
            _startedAt = _lastPulse = _nextUpdate = 0;
        }

        public override void OnNetworkDespawn()
        {
            if (_session.Battery == this) _session.RegisterBattery(null);
        }
    }
}
