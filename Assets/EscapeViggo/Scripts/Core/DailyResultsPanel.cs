using UnityEngine;

namespace EscapeViggo.Core
{
    [RequireComponent(typeof(DailyResults))]
    public sealed class DailyResultsPanel : MonoBehaviour
    {
        private DailyResults _results;
        private NetworkWorkday _workday;
        private Vector2 _scroll;
        public bool IsOpen => _results.Ready && _workday.State.Phase == MatchPhase.EndOfDay;
        private void Awake() { _results = GetComponent<DailyResults>(); _workday = GetComponent<NetworkWorkday>(); }
        private void OnGUI()
        {
            if (!IsOpen) return;
            using var gui = new PrototypeGui(true);
            GUILayout.BeginArea(new Rect((PrototypeGui.Width - 860) / 2, 110, 860, PrototypeGui.Height - 210), GUI.skin.box);
            GUILayout.Label("WORKDAY COMPLETE - 15:00");
            GUILayout.Label("Final results for players present at the end of the workday.");
            DrawRow("Player", "Role", "Score", "Tasks", "Captures");
            _scroll = GUILayout.BeginScrollView(_scroll);
            for (var i = 0; i < _results.Count; i++)
            {
                var row = _results.RowAt(i);
                DrawRow($"Player {row.PlayerId}", row.Role.ToString(), row.Score.ToString(), row.TasksCompleted.ToString(), row.Captures.ToString());
            }
            GUILayout.EndScrollView();
            GUILayout.Space(10);
            GUILayout.Label("Battery: " + (_results.Summary.BatteryOutcome == Battery.BatteryPhase.Secured ? "Recovered by Viggo and secured at the station" :
                _results.Summary.BatteryOutcome == Battery.BatteryPhase.Carried ? "Kept away from Viggo until 15:00" : "At the drill station"));
            GUILayout.Label("Workday finished. Disconnect and restart the host for a new day.");
            GUILayout.EndArea();
            if (GUI.Button(new Rect(PrototypeGui.Width / 2 - 160, PrototypeGui.Height - 85, 320, 40), "Disconnect / return to start"))
                _workday.NetworkManager.GetComponent<Networking.LocalNetworkSession>().Disconnect();
        }

        private static void DrawRow(string player, string role, string score, string tasks, string captures)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(player, GUILayout.Width(160));
            GUILayout.Label(role, GUILayout.Width(220));
            GUILayout.Label(score, GUILayout.Width(140));
            GUILayout.Label(tasks, GUILayout.Width(140));
            GUILayout.Label(captures, GUILayout.Width(140));
            GUILayout.EndHorizontal();
        }
    }
}
