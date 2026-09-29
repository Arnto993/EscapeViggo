using UnityEngine;

namespace EscapeViggo.Player
{
    [CreateAssetMenu(menuName = "Escape Viggo/Capture Settings")]
    public sealed class CaptureSettings : ScriptableObject
    {
        [Range(0.5f, 3)] public float Range = 2;
        [Min(0.5f)] public float DisableSeconds = 3;
        [Min(0)] public float RespawnProtectionSeconds = 2;
    }
}
