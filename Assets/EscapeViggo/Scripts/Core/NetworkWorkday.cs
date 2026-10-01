using EscapeViggo.Networking;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Core
{
    [RequireComponent(typeof(NetworkObject))]
    public sealed class NetworkWorkday : NetworkBehaviour
    {
        [SerializeField] private WorkdaySettings _settings;
        private readonly NetworkVariable<WorkdayState> _state = new(
            new WorkdayState { Phase = MatchPhase.Preparing, MinuteOfDay = WorkdayClock.StartMinute },
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private double _startedAt, _nextPublish;
        private double _durationMinutes;
        public WorkdaySettings Settings => _settings;
        public WorkdayState State => _state.Value;
        public double DurationMinutes => _durationMinutes;
        public event System.Action<WorkdayState> StateChanged;

        public override void OnNetworkSpawn()
        {
            NetworkManager.GetComponent<LocalNetworkSession>().RegisterWorkday(this);
            _state.OnValueChanged += Changed;
            if (!IsServer) return;
            _durationMinutes = _settings.DayDurationMinutes;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _durationMinutes = NetworkManager.GetComponent<LocalNetworkSession>().PlaytestDayMinutes ?? _durationMinutes;
            // Opt-in automation uses the same timer path with a shorter duration.
            if (DevelopmentNetworkPanel.HasArgument("-ev-smoke")) _durationMinutes = 120d / 60;
#endif
            _startedAt = NetworkManager.ServerTime.Time;
            _nextPublish = _startedAt;
            _state.Value = WorkdayClock.Evaluate(0, _settings.PreparingSeconds, _durationMinutes);
        }

        private void Update()
        {
            if (!IsSpawned || !IsServer || State.Phase == MatchPhase.EndOfDay) return;
            var now = NetworkManager.ServerTime.Time;
            if (now < _nextPublish) return;
            _nextPublish = now + 0.1;
            _state.Value = WorkdayClock.Evaluate(now - _startedAt, _settings.PreparingSeconds, _durationMinutes);
        }

        internal void RestartDayOnServer()
        {
            if (!IsServer || !IsSpawned) return;
            _startedAt = NetworkManager.ServerTime.Time;
            _nextPublish = _startedAt;
            _state.Value = WorkdayClock.Evaluate(0, _settings.PreparingSeconds, _durationMinutes);
        }

        public override void OnNetworkDespawn()
        {
            _state.OnValueChanged -= Changed;
            var session = NetworkManager.GetComponent<LocalNetworkSession>();
            if (session.Workday == this) session.RegisterWorkday(null);
        }

        private void Changed(WorkdayState previous, WorkdayState current) => StateChanged?.Invoke(current);
    }
}
