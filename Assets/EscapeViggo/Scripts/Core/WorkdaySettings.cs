using UnityEngine;

namespace EscapeViggo.Core
{
    [CreateAssetMenu(menuName = "Escape Viggo/Workday Settings")]
    public sealed class WorkdaySettings : ScriptableObject
    {
        [Min(0.01f)] public float DayDurationMinutes = 2;
        [Min(0)] public float PreparingSeconds = 5;
    }
}
