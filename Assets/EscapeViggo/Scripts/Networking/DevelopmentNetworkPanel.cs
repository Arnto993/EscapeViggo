using System;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using UnityEngine;

namespace EscapeViggo.Networking
{
    public sealed class DevelopmentNetworkPanel : MonoBehaviour
    {
        [SerializeField] private LocalNetworkSession _session;
        private string _joinCode = "";
        private bool _localDevelopment;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private string _address = "127.0.0.1";
        private string _port = "7777";
        private bool _diagnostics;
        private int _dayPreset = 1;
        private string _commandLineMinutes;
        private readonly string[] _presets = { "Quick: 2 min", "Guided: 5 min", "Asset tuning" };
#endif

        private void Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _address = Argument("-ev-address") ?? _address;
            _port = Argument("-ev-port") ?? _port;
            _commandLineMinutes = Argument("-ev-day-minutes");
            if (HasArgument("-ev-online"))
            {
                _ = ConnectOnlineFromArguments();
                return;
            }
            if (HasArgument("-ev-connection-smoke")) return;
            if (HasArgument("-ev-host")) Connect(true);
            else if (HasArgument("-ev-client")) Connect(false);
#endif
        }

        private void Connect(bool host)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _localDevelopment = true;
            if (!ushort.TryParse(_port, out var port) || port == 0) return;
            if (host)
            {
                var minutes = _commandLineMinutes ?? (_dayPreset == 0 ? "2" : _dayPreset == 1 ? "5" : null);
                if (!_session.ConfigurePlaytestDay(minutes)) return;
                _session.StartHost(_address, port);
            }
            else _session.StartClient(_address, port);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private async System.Threading.Tasks.Task ConnectOnlineFromArguments()
        {
            if (HasArgument("-ev-host"))
            {
                if (!_session.ConfigurePlaytestDay(_commandLineMinutes ?? "5")) return;
                await _session.Online.HostAsync();
                var codeFile = Argument("-ev-online-code-file");
                if (!string.IsNullOrEmpty(codeFile) && !string.IsNullOrEmpty(_session.Online.JoinCode))
                    System.IO.File.WriteAllText(codeFile, _session.Online.JoinCode);
            }
            else await _session.Online.JoinAsync(Argument("-ev-join-code"));
            Debug.Log("EV_ONLINE " + _session.Online.Status);
        }
#endif

        private void HostOnline()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var minutes = _commandLineMinutes ?? (_dayPreset == 0 ? "2" : _dayPreset == 1 ? "5" : null);
            if (!_session.ConfigurePlaytestDay(minutes)) return;
#endif
            _ = _session.Online.HostAsync();
        }

        private void OnGUI()
        {
            var localInput = _session.Manager.LocalClient?.PlayerObject?.GetComponent<PlayerInputReader>();
            if (_session.Online != null && !string.IsNullOrEmpty(_session.Online.JoinCode) && _session.Manager.IsHost)
            {
                using var codeGui = new Core.PrototypeGui(true);
                GUI.Label(new Rect(Core.PrototypeGui.Width - 285, 12, 275, 30), "Join Code: " + _session.Online.JoinCode + "  (Esc to copy)");
            }
            if (localInput != null && (localInput.IsCaptured || localInput.IsTaskModal ||
                (_session.Workday != null && _session.Workday.State.Phase == Core.MatchPhase.EndOfDay))) return;
            using var gui = new Core.PrototypeGui(true);
            var connected = localInput != null;
            var panel = connected ? new Rect(Core.PrototypeGui.Width - 400, 180, 380, Core.PrototypeGui.Height - 200)
                : new Rect(Core.PrototypeGui.Width / 2 - 310, 60, 620, Core.PrototypeGui.Height - 120);
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label("ESCAPE VIGGO - PLAY");
            GUILayout.Label(_localDevelopment || _session.Online == null ? _session.Status : _session.Online.Status);
            GUILayout.Label($"Players: {_session.PlayerCount} / {_session.Capacity}");
            if (_session.Online != null && !string.IsNullOrEmpty(_session.Online.JoinCode))
            {
                GUILayout.Label("Join Code: " + _session.Online.JoinCode);
                if (GUILayout.Button("Copy join code")) GUIUtility.systemCopyBuffer = _session.Online.JoinCode;
            }
            if (!connected)
            {
                GUILayout.Label("Host plays Viggo. Joining players are electricians.");
                GUILayout.Label("The workday starts automatically after five seconds.");
                GUILayout.Space(12);
            }
            if (_session.CanStart)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GUILayout.Label("Host workday length (clients use the host's clock)");
                _dayPreset = GUILayout.SelectionGrid(_dayPreset, _presets, 3, GUILayout.Height(36));
                if (_commandLineMinutes != null)
                {
                    GUILayout.Label("Command-line day length: " + _commandLineMinutes + " minutes");
                    if (GUILayout.Button("Use selected preset instead")) _commandLineMinutes = null;
                }
#endif
                if (!_localDevelopment && _session.Online != null)
                {
                    if (GUILayout.Button("Host Online Game", GUILayout.Height(40))) HostOnline();
                    GUILayout.Label("Join Online Game - six-character code");
                    _joinCode = GUILayout.TextField(_joinCode, 32);
                    if (GUILayout.Button("Join Online Game", GUILayout.Height(40))) _ = _session.Online.JoinAsync(_joinCode);
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _localDevelopment = GUILayout.Toggle(_localDevelopment, "Local Development (direct IP, no cloud services)");
                if (_localDevelopment)
                {
                GUILayout.Label("IP address / port (same PC: 127.0.0.1:7777)");
                _address = GUILayout.TextField(_address);
                _port = GUILayout.TextField(_port);
                var valid = ushort.TryParse(_port, out var port) && port > 0;
                GUI.enabled = valid;
                if (GUILayout.Button("Start Host", GUILayout.Height(40))) Connect(true);
                if (GUILayout.Button("Start Client", GUILayout.Height(40))) Connect(false);
                GUI.enabled = true;
                if (!valid) GUILayout.Label("Port must be 1-65535.");
                }
#endif
            }
            else
            {
                if (localInput != null && GUILayout.Button("Resume controls", GUILayout.Height(40))) localInput.CaptureCursor();
                if (_session.Online == null || _session.Online.Connection.Phase != OnlineConnectionPhase.Leaving)
                {
                    if (GUILayout.Button("Disconnect / return to start")) _session.Disconnect();
                }
                else GUILayout.Label("Cleaning up the previous request...");
                var player = _session.Manager.LocalClient?.PlayerObject;
                if (player != null)
                {
                    var look = player.GetComponent<PlayerLook>();
                    GUILayout.Label($"Mouse sensitivity: {look.SensitivityMultiplier:F1}x");
                    look.SensitivityMultiplier = GUILayout.HorizontalSlider(look.SensitivityMultiplier, 0.2f, 3f);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
#endif
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
    }
}
