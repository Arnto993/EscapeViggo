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
    }
}
