using System;
using EscapeViggo.Core;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class WorkdayClockTests
    {
        [TestCase(0, MatchPhase.Preparing, 420)]
        [TestCase(4.99, MatchPhase.Preparing, 420)]
        [TestCase(5, MatchPhase.Workday, 420)]
        [TestCase(65, MatchPhase.Workday, 660)]
        [TestCase(125, MatchPhase.EndOfDay, 900)]
        [TestCase(10000, MatchPhase.EndOfDay, 900)]
        public void TransitionsAndTimeHaveFixedBoundaries(double elapsed, MatchPhase phase, int minute)
        {
            var state = WorkdayClock.Evaluate(elapsed, 5, 2);
            Assert.That(state.Phase, Is.EqualTo(phase));
            Assert.That(state.MinuteOfDay, Is.EqualTo(minute));
        }

        [TestCase(0.5)]
        [TestCase(5)]
        [TestCase(15)]
        public void DurationIsConfigurable(double minutes)
        {
            Assert.That(WorkdayClock.Evaluate(minutes * 30, 0, minutes).ClockText, Is.EqualTo("11:00"));
            Assert.That(WorkdayClock.Evaluate(minutes * 60, 0, minutes).ClockText, Is.EqualTo("15:00"));
        }

        [Test]
        public void InvalidDurationFailsClearly() => Assert.Throws<ArgumentOutOfRangeException>(() => WorkdayClock.Evaluate(1, 0, 0));
    }
}
