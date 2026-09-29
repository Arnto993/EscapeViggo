using System;
using EscapeViggo.Battery;
using EscapeViggo.Core;
using EscapeViggo.Networking;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    public struct RageState : INetworkSerializable, IEquatable<RageState>
    {
        public bool Active;
        public uint Sequence;
        public bool Equals(RageState other) => Active == other.Active && Sequence == other.Sequence;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Active);
            serializer.SerializeValue(ref Sequence);
        }
    }

    [RequireComponent(typeof(NetworkWorkday))]
    public sealed class ViggoRage : NetworkBehaviour
    {
        [SerializeField] private ViggoSettings _settings;
        private readonly NetworkVariable<RageState> _state = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkWorkday _workday;
        public ViggoSettings Settings => _settings;
        public bool Active => _state.Value.Active;
        public uint Sequence => _state.Value.Sequence;
        private void Awake() => _workday = GetComponent<NetworkWorkday>();

        internal void BeginOnServer(BatteryStation battery)
        {
            if (!IsServer || !IsSpawned || Active || _workday.State.Phase != MatchPhase.Workday ||
                battery != NetworkManager.GetComponent<LocalNetworkSession>().Battery || !battery.State.Discovered ||
                battery.State.Phase != BatteryPhase.Carried) return;
            _state.Value = new RageState { Active = true, Sequence = Sequence + 1 };
        }

        internal void EndOnServer()
        {
            if (IsServer && IsSpawned && Active) _state.Value = new RageState { Sequence = Sequence };
        }

        private void Update()
        {
            if (IsServer && IsSpawned && Active && _workday.State.Phase != MatchPhase.Workday) EndOnServer();
        }
    }
}
