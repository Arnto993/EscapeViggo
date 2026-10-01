using EscapeViggo.Core;
using EscapeViggo.Player;
using EscapeViggo.Viggo;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class DistractionTests
    {
        [TestCase(PlayerRole.Electrician, MatchPhase.Workday, false, true, 10, 2, false)]
        [TestCase(PlayerRole.Viggo, MatchPhase.EndOfDay, false, true, 10, 2, false)]
        [TestCase(PlayerRole.Viggo, MatchPhase.Workday, true, true, 10, 2, false)]
        [TestCase(PlayerRole.Viggo, MatchPhase.Workday, false, false, 10, 2, false)]
        [TestCase(PlayerRole.Viggo, MatchPhase.Workday, false, true, 9, 2, false)]
        [TestCase(PlayerRole.Viggo, MatchPhase.Workday, false, true, 10, 3, false)]
        [TestCase(PlayerRole.Viggo, MatchPhase.Workday, false, true, 10, 2, true)]
        public void InterruptRequiresMeaningfulAllowedWork(PlayerRole role, MatchPhase phase, bool resting, bool progress, double now, float distance, bool allowed)
            => Assert.That(DistractionRules.CanInterrupt(role, phase, resting, progress, now, 10, distance, 2.5f), Is.EqualTo(allowed));
        [Test]
        public void InvalidDistanceCannotInterrupt() => Assert.That(DistractionRules.CanInterrupt(PlayerRole.Viggo, MatchPhase.Workday, false, true, 10, 0, float.NaN, 3), Is.False);
        [Test]
        public void RestorationRequiresWorkerAndSafetyDelay()
        {
            var state = new DistractionState { PowerUntil = 16, RestoreAt = 12, PowerReadyAt = 30 };
            Assert.That(DistractionRules.CanRestore(PlayerRole.Electrician, MatchPhase.Workday, 11, state), Is.False);
            Assert.That(DistractionRules.CanRestore(PlayerRole.Viggo, MatchPhase.Workday, 12, state), Is.False);
            Assert.That(DistractionRules.CanRestore(PlayerRole.Electrician, MatchPhase.EndOfDay, 12, state), Is.False);
            Assert.That(DistractionRules.CanRestore(PlayerRole.Electrician, MatchPhase.Workday, 12, state), Is.True);
            Assert.That(DistractionRules.CanRestore(PlayerRole.Electrician, MatchPhase.Workday, 16, state), Is.False);
            state = DistractionRules.StopPower(state);
            Assert.That(DistractionRules.CanCut(PlayerRole.Viggo, MatchPhase.Workday, false, 16, state), Is.False);
            Assert.That(DistractionRules.CanCut(PlayerRole.Viggo, MatchPhase.Workday, false, 30, state), Is.True);
            Assert.That(DistractionRules.CanCut(PlayerRole.Electrician, MatchPhase.Workday, false, 30, state), Is.False);
            Assert.That(DistractionRules.CanCut(PlayerRole.Viggo, MatchPhase.Workday, true, 30, state), Is.False);
        }
        [Test]
        public void NewDayClearsOutageCooldownsButKeepsEventIdentity()
        {
            var next = DistractionRules.NewDay(new DistractionState { Sequence = 9, PowerUntil = 100, RestoreAt = 90, PowerReadyAt = 110, InterruptReadyAt = 80, Meaningful = true });
            Assert.That(next.Sequence, Is.EqualTo(9));
            Assert.That(next.PowerUntil + next.RestoreAt + next.PowerReadyAt + next.InterruptReadyAt, Is.Zero);
            Assert.That(next.Meaningful, Is.False);
            Assert.That(next.Kind, Is.EqualTo(DistractionKind.None));
        }
    }
}
