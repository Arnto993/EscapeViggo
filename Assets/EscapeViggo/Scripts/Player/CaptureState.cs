using System;
using EscapeViggo.Core;
using Unity.Netcode;

namespace EscapeViggo.Player
{
    public enum CapturePhase : byte { Active, Captured }

    public struct CaptureState : INetworkSerializable, IEquatable<CaptureState>
    {
        public CapturePhase Phase;
        public uint Sequence;
        public ulong CaptorId;
        public double ReleaseAt, ProtectedUntil;
        public bool Equals(CaptureState other) => Phase == other.Phase && Sequence == other.Sequence && CaptorId == other.CaptorId &&
            ReleaseAt == other.ReleaseAt && ProtectedUntil == other.ProtectedUntil;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phase);
            serializer.SerializeValue(ref Sequence);
            serializer.SerializeValue(ref CaptorId);
            serializer.SerializeValue(ref ReleaseAt);
            serializer.SerializeValue(ref ProtectedUntil);
        }
    }

    public static class CaptureRules
    {
        public static bool CanCapture(PlayerRole actor, PlayerRole target, MatchPhase phase, bool resting,
            CaptureState state, double now, float distance, float range) => actor == PlayerRole.Viggo &&
            target == PlayerRole.Electrician && phase == MatchPhase.Workday && !resting && state.Phase == CapturePhase.Active &&
            now >= state.ProtectedUntil && float.IsFinite(distance) && distance >= 0 && distance <= range;
    }
}
