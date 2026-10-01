using UnityEngine;

namespace EscapeViggo.Scoring
{
    [CreateAssetMenu(menuName = "Escape Viggo/Score Settings")]
    public sealed class ScoreSettings : ScriptableObject
    {
        [Min(0)] public int CircuitTaskPoints = 100;
        [Min(0)] public int VoltageTaskPoints = 120;
        [Min(0)] public int OutletTaskPoints = 150;
        [Min(0)] public int BreakerTaskPoints = 180;
        [Min(0)] public int WorkInterruptionPoints = 15;
        [Min(0)] public int PowerCutPoints = 25;
        [Min(0)] public int CapturePoints = 50;
        [Min(0)] public int CarrierCaptureBonus = 50;
        [Min(0)] public int CapturedPenalty = 25;
    }
}
