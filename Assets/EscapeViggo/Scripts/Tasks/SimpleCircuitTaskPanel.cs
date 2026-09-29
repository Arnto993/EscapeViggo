using EscapeViggo.Player;
using UnityEngine;

namespace EscapeViggo.Tasks
{
    [RequireComponent(typeof(SimpleCircuitTask))]
    public sealed class SimpleCircuitTaskPanel : MonoBehaviour
    {
        [SerializeField] private TextMesh _label;
        [SerializeField] private Renderer _panel;
        private SimpleCircuitTask _task;
        private PlayerInputReader _localInput;
        private bool _open;
        public bool IsOpen => _open;

        private void Awake() => _task = GetComponent<SimpleCircuitTask>();
        private void Update() { if (_open && (!_task.IsSpawned || !_task.IsWorkday)) Close(); }
        private void OnEnable() => _task.StateChanged += Present;
        private void OnDisable()
        {
            _task.StateChanged -= Present;
            Close();
        }

        private void Present(WorkTaskState state)
        {
            if (_label != null) _label.text = "SIMPLE CIRCUIT\n" + (state.Phase == WorkTaskPhase.Completed
                ? $"Completed by {state.WorkerId}" : state.Phase.ToString());
            if (_panel != null)
            {
                var tint = new MaterialPropertyBlock();
                tint.SetColor("_BaseColor", state.Phase == WorkTaskPhase.Completed ? Color.green : new Color(1, 0.7f, 0.12f));
                _panel.SetPropertyBlock(tint);
            }
            var manager = _task.NetworkManager;
            var activeHere = _task.IsSpawned && manager.IsClient && state.Phase == WorkTaskPhase.InProgress && state.WorkerId == manager.LocalClientId;
            if (!activeHere) { Close(); return; }
            if (_open) return;
            var player = manager.LocalClient?.PlayerObject;
            if (player == null) return;
            _localInput = player.GetComponent<PlayerInputReader>();
            _localInput.SetTaskModal(true);
            _open = true;
        }

        private void Close()
        {
            if (!_open) return;
            _open = false;
            if (_localInput != null) _localInput.SetTaskModal(false);
            _localInput = null;
        }

        private void OnGUI()
        {
            if (!_open || !_task.IsSpawned) return;
            var state = _task.State;
            GUILayout.BeginArea(new Rect(Screen.width / 2f - 200, Screen.height / 2f - 125, 400, 250), GUI.skin.box);
            GUILayout.Label("SIMPLE CIRCUIT TASK");
            GUILayout.Label("Activate in order: " + _task.SequenceText);
            GUILayout.Label($"Connected: {state.Progress} / 3");
            GUILayout.Label(state.WrongInput ? "Wrong element. Progress reset; try again." : "Select the next element.");
            GUILayout.BeginHorizontal();
            for (var element = 1; element <= 3; element++)
                if (GUILayout.Button(element.ToString(), GUILayout.Height(50))) _task.SubmitElementRpc(element, state.Revision);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Cancel")) _task.CancelRpc(state.Revision);
            GUILayout.EndArea();
        }
    }
}
