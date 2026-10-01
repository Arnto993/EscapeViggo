using EscapeViggo.Player;
using UnityEngine;

namespace EscapeViggo.Tasks
{
    [RequireComponent(typeof(WorkTask))]
    public abstract class WorkTaskPanel : MonoBehaviour
    {
        [SerializeField] private TextMesh _label;
        [SerializeField] private Renderer _panel;
        protected WorkTask Task;
        private PlayerInputReader _localInput;
        private bool _open;
        public bool IsOpen => _open;

        private void Awake() => Task = GetComponent<WorkTask>();
        private void Update() { if (_open && (!Task.IsSpawned || !Task.IsWorkday)) Close(); }
        private void OnEnable() => Task.StateChanged += Present;
        private void OnDisable()
        {
            Task.StateChanged -= Present;
            Close();
        }

        private void Present(WorkTaskState state)
        {
            if (_label != null) _label.text = Task.Title + "\n" + (state.Phase == WorkTaskPhase.Completed
                ? $"Completed by {state.WorkerId}" : state.Phase.ToString());
            if (_panel != null)
            {
                var tint = new MaterialPropertyBlock();
                tint.SetColor("_BaseColor", state.Phase == WorkTaskPhase.Completed ? Color.green : new Color(1, 0.7f, 0.12f));
                _panel.SetPropertyBlock(tint);
            }
            var manager = Task.NetworkManager;
            var activeHere = Task.IsSpawned && manager.IsClient && state.Phase == WorkTaskPhase.InProgress && state.WorkerId == manager.LocalClientId;
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

        protected abstract void DrawControls(WorkTaskState state);

        private void OnGUI()
        {
            if (!_open || !Task.IsSpawned) return;
            using var gui = new Core.PrototypeGui(true);
            var state = Task.State;
            GUILayout.BeginArea(new Rect(Core.PrototypeGui.Width / 2 - 320, 180, 640, 420), GUI.skin.box);
            GUILayout.Label(Task.Title);
            DrawControls(state);
            if (GUILayout.Button("Cancel")) Task.CancelRpc(state.Revision);
            GUILayout.EndArea();
        }
    }
}

