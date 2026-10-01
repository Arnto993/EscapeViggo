using EscapeViggo.Core;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class WeekTests
    {
        [Test]
        public void FiveDaysRequireFrozenResultsAndStopAtFriday()
        {
            for (var day = 1; day <= 5; day++)
            {
                Assert.That(WeekRules.CanAdvance(day, MatchPhase.Workday, true), Is.False);
                Assert.That(WeekRules.CanAdvance(day, MatchPhase.EndOfDay, false), Is.False);
                Assert.That(WeekRules.CanAdvance(day, MatchPhase.EndOfDay, true), Is.EqualTo(day < 5));
            }
            Assert.That(WeekRules.DayName(1), Is.EqualTo("Monday"));
            Assert.That(WeekRules.DayName(5), Is.EqualTo("Friday"));
            Assert.That(WeekRules.CanAdvance(0, MatchPhase.EndOfDay, true), Is.False);
        }
        [Test]
        public void RotationBalancesDaysAndAvoidsConsecutiveAssignment()
        {
            var rows = new[] { new WeeklyResult { PlayerId = 0, DaysAsViggo = 1 }, new WeeklyResult { PlayerId = 1 }, new WeeklyResult { PlayerId = 2 } };
            ulong previous = 0;
            foreach (var expected in new ulong[] { 1, 2, 0, 1 })
            {
                var next = WeekRules.ChooseViggo(rows, previous);
                Assert.That(next, Is.EqualTo(expected));
                rows[(int)next].DaysAsViggo++;
                previous = next;
            }
            Assert.That(WeekRules.ChooseViggo(new[] { rows[0] }, 0), Is.EqualTo(0));
        }
        [Test]
        public void DailyScoresRemainSeparateFromWeeklyAccumulation()
        {
            var monday = new DailyResult { PlayerId = 4, Score = 100, TasksCompleted = 1 };
            var total = WeekRules.Accumulate(default, monday);
            total = WeekRules.Accumulate(total, new DailyResult { PlayerId = 4, Score = -25, Captures = 2 });
            Assert.That(total.Score, Is.EqualTo(75));
            Assert.That(total.Tasks, Is.EqualTo(1));
            Assert.That(total.Captures, Is.EqualTo(2));
            Assert.That(monday.Score, Is.EqualTo(100));
        }
    }
}
