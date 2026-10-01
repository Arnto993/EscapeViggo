using EscapeViggo.Battery;
using EscapeViggo.Interaction;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Viggo;
using UnityEngine;

namespace EscapeViggo.Core
{
    /// <summary>Local presentation only. Never writes gameplay state or inspects remote carrier identities for display.</summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        private NetworkPlayer _player;
        private PlayerInputReader _input;
        private PlayerInteraction _interaction;
        private PlayerScore _score;
        private PlayerCapture _capture;
        private ViggoStamina _stamina;
        private LocalNetworkSession _session;
        private bool _initialized, _wasStealing, _wasCarrying, _wasRaging;
        private int _lastScore, _lastTasks, _lastCaptures, _lastDay;
        private string _feedback = "", _scoreFeedback = "";
        private float _feedbackUntil, _scoreUntil, _rageUntil;
        public bool IsVisible => _player != null && _player.IsSpawned && _player.IsOwner;
        public bool ShowsStamina => IsVisible && _player.Role == PlayerRole.Viggo;
        public bool ShowsCarrier => IsVisible && _player.Role == PlayerRole.Electrician && _session != null &&
            _session.Battery != null && _session.Battery.State.Phase == BatteryPhase.Carried &&
            _session.Battery.State.CarrierId == _player.OwnerClientId;
        public bool ShowsTheftProgress => IsVisible && _player.Role == PlayerRole.Electrician && _session != null &&
            _session.Battery != null && _session.Battery.State.Phase == BatteryPhase.BeingStolen &&
            _session.Battery.State.ActorId == _player.OwnerClientId;
        public bool ShowsRage => IsVisible && _session != null && _session.Workday != null &&
            _session.Workday.GetComponent<ViggoRage>().Active;
        public string Feedback => Time.unscaledTime < _feedbackUntil ? _feedback : "";

        private void Awake()
        {
            _player = GetComponent<NetworkPlayer>();
            _input = GetComponent<PlayerInputReader>();
            _interaction = GetComponent<PlayerInteraction>();
            _score = GetComponent<PlayerScore>();
            _capture = GetComponent<PlayerCapture>();
            _stamina = GetComponent<ViggoStamina>();
        }
        private void Update()
        {
            if (!IsVisible) { _initialized = false; return; }
            if (_session == null) _session = _player.NetworkManager.GetComponent<LocalNetworkSession>();
            if (_session.Workday != null && _session.Workday.GetComponent<WorkWeek>() is WorkWeek week && _lastDay != week.Day)
            {
                _lastDay = week.Day;
                _initialized = false;
                _feedbackUntil = _scoreUntil = _rageUntil = 0;
                _wasStealing = _wasCarrying = _wasRaging = false;
            }
            if (!_initialized)
            {
                _lastScore = _score.Value;
                _lastTasks = _score.TasksCompleted;
                _lastCaptures = _score.Captures;
                _initialized = true;
            }
            if (_lastScore != _score.Value)
            {
                var delta = (long)_score.Value - _lastScore;
                _scoreFeedback = $"{(delta > 0 ? "+" : "")}{delta} points";
                _scoreUntil = Time.unscaledTime + 4;
            }
            if (_score.TasksCompleted > _lastTasks) Notify("Task complete! Find another job or go for Viggo's battery.");
            if (_score.Captures > _lastCaptures) Notify("Capture successful!");
            if (_wasStealing && !ShowsTheftProgress && !ShowsCarrier && !_player.GameplayStopped)
                Notify("Theft cancelled. Keep holding E while aiming at the station.");
            if (!_wasCarrying && ShowsCarrier) Notify("Battery stolen! Keep it away from Viggo.");
            if (!_wasRaging && ShowsRage) _rageUntil = Time.unscaledTime + 5;
            _lastScore = _score.Value;
            _lastTasks = _score.TasksCompleted;
            _lastCaptures = _score.Captures;
            _wasStealing = ShowsTheftProgress;
            _wasCarrying = ShowsCarrier;
            _wasRaging = ShowsRage;
        }
        private void Notify(string message) { _feedback = message; _feedbackUntil = Time.unscaledTime + 4; }

