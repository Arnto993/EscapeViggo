using UnityEngine;

namespace EscapeViggo.Scoring
{
    [CreateAssetMenu(menuName = "Escape Viggo/Score Settings")]
    public sealed class ScoreSettings : ScriptableObject
    {
        [Min(0)] public int CircuitTaskPoints = 100;
        [Min(0)] public int CapturePoints = 50;
        [Min(0)] public int CarrierCaptureBonus = 50;
        [Min(0)] public int CapturedPenalty = 25;
    }
}
