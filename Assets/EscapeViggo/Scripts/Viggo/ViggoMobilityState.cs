using System;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    public enum ViggoMobility : byte { Normal, Resting }

    public struct ViggoMobilityState : INetworkSerializable, IEquatable<ViggoMobilityState>
    {
        public ViggoMobility Phase;
        public float Stamina;
        public float RestRemaining;
        public uint RestCount;
        public bool Equals(ViggoMobilityState other) => Phase == other.Phase && Stamina == other.Stamina &&
            RestRemaining == other.RestRemaining && RestCount == other.RestCount;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phase);
            serializer.SerializeValue(ref Stamina);
            serializer.SerializeValue(ref RestRemaining);
            serializer.SerializeValue(ref RestCount);
        }

        public static ViggoMobilityState Advance(ViggoMobilityState state, float speed, float seconds, ViggoSettings settings)
        {
            if (seconds <= 0 || float.IsNaN(seconds) || float.IsInfinity(seconds)) return state;
            if (state.Phase == ViggoMobility.Resting)
            {
                state.Stamina = Mathf.Min(settings.MaximumStamina,
                    state.Stamina + Mathf.Min(seconds, state.RestRemaining) * settings.RestRecoveryPerSecond);
                state.RestRemaining = Mathf.Max(0, state.RestRemaining - seconds);
                if (state.RestRemaining <= 0)
                {
                    state.Phase = ViggoMobility.Normal;
                    if (settings.RefillAtRestEnd) state.Stamina = settings.MaximumStamina;
                }
                return state;
            }
            state.Stamina = StaminaRules.Advance(state.Stamina, speed, seconds, settings);
            if (state.Stamina <= 0)
            {
                state.Phase = ViggoMobility.Resting;
                state.RestRemaining = settings.RestSeconds;
                state.RestCount++;
            }
            return state;
        }
    }
}
