using EscapeViggo.Scoring;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class ScoreAwardTests
    {
        [TestCase(25)]
        [TestCase(100)]
        [TestCase(175)]
        public void RewardUsesConfiguredValueAndAddsToExistingScore(int reward)
        {
            var ledger = new ScoreAwardLedger();
            Assert.That(ledger.TryApply(42, 0, 50, reward, out var score), Is.True);
            Assert.That(score, Is.EqualTo(50 + reward));
        }

        [Test]
        public void ReplayedEventCannotAwardAgainOrAwardAnotherRecipient()
        {
            var ledger = new ScoreAwardLedger();
            ledger.TryApply(42, 0, 0, 100, out var first);
            Assert.That(ledger.TryApply(42, 0, first, 100, out var duplicate), Is.False);
            Assert.That(duplicate, Is.EqualTo(100));
            Assert.That(ledger.TryApply(42, 0, 0, 100, out var other), Is.False);
            Assert.That(other, Is.Zero);
        }

        [Test]
        public void IndependentSourcesAndNewOccurrencesCanAward()
        {
            var ledger = new ScoreAwardLedger();
            ledger.TryApply(42, 0, 0, 100, out var score);
            Assert.That(ledger.TryApply(43, 0, score, 25, out score), Is.True);
            Assert.That(ledger.TryApply(42, 1, score, 50, out score), Is.True);
            Assert.That(score, Is.EqualTo(175));
        }

        [Test]
        public void OverflowDoesNotCorruptScoreOrConsumeEvent()
        {
            var ledger = new ScoreAwardLedger();
            Assert.That(ledger.TryApply(42, 0, int.MaxValue, 100, out var score), Is.False);
            Assert.That(score, Is.EqualTo(int.MaxValue));
            Assert.That(ledger.TryApply(42, 0, 0, 100, out score), Is.True);
        }
    }
}
