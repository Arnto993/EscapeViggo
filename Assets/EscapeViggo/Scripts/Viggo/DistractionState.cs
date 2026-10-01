using System;
using EscapeViggo.Core;
using EscapeViggo.Player;
using Unity.Netcode;

namespace EscapeViggo.Viggo
{
    public enum DistractionKind : byte { None, WorkInterruption, PowerCut }
    public struct DistractionState : INetworkSerializable, IEquatable<DistractionState>
    {
        public double InterruptReadyAt, PowerReadyAt, PowerUntil, RestoreAt;
        public uint Sequence;
        public ulong ActorId, VictimId;
        public DistractionKind Kind;
        public bool Meaningful;
        public bool Equals(DistractionState other) => InterruptReadyAt == other.InterruptReadyAt && PowerReadyAt == other.PowerReadyAt &&
            PowerUntil == other.PowerUntil && RestoreAt == other.RestoreAt && Sequence == other.Sequence && ActorId == other.ActorId &&
            VictimId == other.VictimId && Kind == other.Kind && Meaningful == other.Meaningful;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref InterruptReadyAt); s.SerializeValue(ref PowerReadyAt); s.SerializeValue(ref PowerUntil); s.SerializeValue(ref RestoreAt);
            s.SerializeValue(ref Sequence); s.SerializeValue(ref ActorId); s.SerializeValue(ref VictimId); s.SerializeValue(ref Kind); s.SerializeValue(ref Meaningful);
        }
    }
    public static class DistractionRules
    {
        public static bool CanInterrupt(PlayerRole role, MatchPhase phase, bool resting, bool hasProgress, double now, double readyAt, float distance, float range) =>
            role == PlayerRole.Viggo && phase == MatchPhase.Workday && !resting && hasProgress && now >= readyAt &&
            float.IsFinite(distance) && distance >= 0 && distance <= range;
        public static bool CanCut(PlayerRole role, MatchPhase phase, bool resting, double now, DistractionState state) =>
            role == PlayerRole.Viggo && phase == MatchPhase.Workday && !resting && state.PowerUntil == 0 && now >= state.PowerReadyAt;
        public static bool CanRestore(PlayerRole role, MatchPhase phase, double now, DistractionState state) =>
            role == PlayerRole.Electrician && phase == MatchPhase.Workday && state.PowerUntil > now && now >= state.RestoreAt;
        public static DistractionState StopPower(DistractionState state) { state.PowerUntil = state.RestoreAt = 0; return state; }
        public static DistractionState NewDay(DistractionState state) => new() { Sequence = state.Sequence, ActorId = ulong.MaxValue, VictimId = ulong.MaxValue };
    }
}
