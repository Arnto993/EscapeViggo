using UnityEngine;

namespace EscapeViggo.Battery
{
    [CreateAssetMenu(menuName = "Escape Viggo/Battery Settings")]
    public sealed class BatterySettings : ScriptableObject
    {
        [Min(0.5f)] public float TheftSeconds = 3;
        [Tooltip("Cancel if no validated hold request arrives within this interval.")]
        [Range(0.2f, 1)] public float HoldTimeoutSeconds = 0.4f;
    }
}
