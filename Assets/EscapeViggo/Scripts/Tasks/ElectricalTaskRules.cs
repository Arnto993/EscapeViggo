using System;

namespace EscapeViggo.Tasks
{
    public enum ElectricalTaskKind : byte { VoltageMeasurement, OutletReplacement, BreakerInstallation }
    public enum ElectricalAction : byte
    {
        AcVoltage, DcVoltage, Resistance,
        BlackNeutral, BlackLive, BlackEarth, RedLive, RedNeutral, RedEarth,
        Reading230, ReadingZero, ReadingLow,
        RemoveFaceplate, DisconnectBrown, DisconnectBlue, DisconnectEarth,
        SelectBrown, SelectBlue, SelectEarth, TerminalLive, TerminalNeutral, TerminalEarth,
        InstallOutlet, ReplaceFaceplate,
        Breaker10, Breaker16, Breaker32, Slot1, Slot2, Slot3, ConnectSupply, ConnectLoad, ToggleOn, ConfirmBreaker
    }

    public static class TaskPlacement
    {
        public static ElectricalTaskKind ForDay(ElectricalTaskKind[] layout, int day)
        {
            if (layout == null || layout.Length == 0 || day < 1 || day > 5) throw new ArgumentException("Task placement needs a layout and weekday 1–5.");
            return layout[(day - 1) % layout.Length];
        }
    }

    public static class ElectricalTaskRules
    {
        public static bool TryAdvance(ElectricalTaskKind kind, WorkTaskState current, ulong sender,
            ElectricalAction action, uint revision, out WorkTaskState next)
        {
            next = current;
            if (current.Phase != WorkTaskPhase.InProgress || current.WorkerId != sender || current.Revision != revision ||
                !Enum.IsDefined(typeof(ElectricalAction), action)) return false;
            next.WrongInput = false;
            var accepted = kind switch
            {
                ElectricalTaskKind.VoltageMeasurement => Voltage(ref next, action),
                ElectricalTaskKind.OutletReplacement => Outlet(ref next, action),
                ElectricalTaskKind.BreakerInstallation => Breaker(ref next, action),
                _ => false
            };
            if (!accepted) return false;
            next.Revision++;
            return true;
        }
        private static bool Step(ref WorkTaskState state, ElectricalAction actual, ElectricalAction expected, int finalStep)
        {
            state.WrongInput = actual != expected;
            if (!state.WrongInput && ++state.Progress == finalStep) state.Phase = WorkTaskPhase.Completed;
            return true;
        }
        private static bool Voltage(ref WorkTaskState state, ElectricalAction action) => state.Progress switch
        {
            0 => Step(ref state, action, ElectricalAction.AcVoltage, 4),
            1 => Step(ref state, action, ElectricalAction.BlackNeutral, 4),
            2 => Step(ref state, action, ElectricalAction.RedLive, 4),
            3 => Step(ref state, action, ElectricalAction.Reading230, 4),
            _ => false
        };
        private static bool Outlet(ref WorkTaskState state, ElectricalAction action)
        {
            switch (state.Progress)
            {
                case 0: return Step(ref state, action, ElectricalAction.RemoveFaceplate, 5);
                case 1:
                    var wire = (int)action - (int)ElectricalAction.DisconnectBrown;
                    if (wire < 0 || wire > 2) { state.WrongInput = true; return true; }
                    var bit = 1 << wire;
                    if ((state.Connections & bit) != 0) return false;
                    state.Connections |= bit;
                    if (state.Connections == 7) { state.Progress++; state.Connections = 0; }
                    return true;
                case 2:
                    var selected = (int)action - (int)ElectricalAction.SelectBrown;
                    if (selected >= 0 && selected <= 2)
                    {
                        if ((state.Connections & (1 << selected)) != 0) return false;
                        state.Selection = selected + 1;
                        return true;
                    }
                    var terminal = (int)action - (int)ElectricalAction.TerminalLive;
                    if (terminal < 0 || terminal > 2 || state.Selection == 0) { state.WrongInput = true; return true; }
                    state.WrongInput = terminal + 1 != state.Selection;
                    if (!state.WrongInput)
                    {
                        state.Connections |= 1 << terminal;
                        state.Selection = 0;
                        if (state.Connections == 7) { state.Progress++; state.Connections = 0; }
                    }
                    return true;
                case 3: return Step(ref state, action, ElectricalAction.InstallOutlet, 5);
                case 4: return Step(ref state, action, ElectricalAction.ReplaceFaceplate, 5);
                default: return false;
            }
        }
        private static bool Breaker(ref WorkTaskState state, ElectricalAction action)
        {
            switch (state.Progress)
            {
                case 0: return Step(ref state, action, ElectricalAction.Breaker16, 5);
                case 1: return Step(ref state, action, ElectricalAction.Slot2, 5);
                case 2:
                    var bit = action == ElectricalAction.ConnectSupply ? 1 : action == ElectricalAction.ConnectLoad ? 2 : 0;
                    if (bit == 0) { state.WrongInput = true; return true; }
                    if ((state.Connections & bit) != 0) return false;
                    state.Connections |= bit;
                    if (state.Connections == 3) { state.Progress++; state.Connections = 0; }
                    return true;
                case 3: return Step(ref state, action, ElectricalAction.ToggleOn, 5);
                case 4: return Step(ref state, action, ElectricalAction.ConfirmBreaker, 5);
                default: return false;
            }
        }
    }
}
