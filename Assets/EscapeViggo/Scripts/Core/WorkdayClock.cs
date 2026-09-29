using System;
using Unity.Netcode;

namespace EscapeViggo.Core
{
    public enum MatchPhase : byte { Preparing, Workday, EndOfDay }

    public struct WorkdayState : INetworkSerializable, IEquatable<WorkdayState>
    {
        public MatchPhase Phase;
        public int MinuteOfDay;
        public string ClockText => $"{MinuteOfDay / 60:00}:{MinuteOfDay % 60:00}";
        public bool Equals(WorkdayState other) => Phase == other.Phase && MinuteOfDay == other.MinuteOfDay;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phase);
            serializer.SerializeValue(ref MinuteOfDay);
        }
    }

    public static class WorkdayClock
    {
        public const int StartMinute = 7 * 60;
        public const int EndMinute = 15 * 60;

        public static WorkdayState Evaluate(double elapsedSeconds, double preparingSeconds, double dayDurationMinutes)
        {
            if (double.IsNaN(dayDurationMinutes) || double.IsInfinity(dayDurationMinutes) || dayDurationMinutes <= 0)
                throw new ArgumentOutOfRangeException(nameof(dayDurationMinutes));
            var elapsed = Math.Max(0, elapsedSeconds - Math.Max(0, preparingSeconds));
            var progress = Math.Clamp(elapsed / (dayDurationMinutes * 60), 0, 1);
            return new WorkdayState
            {
                Phase = elapsedSeconds < preparingSeconds ? MatchPhase.Preparing :
                    progress >= 1 ? MatchPhase.EndOfDay : MatchPhase.Workday,
                MinuteOfDay = StartMinute + (int)Math.Floor(progress * (EndMinute - StartMinute))
            };
        }
    }
}
