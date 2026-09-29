using EscapeViggo.Battery;
using NUnit.Framework;

namespace EscapeViggo.Tests
{
    public sealed class BatteryTests
    {
        [Test] public void OnlyOneActorCanBeginAndOnlyThatActorCanCancel()
        {
            var state = BatteryState.Initial;
            Assert.That(BatteryRules.TryBegin(ref state, 1), Is.True);
            Assert.That(BatteryRules.TryBegin(ref state, 2), Is.False);
            Assert.That(BatteryRules.Cancel(ref state, 2), Is.False);
            Assert.That(BatteryRules.Complete(ref state), Is.False);
            Assert.That(BatteryRules.Cancel(ref state, 1), Is.True);
            Assert.That(state.Phase, Is.EqualTo(BatteryPhase.AtDrill));
            Assert.That(state.CarrierId, Is.EqualTo(BatteryState.Nobody));
            Assert.That(BatteryRules.TryBegin(ref state, 2), Is.True);
        }

        [Test] public void CompletedTheftIsUniqueAndUndiscovered()
        {
            var state = BatteryState.Initial;
            BatteryRules.TryBegin(ref state, 7);
            state.Progress = 1;
            Assert.That(BatteryRules.Complete(ref state), Is.True);
            Assert.That(BatteryRules.Complete(ref state), Is.False);
            Assert.That(BatteryRules.TryBegin(ref state, 8), Is.False);
            Assert.That(state.CarrierId, Is.EqualTo(7));
            Assert.That(state.OriginalThiefId, Is.EqualTo(7));
            Assert.That(state.TheftId, Is.EqualTo(1));
            Assert.That(state.Discovered, Is.False);
            state.CarrierId = 8;
            Assert.That(state.OriginalThiefId, Is.EqualTo(7), "Carrier and original thief must be separate fields.");
        }
    }
}
