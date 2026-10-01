using System.Collections.Generic;

namespace EscapeViggo.Scoring
{
    /// <summary>One award per authoritative source event, regardless of recipient or message repetition.</summary>
    public sealed class ScoreAwardLedger
    {
        private readonly HashSet<(ulong Source, uint Sequence)> _awarded = new();
        public void Clear() => _awarded.Clear();

        public bool TryApply(ulong source, uint sequence, int currentScore, int points, out int updatedScore)
        {
            updatedScore = currentScore;
            var sum = (long)currentScore + points;
            if (sum < int.MinValue || sum > int.MaxValue || !_awarded.Add((source, sequence))) return false;
            updatedScore = (int)sum;
            return true;
        }
    }
}
