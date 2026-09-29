using EscapeViggo.Viggo;
using NUnit.Framework;
using UnityEngine;

namespace EscapeViggo.Tests
{
    public sealed class StaminaTests
    {
        private ViggoSettings _settings;
        [SetUp] public void SetUp() => _settings = ScriptableObject.CreateInstance<ViggoSettings>();
        [TearDown] public void TearDown() => Object.DestroyImmediate(_settings);

        [Test] public void MovementDrainsAndIdleRecoversWithoutExceedingBounds()
        {
            Assert.That(StaminaRules.Advance(100, 3.5f, 2, _settings), Is.EqualTo(60));
            Assert.That(StaminaRules.Advance(5, 3.5f, 2, _settings), Is.Zero);
            Assert.That(StaminaRules.Advance(60, 0, 2, _settings), Is.EqualTo(90));
            Assert.That(StaminaRules.Advance(90, 0, 2, _settings), Is.EqualTo(100));
        }

        [Test] public void ThresholdAndConditionsAreConfigurable()
        {
            _settings.MovementThreshold = 2;
            _settings.DrainPerSecond = 7;
            _settings.RecoveryPerSecond = 3;
            Assert.That(StaminaRules.Advance(50, 1, 2, _settings), Is.EqualTo(56));
            Assert.That(StaminaRules.Advance(50, 2, 2, _settings), Is.EqualTo(36));
            _settings.DrainWhileMoving = _settings.RecoverWhileStationary = false;
            Assert.That(StaminaRules.Advance(50, 5, 2, _settings), Is.EqualTo(50));
            Assert.That(StaminaRules.Advance(50, 0, 2, _settings), Is.EqualTo(50));
        }

        [TestCase(0)] [TestCase(-1)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidTimeCannotChangeStamina(float seconds) =>
            Assert.That(StaminaRules.Advance(50, 5, seconds, _settings), Is.EqualTo(50));

        [Test] public void ExhaustionEntersRestOnceAndMovementCannotDrainDuringRest()
        {
            var state = new ViggoMobilityState { Stamina = 1 };
            state = ViggoMobilityState.Advance(state, 5, 0.1f, _settings);
            Assert.That(state.Phase, Is.EqualTo(ViggoMobility.Resting));
            Assert.That(state.RestCount, Is.EqualTo(1));
            for (var i = 0; i < 10; i++) state = ViggoMobilityState.Advance(state, 100, 0.1f, _settings);
            Assert.That(state.Stamina, Is.EqualTo(25).Within(0.01f));
            Assert.That(state.RestCount, Is.EqualTo(1));
            Assert.That(state.Phase, Is.EqualTo(ViggoMobility.Resting));
            state = ViggoMobilityState.Advance(state, 100, 4, _settings);
            Assert.That(state.Phase, Is.EqualTo(ViggoMobility.Normal));
            Assert.That(state.Stamina, Is.EqualTo(100));
        }

        [Test] public void RestDurationAndRecoveryAreConfigurable()
        {
            _settings.RestSeconds = 2;
            _settings.RestRecoveryPerSecond = 10;
            _settings.RefillAtRestEnd = false;
            var state = ViggoMobilityState.Advance(new ViggoMobilityState { Stamina = 1 }, 5, 1, _settings);
            state = ViggoMobilityState.Advance(state, 5, 2, _settings);
            Assert.That(state.Phase, Is.EqualTo(ViggoMobility.Normal));
            Assert.That(state.Stamina, Is.EqualTo(20));
            state = ViggoMobilityState.Advance(state, 5, 2, _settings);
            Assert.That(state.RestCount, Is.EqualTo(2));
        }
    }
}
