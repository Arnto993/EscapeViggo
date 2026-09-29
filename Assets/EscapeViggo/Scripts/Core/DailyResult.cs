using System;
using EscapeViggo.Battery;
using EscapeViggo.Player;
using Unity.Netcode;

namespace EscapeViggo.Core
{
    public struct DailyResult : INetworkSerializable, IEquatable<DailyResult>
    {
        public ulong PlayerId;
        public PlayerRole Role;
        public int Score, TasksCompleted, Captures;
        public bool Equals(DailyResult other) => PlayerId == other.PlayerId && Role == other.Role && Score == other.Score &&
            TasksCompleted == other.TasksCompleted && Captures == other.Captures;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref PlayerId);
            serializer.SerializeValue(ref Role);
            serializer.SerializeValue(ref Score);
            serializer.SerializeValue(ref TasksCompleted);
            serializer.SerializeValue(ref Captures);
        }
        public override string ToString() => $"Player {PlayerId} | {Role} | {Score} | {TasksCompleted} | {Captures}";
    }

    public struct DailySummary : INetworkSerializable, IEquatable<DailySummary>
    {
        public bool Ready;
        public int PlayerCount;
        public BatteryPhase BatteryOutcome;
        public ulong OriginalThiefId, CarrierId;
        public bool Discovered;
        public bool Equals(DailySummary other) => Ready == other.Ready && PlayerCount == other.PlayerCount && BatteryOutcome == other.BatteryOutcome &&
            OriginalThiefId == other.OriginalThiefId && CarrierId == other.CarrierId && Discovered == other.Discovered;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Ready);
            serializer.SerializeValue(ref PlayerCount);
            serializer.SerializeValue(ref BatteryOutcome);
            serializer.SerializeValue(ref OriginalThiefId);
            serializer.SerializeValue(ref CarrierId);
            serializer.SerializeValue(ref Discovered);
        }
    }
}
