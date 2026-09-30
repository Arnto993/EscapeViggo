using System;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using UnityEngine;

namespace EscapeViggo.Networking
{
    public sealed class DevelopmentNetworkPanel : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private LocalNetworkSession _session;
        private string _address = "127.0.0.1";
        private string _port = "7777";
        private bool _diagnostics;
        private int _dayPreset = 1;
        private string _commandLineMinutes;
        private readonly string[] _presets = { "Quick: 2 min", "Guided: 5 min", "Asset tuning" };

        private void Start()
        {
            _address = Argument("-ev-address") ?? _address;
            _port = Argument("-ev-port") ?? _port;
            _commandLineMinutes = Argument("-ev-day-minutes");
            if (HasArgument("-ev-connection-smoke")) return;
            if (HasArgument("-ev-host")) Connect(true);
            else if (HasArgument("-ev-client")) Connect(false);
        }

        private void Connect(bool host)
        {
            if (!ushort.TryParse(_port, out var port) || port == 0) return;
            if (host)
            {
                var minutes = _commandLineMinutes ?? (_dayPreset == 0 ? "2" : _dayPreset == 1 ? "5" : null);
                if (!_session.ConfigurePlaytestDay(minutes)) return;
                _session.StartHost(_address, port);
            }
            else _session.StartClient(_address, port);
        }

        private void OnGUI()
        {
            var localInput = _session.Manager.LocalClient?.PlayerObject?.GetComponent<PlayerInputReader>();
            if (localInput != null && (localInput.IsCaptured || localInput.IsTaskModal ||
                (_session.Workday != null && _session.Workday.State.Phase == Core.MatchPhase.EndOfDay))) return;
            using var gui = new Core.PrototypeGui(true);
            var connected = localInput != null;
            var panel = connected ? new Rect(Core.PrototypeGui.Width - 400, 180, 380, Core.PrototypeGui.Height - 200)
                : new Rect(Core.PrototypeGui.Width / 2 - 310, 60, 620, Core.PrototypeGui.Height - 120);
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label("ESCAPE VIGGO - LOCAL PLAYTEST");
            GUILayout.Label(_session.Status);
            GUILayout.Label($"Players: {_session.PlayerCount} / 8");
            if (!connected)
            {
                GUILayout.Label("Host plays Viggo. Joining players are electricians.");
                GUILayout.Label("On this PC, use 127.0.0.1 in every window.");
                GUILayout.Label("The workday starts automatically after five seconds.");
                GUILayout.Space(12);
            }
            if (_session.CanStart)
            {
                GUILayout.Label("IP address / port");
                _address = GUILayout.TextField(_address);
                _port = GUILayout.TextField(_port);
                GUILayout.Label("Host workday length (clients use the host's clock)");
                _dayPreset = GUILayout.SelectionGrid(_dayPreset, _presets, 3, GUILayout.Height(36));
                if (_commandLineMinutes != null)
                {
                    GUILayout.Label("Command-line day length: " + _commandLineMinutes + " minutes");
                    if (GUILayout.Button("Use selected preset instead")) _commandLineMinutes = null;
                }
                var valid = ushort.TryParse(_port, out var port) && port > 0;
                GUI.enabled = valid;
                if (GUILayout.Button("Start Host", GUILayout.Height(40))) Connect(true);
                if (GUILayout.Button("Start Client", GUILayout.Height(40))) Connect(false);
                GUI.enabled = true;
                if (!valid) GUILayout.Label("Port must be 1-65535.");
            }
            else
            {
                if (localInput != null && GUILayout.Button("Resume controls", GUILayout.Height(40))) localInput.CaptureCursor();
                if (GUILayout.Button("Disconnect / return to start")) _session.Disconnect();
                var player = _session.Manager.LocalClient?.PlayerObject;
                if (player != null)
                {
                    var look = player.GetComponent<PlayerLook>();
                    GUILayout.Label($"Mouse sensitivity: {look.SensitivityMultiplier:F1}x");
                    look.SensitivityMultiplier = GUILayout.HorizontalSlider(look.SensitivityMultiplier, 0.2f, 3f);
                    _diagnostics = GUILayout.Toggle(_diagnostics, "Development diagnostics");
                    if (_diagnostics)
                    {
                        GUILayout.Label($"Local ID: {_session.Manager.LocalClientId}");
                        foreach (var spawned in _session.Manager.SpawnManager.SpawnedObjects.Values)
                        {
                            if (!spawned.IsPlayerObject) continue;
                            GUILayout.Label($"{spawned.OwnerClientId} | {spawned.GetComponent<NetworkPlayer>().Role} | {spawned.GetComponent<PlayerScore>().Value}");
                        }
                    }
                }
            }
            if (GUILayout.Button("Quit game")) Application.Quit();
            GUILayout.EndArea();
        }

        public static bool HasArgument(string argument) => Array.IndexOf(Environment.GetCommandLineArgs(), argument) >= 0;
        public static string Argument(string argument)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, argument);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
#endif
    }
}