        private void OnGUI()
        {
            if (!IsVisible || _session == null || _session.Workday == null) return;
            using var gui = new PrototypeGui(true);
            var width = PrototypeGui.Width;
            var height = PrototypeGui.Height;
            var state = _session.Workday.State;
            var week = _session.Workday.GetComponent<WorkWeek>();
            var total = (long)(week != null ? week.TotalFor(_player.OwnerClientId).Score : 0) + (state.Phase == MatchPhase.EndOfDay ? 0 : _score.Value);
            GUI.Box(new Rect(20, 16, 310, 66), $"{_player.Role}  |  Player {_player.OwnerClientId}\nToday: {_score.Value} | Week: {total}");
            GUI.Box(new Rect(width / 2 - 125, 16, 250, 66), $"{week?.DayName} {state.ClockText}\n{(state.Phase == MatchPhase.EndOfDay ? "Workday finished" : state.Phase.ToString())}");
            GUI.Box(new Rect(width - 275, 16, 255, 40), "Esc: menu / release mouse");
            if (state.Phase == MatchPhase.EndOfDay) return;
            GUI.Box(new Rect(20, 94, 370, 66), ShowsStamina
                ? "Protect your battery. Check its station.\nGet close and press E to capture workers."
                : "Find yellow task stations around the worksite.\nHold E at the battery station to steal.");
            if (ShowsStamina)
            {
                PrototypeGui.Bar(new Rect(20, 174, 350, 32), _stamina.Value / _stamina.Settings.MaximumStamina,
                    ShowsRage ? "RAGE: unlimited pursuit" : $"Stamina: {_stamina.Value:F0} / {_stamina.Settings.MaximumStamina:F0}", new Color(0.15f, 0.4f, 0.25f));
                if (_stamina.IsResting)
                    GUI.Box(new Rect(20, 214, 350, 65), $"Javell.\nResting: {_stamina.State.RestRemaining:F1}s - wait to move");
            }
            if (ShowsCarrier) GUI.Box(new Rect(width / 2 - 240, 174, 480, 40), "CARRYING VIGGO'S BATTERY - keep it hidden!");
            if (ShowsTheftProgress)
                PrototypeGui.Bar(new Rect(width / 2 - 240, 224, 480, 36), _session.Battery.State.Progress,
                    $"Hold E - stealing battery {_session.Battery.State.Progress:P0}", new Color(0.4f, 0.3f, 0.05f));
            if (ShowsRage)
            {
                var previous = GUI.color;
                GUI.color = new Color(0.8f, 0.12f, 0.05f, 0.8f);
                GUI.DrawTexture(new Rect(0, 0, width, 7), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(0, height - 7, width, 7), Texture2D.whiteTexture);
                GUI.color = previous;
                GUI.Box(new Rect(width / 2 - 240, 94, 480, 65), _session.Workday.GetComponent<RagePresentation>().WarningFor(ShowsStamina));
                if (Time.unscaledTime < _rageUntil)
                    GUI.Box(new Rect(width / 2 - 240, height / 2 - 90, 480, 40), "THE THEFT HAS BEEN DISCOVERED!");
            }
            if (_capture.IsCaptured)
                GUI.Box(new Rect(width / 2 - 250, height / 2 - 40, 500, 66),
                    $"CAPTURED! Returning to worker spawn in {Mathf.Max(0, (float)(_capture.State.ReleaseAt - _player.NetworkManager.ServerTime.Time)):F1}s\nYou will be able to play again shortly.");
            else if (_input.IsCaptured && !_input.IsTaskModal)
            {
                GUI.Label(new Rect(width / 2 - 5, height / 2 - 12, 20, 25), "+");
                if (_interaction.Target != null) GUI.Box(new Rect(width / 2 - 250, height / 2 + 32, 500, 36), _interaction.Prompt);
            }
            if (Feedback.Length > 0) GUI.Box(new Rect(width / 2 - 320, height - 150, 640, 36), Feedback);
            if (Time.unscaledTime < _scoreUntil) GUI.Box(new Rect(20, 290, 210, 36), _scoreFeedback);
            GUI.Box(new Rect(width / 2 - 340, height - 42, 680, 30), "WASD move | Mouse look | Shift sprint | Space jump | Ctrl/C crouch | E interact");
        }
    }
}
