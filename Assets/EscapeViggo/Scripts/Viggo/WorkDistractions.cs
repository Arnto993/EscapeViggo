using System;
using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    [RequireComponent(typeof(NetworkWorkday), typeof(ScoreService))]
    public sealed class WorkDistractions : NetworkBehaviour
    {
        [SerializeField] private DistractionSettings _settings;
        private readonly NetworkVariable<DistractionState> _state = new(default);
        private NetworkWorkday _workday;
        private ScoreService _scores;
        public DistractionSettings Settings => _settings;
        public DistractionState State => _state.Value;
        public bool PowerOff => IsSpawned && State.PowerUntil > 0;
        // Presentation hooks only; correctness depends on authoritative state, never subscribers.
        public event Action<ulong> WorkInterrupted;
        public event Action PowerCut, PowerRestored;
        private void Awake() { _workday = GetComponent<NetworkWorkday>(); _scores = GetComponent<ScoreService>(); }
        public override void OnNetworkSpawn() => _state.OnValueChanged += Changed;
        public override void OnNetworkDespawn() => _state.OnValueChanged -= Changed;
        private void Changed(DistractionState previous, DistractionState current)
        {
            if (current.Sequence > previous.Sequence && current.Kind == DistractionKind.WorkInterruption) WorkInterrupted?.Invoke(current.VictimId);
            if (previous.PowerUntil == 0 && current.PowerUntil > 0) PowerCut?.Invoke();
            if (previous.PowerUntil > 0 && current.PowerUntil == 0) PowerRestored?.Invoke();
        }
        public bool CanInterrupt(PlayerInteraction actor, WorkTask task) => IsSpawned && _settings != null && actor != null && actor.CanAct &&
            task != null && task.IsSpawned && !PowerOff && DistractionRules.CanInterrupt(actor.Player.Role, _workday.State.Phase,
                actor.GetComponent<ViggoStamina>().IsResting, task.HasProgress, NetworkManager.ServerTime.Time, State.InterruptReadyAt,
                Vector3.Distance(actor.transform.position, task.transform.position), _settings.InterruptRange);
        internal bool InterruptOnServer(PlayerInteraction actor, WorkTask task)
        {
            if (!IsServer || !CanInterrupt(actor, task) || !actor.ServerCanReach(task)) return false;
            var victim = task.State.WorkerId;
            if (!NetworkManager.ConnectedClients.TryGetValue(victim, out var client) || client.PlayerObject == null ||
                client.PlayerObject.GetComponent<Networking.NetworkPlayer>().Role != PlayerRole.Electrician) return false;
            var next = State;
            next.InterruptReadyAt = NetworkManager.ServerTime.Time + Mathf.Max(1, _settings.InterruptCooldownSeconds);
            next.Sequence++; next.ActorId = actor.OwnerClientId; next.VictimId = victim;
            next.Kind = DistractionKind.WorkInterruption; next.Meaningful = true;
            task.InterruptOnServer(NetworkManager.ServerTime.Time + Mathf.Max(0, _settings.TaskRetrySeconds));
            _state.Value = next;
            _scores.AwardDistraction(this, actor.Player);
            return true;
        }
        public bool CanUsePower(PlayerInteraction actor)
        {
            if (!IsSpawned || _settings == null || actor == null || !actor.CanAct) return false;
            var now = NetworkManager.ServerTime.Time;
            return DistractionRules.CanCut(actor.Player.Role, _workday.State.Phase, actor.GetComponent<ViggoStamina>().IsResting, now, State) ||
                DistractionRules.CanRestore(actor.Player.Role, _workday.State.Phase, now, State);
        }
        internal void UsePowerOnServer(PlayerInteraction actor, PowerControl control)
        {
            if (!IsServer || !CanUsePower(actor) || !actor.ServerCanReach(control)) return;
            if (actor.Player.Role == PlayerRole.Electrician) { StopOnServer(); return; }
            var meaningful = false;
            foreach (var task in _scores.Tasks)
            {
                if (task == null) continue;
                meaningful |= task.HasProgress;
                task.StopOnServer();
            }
            var now = NetworkManager.ServerTime.Time;
            var next = State;
            next.PowerUntil = now + Mathf.Max(1, _settings.PowerCutSeconds);
            next.RestoreAt = now + Mathf.Clamp(_settings.RestoreDelaySeconds, 0, _settings.PowerCutSeconds);
            // Always leave a recovery window, even if an inspector value is misconfigured.
            next.PowerReadyAt = now + Mathf.Max(_settings.PowerCutSeconds + 1, _settings.PowerCooldownSeconds);
            next.Sequence++; next.ActorId = actor.OwnerClientId; next.VictimId = ulong.MaxValue;
            next.Kind = DistractionKind.PowerCut; next.Meaningful = meaningful;
            _state.Value = next;
            _scores.AwardDistraction(this, actor.Player);
        }
        private void Update()
        {
            if (!IsServer || !IsSpawned || !PowerOff) return;
            if (_workday.State.Phase != MatchPhase.Workday || NetworkManager.ServerTime.Time >= State.PowerUntil) StopOnServer();
        }
        internal void StopOnServer() { if (IsServer && IsSpawned) _state.Value = DistractionRules.StopPower(State); }
        internal void ResetDayOnServer() { if (IsServer && IsSpawned) _state.Value = DistractionRules.NewDay(State); }
    }
}
