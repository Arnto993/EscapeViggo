using UnityEngine;

namespace EscapeViggo.Core
{
    [RequireComponent(typeof(DailyResults))]
    public sealed class DailyResultsPanel : MonoBehaviour
    {
        private DailyResults _results;
        private NetworkWorkday _workday;
        public bool IsOpen => _results.Ready && _workday.State.Phase == MatchPhase.EndOfDay;
        private void Awake() { _results = GetComponent<DailyResults>(); _workday = GetComponent<NetworkWorkday>(); }
        private void OnGUI()
        {
            if (!IsOpen) return;
            var width = Mathf.Min(620, Screen.width - 24);
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2, 210, width, 160 + _results.Count * 25), GUI.skin.box);
            GUILayout.Label("END OF DAY - 15:00");
            GUILayout.Label("Player | Role | Score | Tasks completed | Captures");
            for (var i = 0; i < _results.Count; i++) GUILayout.Label(_results.RowAt(i).ToString());
            GUILayout.Space(10);
            GUILayout.Label($"Battery outcome: {_results.Summary.BatteryOutcome}");
            GUILayout.Label("Workday finished. Disconnect and restart the host for a new day.");
            GUILayout.EndArea();
        }
    }
}
