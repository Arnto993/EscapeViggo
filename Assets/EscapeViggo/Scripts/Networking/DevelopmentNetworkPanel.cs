using System;
using EscapeViggo.Player;
using UnityEngine;

namespace EscapeViggo.Networking
{
    public sealed class DevelopmentNetworkPanel : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private LocalNetworkSession _session;
        private string _address = "127.0.0.1";
        private string _port = "7777";

        private void Start()
        {
            _address = Argument("-ev-address") ?? _address;
            _port = Argument("-ev-port") ?? _port;
            if (HasArgument("-ev-host")) Connect(true);
            else if (HasArgument("-ev-client")) Connect(false);
        }

        private void Connect(bool host)
        {
            if (!ushort.TryParse(_port, out var port) || port == 0) return;
            if (host) _session.StartHost(_address, port);
            else _session.StartClient(_address, port);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12, 12, 325, 270), GUI.skin.box);
            GUILayout.Label("ESCAPE VIGGO - LOCAL DEVELOPMENT");
            GUILayout.Label(_session.Status);
            GUILayout.Label($"Players: {_session.PlayerCount}  |  Local ID: {_session.Manager.LocalClientId}");
            if (_session.CanStart)
            {
                GUILayout.Label("IP address / port");
                _address = GUILayout.TextField(_address);
                _port = GUILayout.TextField(_port);
                var valid = ushort.TryParse(_port, out var port) && port > 0;
                GUI.enabled = valid;
                if (GUILayout.Button("Start Host")) Connect(true);
                if (GUILayout.Button("Start Client")) Connect(false);
                GUI.enabled = true;
                if (!valid) GUILayout.Label("Port must be 1-65535.");
            }
            else
            {
                if (GUILayout.Button("Disconnect")) _session.Disconnect();
                GUILayout.Label("WASD move | Shift sprint | Space jump");
                GUILayout.Label("Ctrl / C crouch | Esc release mouse");
                var player = _session.Manager.LocalClient?.PlayerObject;
                if (player != null)
                {
                    GUILayout.Label($"Role: {player.GetComponent<NetworkPlayer>().Role}");
                    var look = player.GetComponent<PlayerLook>();
                    GUILayout.Label($"Mouse sensitivity: {look.SensitivityMultiplier:F1}x");
                    look.SensitivityMultiplier = GUILayout.HorizontalSlider(look.SensitivityMultiplier, 0.2f, 3f);
                }
            }
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
