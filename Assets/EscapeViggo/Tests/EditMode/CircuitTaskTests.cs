using EscapeViggo.Player;
using EscapeViggo.Tasks;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class CircuitTaskTests
    {
        private readonly int[] _sequence = { 1, 3, 2 };
        private static WorkTaskState Started => new() { Phase = WorkTaskPhase.InProgress, WorkerId = 7, Revision = 1 };

        [TestCase(PlayerRole.Electrician, true)]
        [TestCase(PlayerRole.Viggo, false)]
        public void OnlyElectriciansHaveTaskAccess(PlayerRole role, bool allowed) => Assert.That(WorkTask.RoleCanWork(role), Is.EqualTo(allowed));

        [Test]
        public void CorrectSequenceCompletesExactlyOnce()
        {
            var state = Started;
            foreach (var element in _sequence)
            {
                Assert.That(CircuitSequence.TryAdvance(state, _sequence, 7, element, state.Revision, out var next), Is.True);
                state = next;
            }
            Assert.That(state.Phase, Is.EqualTo(WorkTaskPhase.Completed));
            Assert.That(state.WorkerId, Is.EqualTo(7));
            Assert.That(CircuitSequence.TryAdvance(state, _sequence, 7, 2, state.Revision, out _), Is.False);
        }

        [Test]
        public void WrongElementResetsProgressAndSignalsFeedback()
        {
            CircuitSequence.TryAdvance(Started, _sequence, 7, 1, 1, out var state);
            CircuitSequence.TryAdvance(state, _sequence, 7, 2, state.Revision, out var reset);
            Assert.That(reset.Progress, Is.Zero);
            Assert.That(reset.WrongInput, Is.True);
            Assert.That(reset.Phase, Is.EqualTo(WorkTaskPhase.InProgress));
        }

        [Test]
        public void ReplayedRevisionWrongWorkerAndInvalidButtonAreRejected()
        {
            CircuitSequence.TryAdvance(Started, _sequence, 7, 1, 1, out var state);
            Assert.That(CircuitSequence.TryAdvance(state, _sequence, 7, 3, 1, out _), Is.False);
            Assert.That(CircuitSequence.TryAdvance(state, _sequence, 8, 3, state.Revision, out _), Is.False);
            Assert.That(CircuitSequence.TryAdvance(state, _sequence, 7, 99, state.Revision, out _), Is.False);
        }

        [Test]
        public void ConfigurationSupportsAnotherOrderAndIndependentInstances()
        {
            var other = new[] { 3, 1, 2 };
            Assert.That(CircuitSequence.IsValid(other), Is.True);
            CircuitSequence.TryAdvance(Started, other, 7, 3, 1, out var first);
            Assert.That(first.Progress, Is.EqualTo(1));
            Assert.That(Started.Progress, Is.Zero);
            Assert.That(CircuitSequence.IsValid(new[] { 1, 4, 2 }), Is.False);
        }
    }
}
