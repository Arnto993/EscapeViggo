using EscapeViggo.Core;
using EscapeViggo.Player;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class CaptureTests
    {
        [TestCase(PlayerRole.Viggo, PlayerRole.Electrician, true)]
        [TestCase(PlayerRole.Electrician, PlayerRole.Electrician, false)]
        [TestCase(PlayerRole.Electrician, PlayerRole.Viggo, false)]
        [TestCase(PlayerRole.Viggo, PlayerRole.Viggo, false)]
        public void RolesAreValidated(PlayerRole actor, PlayerRole target, bool expected) =>
            Assert.That(CaptureRules.CanCapture(actor, target, MatchPhase.Workday, false, default, 10, 1, 2), Is.EqualTo(expected));

        [TestCase(2, true)] [TestCase(2.01f, false)] [TestCase(float.NaN, false)]
        public void DistanceIsValidated(float distance, bool expected) =>
            Assert.That(CaptureRules.CanCapture(PlayerRole.Viggo, PlayerRole.Electrician, MatchPhase.Workday, false,
                default, 10, distance, 2), Is.EqualTo(expected));

        [Test] public void RestEndOfDayDuplicateAndRespawnProtectionRejectCapture()
        {
            Assert.That(CaptureRules.CanCapture(PlayerRole.Viggo, PlayerRole.Electrician, MatchPhase.Workday, true, default, 10, 1, 2), Is.False);
            Assert.That(CaptureRules.CanCapture(PlayerRole.Viggo, PlayerRole.Electrician, MatchPhase.EndOfDay, false, default, 10, 1, 2), Is.False);
            Assert.That(CaptureRules.CanCapture(PlayerRole.Viggo, PlayerRole.Electrician, MatchPhase.Workday, false,
                new CaptureState { Phase = CapturePhase.Captured }, 10, 1, 2), Is.False);
            Assert.That(CaptureRules.CanCapture(PlayerRole.Viggo, PlayerRole.Electrician, MatchPhase.Workday, false,
                new CaptureState { ProtectedUntil = 11 }, 10, 1, 2), Is.False);
        }
    }
}
