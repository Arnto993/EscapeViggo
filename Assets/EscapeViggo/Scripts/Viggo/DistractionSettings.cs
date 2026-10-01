using UnityEngine;

namespace EscapeViggo.Viggo
{
    [CreateAssetMenu(menuName = "Escape Viggo/Distraction Settings")]
    public sealed class DistractionSettings : ScriptableObject
    {
        [Min(0.1f)] public float InterruptRange = 2.5f;
        [Min(1)] public float InterruptCooldownSeconds = 8;
        [Min(0)] public float TaskRetrySeconds = 2;
        [Min(1)] public float PowerCutSeconds = 6;
        [Min(1)] public float PowerCooldownSeconds = 20;
        [Min(0)] public float RestoreDelaySeconds = 2;
    }
}
