using UnityEngine;

namespace EscapeViggo.Tasks
{
    [RequireComponent(typeof(ElectricalWorkTask))]
    public sealed class ElectricalTaskPanel : WorkTaskPanel
    {
        private void Action(string label, ElectricalAction action)
        {
            if (GUILayout.Button(label, GUILayout.Height(38))) ((ElectricalWorkTask)Task).SubmitActionRpc(action, Task.State.Revision);
        }
        protected override void DrawControls(WorkTaskState state)
        {
            if (((ElectricalWorkTask)Task).Kind == ElectricalTaskKind.VoltageMeasurement) DrawVoltage(state);
            else if (((ElectricalWorkTask)Task).Kind == ElectricalTaskKind.OutletReplacement) DrawOutlet(state);
            else DrawBreaker(state);
            if (state.WrongInput) GUILayout.Label("Check the work order and labelled points, then try again.");
        }
        private void DrawVoltage(WorkTaskState state)
        {
            GUILayout.Label("Work order: check the marked L-N supply. Expected: 230 V AC.");
            GUILayout.Box(state.Progress >= 3 ? "MULTIMETER     230 V ~" : "MULTIMETER     --.- V", GUILayout.Height(58));
            switch (state.Progress)
            {
                case 0:
                    GUILayout.Label("Turn the dial to measure alternating voltage.");
                    GUILayout.BeginHorizontal();
                    Action("V ~ AC voltage", ElectricalAction.AcVoltage); Action("V = DC voltage", ElectricalAction.DcVoltage); Action("Ohms resistance", ElectricalAction.Resistance);
                    GUILayout.EndHorizontal(); break;
                case 1:
                    GUILayout.Label("Place the BLACK probe on the marked N reference point.");
                    GUILayout.BeginHorizontal();
                    Action("N / blue", ElectricalAction.BlackNeutral); Action("L / brown", ElectricalAction.BlackLive); Action("PE / earth", ElectricalAction.BlackEarth);
                    GUILayout.EndHorizontal(); break;
                case 2:
                    GUILayout.Label("BLACK -> N. Place the RED probe on the marked L point.");
                    GUILayout.BeginHorizontal();
                    Action("N / blue", ElectricalAction.RedNeutral); Action("L / brown", ElectricalAction.RedLive); Action("PE / earth", ElectricalAction.RedEarth);
                    GUILayout.EndHorizontal(); break;
                case 3:
                    GUILayout.Label("BLACK -> N     RED -> L. Compare the reading to the work order.");
                    Action("230 V: matches the work order", ElectricalAction.Reading230);
                    Action("0 V: no supply", ElectricalAction.ReadingZero); Action("24 V: low voltage", ElectricalAction.ReadingLow); break;
            }
        }
        private void DrawOutlet(WorkTaskState state)
        {
            GUILayout.Label("Training outlet - supply isolated. Replace the old outlet.");
            GUILayout.Label("BROWN -> L     BLUE -> N     GREEN/YELLOW -> PE");
            switch (state.Progress)
            {
                case 0: Action("Unscrew and remove faceplate", ElectricalAction.RemoveFaceplate); break;
                case 1:
                    GUILayout.Label("Disconnect each old conductor (any order)."); WireButtons(state, true); break;
                case 2:
                    GUILayout.Label("Select a wire, then its matching terminal (any order).");
                    WireButtons(state, false);
                    GUILayout.Label(state.Selection == 0 ? "No wire selected" : "Selected: " + new[] { "Brown", "Blue", "Green/yellow" }[state.Selection - 1]);
                    GUILayout.BeginHorizontal();
                    Action("L terminal", ElectricalAction.TerminalLive); Action("N terminal", ElectricalAction.TerminalNeutral); Action("PE terminal", ElectricalAction.TerminalEarth);
                    GUILayout.EndHorizontal(); break;
                case 3: Action("Fit the new outlet into the wall box", ElectricalAction.InstallOutlet); break;
                case 4: Action("Replace and fasten faceplate", ElectricalAction.ReplaceFaceplate); break;
            }
        }
        private void WireButtons(WorkTaskState state, bool disconnect)
        {
            var labels = new[] { "Brown", "Blue", "Green/yellow" };
            var colors = new[] { new Color(0.7f, 0.4f, 0.15f), new Color(0.3f, 0.65f, 1), new Color(0.75f, 1, 0.2f) };
            GUILayout.BeginHorizontal();
            var original = GUI.backgroundColor;
            for (var wire = 0; wire < 3; wire++)
            {
                GUI.backgroundColor = colors[wire];
                GUI.enabled = (state.Connections & (1 << wire)) == 0;
                Action(labels[wire] + (GUI.enabled ? "" : " - done"), (ElectricalAction)((int)(disconnect ? ElectricalAction.DisconnectBrown : ElectricalAction.SelectBrown) + wire));
            }
            GUI.backgroundColor = original; GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
        private void DrawBreaker(WorkTaskState state)
        {
            GUILayout.Label("Work order: install a 16 A breaker in SLOT 2.");
            GUILayout.Box(state.Progress < 2 ? "[ SLOT 1 ]   [ SLOT 2: EMPTY ]   [ SLOT 3 ]" : "[ SLOT 1 ]   [ SLOT 2: 16 A ]   [ SLOT 3 ]", GUILayout.Height(55));
            switch (state.Progress)
            {
                case 0:
                    GUILayout.BeginHorizontal(); Action("10 A", ElectricalAction.Breaker10); Action("16 A", ElectricalAction.Breaker16); Action("32 A", ElectricalAction.Breaker32); GUILayout.EndHorizontal(); break;
                case 1:
                    GUILayout.BeginHorizontal(); Action("Slot 1", ElectricalAction.Slot1); Action("Slot 2", ElectricalAction.Slot2); Action("Slot 3", ElectricalAction.Slot3); GUILayout.EndHorizontal(); break;
                case 2:
                    GUILayout.Label("Supply isolated. Attach the two labelled conductors.");
                    GUI.enabled = (state.Connections & 1) == 0; Action("Supply -> IN", ElectricalAction.ConnectSupply);
                    GUI.enabled = (state.Connections & 2) == 0; Action("Load -> OUT", ElectricalAction.ConnectLoad);
                    GUI.enabled = true; break;
                case 3: Action("Switch breaker ON", ElectricalAction.ToggleOn); break;
                case 4: Action("Indicator green - confirm installation", ElectricalAction.ConfirmBreaker); break;
            }
        }
    }
}
