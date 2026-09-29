using System;
using Unity.Netcode;

namespace EscapeViggo.Battery
{
    public enum BatteryPhase : byte { AtDrill, BeingStolen, Carried }

    public struct BatteryState : INetworkSerializable, IEquatable<BatteryState>
    {
        public const ulong Nobody = ulong.MaxValue;
        public BatteryPhase Phase;
        public ulong ActorId, CarrierId, OriginalThiefId;
        public float Progress;
        public bool Discovered;
        public uint TheftId;
        public static BatteryState Initial => new() { ActorId = Nobody, CarrierId = Nobody, OriginalThiefId = Nobody };
        public bool Equals(BatteryState other) => Phase == other.Phase && ActorId == other.ActorId &&
            CarrierId == other.CarrierId && OriginalThiefId == other.OriginalThiefId && Progress == other.Progress &&
            Discovered == other.Discovered && TheftId == other.TheftId;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phase);
            serializer.SerializeValue(ref ActorId);
            serializer.SerializeValue(ref CarrierId);
            serializer.SerializeValue(ref OriginalThiefId);
            serializer.SerializeValue(ref Progress);
            serializer.SerializeValue(ref Discovered);
            serializer.SerializeValue(ref TheftId);
        }
    }

    public static class BatteryRules
    {
        public static bool Discover(ref BatteryState state, Player.PlayerRole role)
        {
            if (role != Player.PlayerRole.Viggo || state.Phase != BatteryPhase.Carried || state.Discovered) return false;
            state.Discovered = true;
            return true;
        }

        public static bool TryBegin(ref BatteryState state, ulong actor)
        {
            if (actor == BatteryState.Nobody || state.Phase != BatteryPhase.AtDrill) return false;
            state.Phase = BatteryPhase.BeingStolen;
            state.ActorId = actor;
            state.Progress = 0;
            return true;
        }

        public static bool Cancel(ref BatteryState state, ulong actor)
        {
            if (state.Phase != BatteryPhase.BeingStolen || state.ActorId != actor) return false;
            state.Phase = BatteryPhase.AtDrill;
            state.ActorId = BatteryState.Nobody;
            state.Progress = 0;
            return true;
        }

        public static bool Complete(ref BatteryState state)
        {
            if (state.Phase != BatteryPhase.BeingStolen || state.Progress < 1) return false;
            state.Phase = BatteryPhase.Carried;
            state.CarrierId = state.ActorId;
            state.OriginalThiefId = state.ActorId;
            state.ActorId = BatteryState.Nobody;
            state.Discovered = false;
            state.TheftId++;
            return true;
        }
    }
}
