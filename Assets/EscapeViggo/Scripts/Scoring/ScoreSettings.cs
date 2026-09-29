using UnityEngine;

namespace EscapeViggo.Scoring
{
    [CreateAssetMenu(menuName = "Escape Viggo/Score Settings")]
    public sealed class ScoreSettings : ScriptableObject
    {
        [Min(0)] public int CircuitTaskPoints = 100;
    }
}
