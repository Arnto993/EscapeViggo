using UnityEngine;

namespace EscapeViggo.Tasks
{
    [RequireComponent(typeof(SimpleCircuitTask))]
    public sealed class SimpleCircuitTaskPanel : WorkTaskPanel
    {
        protected override void DrawControls(WorkTaskState state)
        {
            var circuit = (SimpleCircuitTask)Task;
            GUILayout.Label("Activate in order: " + circuit.SequenceText);
            GUILayout.Label($"Connected: {state.Progress} / 3");
            GUILayout.Label(state.WrongInput ? "Wrong element. Progress reset; try again." : "Select the next element.");
            GUILayout.BeginHorizontal();
            for (var element = 1; element <= 3; element++)
                if (GUILayout.Button(element.ToString(), GUILayout.Height(50))) circuit.SubmitElementRpc(element, state.Revision);
            GUILayout.EndHorizontal();
        }
    }
}
