using UnityEngine;

namespace EscapeViggo.Viggo
{
    public static class StaminaRules
    {
        public static float Advance(float current, float horizontalSpeed, float seconds, ViggoSettings settings)
        {
            if (seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return current;
            var moving = horizontalSpeed >= settings.MovementThreshold;
            var rate = moving ? (settings.DrainWhileMoving ? -settings.DrainPerSecond : 0) :
                (settings.RecoverWhileStationary ? settings.RecoveryPerSecond : 0);
            return Mathf.Clamp(current + rate * seconds, 0, settings.MaximumStamina);
        }
    }
}
