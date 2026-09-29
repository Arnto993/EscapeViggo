using UnityEngine;

namespace EscapeViggo.Viggo
{
    [CreateAssetMenu(menuName = "Escape Viggo/Viggo Settings")]
    public sealed class ViggoSettings : ScriptableObject
    {
        [Min(1)] public float MaximumStamina = 100;
        [Min(0)] public float DrainPerSecond = 20;
        [Min(0)] public float RecoveryPerSecond = 15;
        [Tooltip("Server-observed horizontal metres per second that count as movement.")]
        [Min(0.01f)] public float MovementThreshold = 0.1f;
        public bool DrainWhileMoving = true;
        public bool RecoverWhileStationary = true;
        [Min(0.1f)] public float WalkSpeed = 3.5f;
        [Min(0.1f)] public float ChaseSpeed = 5.5f;
        [Min(0.1f)] public float CrouchSpeed = 1.8f;
        [Min(0.1f)] public float RestSeconds = 4;
        [Min(0)] public float RestRecoveryPerSecond = 25;
        public bool RefillAtRestEnd = true;
        [Range(0, 0.25f)] public float RestMovementMultiplier = 0;
        [Min(0.1f)] public float RageSpeed = 8;
        public bool EndRageOnRecovery = true;
        public bool EndRageOnCarrierCapture = true;
    }
}
