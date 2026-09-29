using UnityEngine;

namespace EscapeViggo.Player
{
    [CreateAssetMenu(menuName = "Escape Viggo/Movement Settings")]
    public sealed class MovementSettings : ScriptableObject
    {
        [Min(0.1f)] public float WalkSpeed = 4.5f;
        [Min(0.1f)] public float SprintSpeed = 7f;
        [Min(0.1f)] public float CrouchSpeed = 2.3f;
        [Min(0.1f)] public float JumpHeight = 1.1f;
        [Min(0.1f)] public float Gravity = 22f;
        [Min(0.1f)] public float GroundStickSpeed = 2f;
        [Min(0.8f)] public float StandingHeight = 1.8f;
        [Min(0.8f)] public float CrouchingHeight = 1.05f;
        [Min(0.01f)] public float EyeInset = 0.15f;
        [Min(0.001f)] public float MouseSensitivity = 0.12f;
        [Range(30, 89)] public float PitchLimit = 85;
    }
}
