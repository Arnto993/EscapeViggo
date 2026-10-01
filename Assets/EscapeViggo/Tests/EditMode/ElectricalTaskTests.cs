using EscapeViggo.Tasks;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class ElectricalTaskTests
    {
        private static WorkTaskState Start => new() { Phase = WorkTaskPhase.InProgress, WorkerId = 7, Revision = 10 };
        private static WorkTaskState Apply(ElectricalTaskKind kind, WorkTaskState state, ElectricalAction action)
        {
            Assert.That(ElectricalTaskRules.TryAdvance(kind, state, 7, action, state.Revision, out var next), Is.True);
            return next;
        }
        [Test]
        public void VoltageRequiresCorrectModeProbesAndDiagnosis()
        {
            var state = Apply(ElectricalTaskKind.VoltageMeasurement, Start, ElectricalAction.DcVoltage);
            Assert.That(state.Progress, Is.Zero); Assert.That(state.WrongInput, Is.True);
            foreach (var action in new[] { ElectricalAction.AcVoltage, ElectricalAction.BlackNeutral, ElectricalAction.RedLive }) state = Apply(ElectricalTaskKind.VoltageMeasurement, state, action);
            state = Apply(ElectricalTaskKind.VoltageMeasurement, state, ElectricalAction.ReadingZero);
            Assert.That(state.Phase, Is.EqualTo(WorkTaskPhase.InProgress));
            state = Apply(ElectricalTaskKind.VoltageMeasurement, state, ElectricalAction.Reading230);
            Assert.That(state.Phase, Is.EqualTo(WorkTaskPhase.Completed));
        }
        [Test]
        public void OutletAcceptsAnyWireOrderButRejectsWrongTerminalAndPrematureInstallation()
        {
            var kind = ElectricalTaskKind.OutletReplacement;
            var state = Apply(kind, Start, ElectricalAction.RemoveFaceplate);
            state = Apply(kind, state, ElectricalAction.InstallOutlet);
            Assert.That(state.Progress, Is.EqualTo(1));
            foreach (var action in new[] { ElectricalAction.DisconnectEarth, ElectricalAction.DisconnectBlue, ElectricalAction.DisconnectBrown }) state = Apply(kind, state, action);
            state = Apply(kind, state, ElectricalAction.SelectEarth);
            state = Apply(kind, state, ElectricalAction.TerminalLive);
            Assert.That(state.Connections, Is.Zero); Assert.That(state.WrongInput, Is.True);
            foreach (var action in new[] { ElectricalAction.TerminalEarth, ElectricalAction.SelectBlue, ElectricalAction.TerminalNeutral, ElectricalAction.SelectBrown, ElectricalAction.TerminalLive,
                ElectricalAction.InstallOutlet, ElectricalAction.ReplaceFaceplate }) state = Apply(kind, state, action);
            Assert.That(state.Phase, Is.EqualTo(WorkTaskPhase.Completed));
        }
        [TestCase(ElectricalTaskKind.VoltageMeasurement)]
        [TestCase(ElectricalTaskKind.OutletReplacement)]
        [TestCase(ElectricalTaskKind.BreakerInstallation)]
        public void RejectsOtherWorkerOldRevisionInvalidInputAndCompletedTask(ElectricalTaskKind kind)
        {
            Assert.That(ElectricalTaskRules.TryAdvance(kind, Start, 8, ElectricalAction.AcVoltage, 10, out _), Is.False);
            Assert.That(ElectricalTaskRules.TryAdvance(kind, Start, 7, ElectricalAction.AcVoltage, 9, out _), Is.False);
            Assert.That(ElectricalTaskRules.TryAdvance(kind, Start, 7, (ElectricalAction)255, 10, out _), Is.False);
            var completed = Start; completed.Phase = WorkTaskPhase.Completed;
            Assert.That(ElectricalTaskRules.TryAdvance(kind, completed, 7, ElectricalAction.AcVoltage, 10, out _), Is.False);
        }
        [Test]
        public void LayoutVariesByDayAndWrapsWithoutDaySix()
        {
            var layout = new[] { ElectricalTaskKind.VoltageMeasurement, ElectricalTaskKind.OutletReplacement };
            Assert.That(TaskPlacement.ForDay(layout, 1), Is.EqualTo(layout[0]));
            Assert.That(TaskPlacement.ForDay(layout, 2), Is.EqualTo(layout[1]));
            Assert.That(TaskPlacement.ForDay(layout, 5), Is.EqualTo(layout[0]));
            Assert.Throws<System.ArgumentException>(() => TaskPlacement.ForDay(layout, 6));
        }
        [Test]
        public void BreakerRequiresCorrectRatingSlotBothConnectionsAndConfirmation()
        {
            var kind = ElectricalTaskKind.BreakerInstallation;
            var state = Apply(kind, Start, ElectricalAction.Breaker32);
            Assert.That(state.Progress, Is.Zero);
            state = Apply(kind, state, ElectricalAction.Breaker16);
            state = Apply(kind, state, ElectricalAction.Slot1);
            Assert.That(state.Progress, Is.EqualTo(1));
            foreach (var action in new[] { ElectricalAction.Slot2, ElectricalAction.ConnectLoad, ElectricalAction.ConnectSupply, ElectricalAction.ToggleOn }) state = Apply(kind, state, action);
            Assert.That(state.Phase, Is.EqualTo(WorkTaskPhase.InProgress));
            state = Apply(kind, state, ElectricalAction.ConfirmBreaker);
            Assert.That(state.Phase, Is.EqualTo(WorkTaskPhase.Completed));
        }
    }
}
