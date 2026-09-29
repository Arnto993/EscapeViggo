using System;
using Unity.Netcode;

namespace EscapeViggo.Tasks
{
    public enum WorkTaskPhase : byte { Available, InProgress, Completed }

    public struct WorkTaskState : INetworkSerializable, IEquatable<WorkTaskState>
    {
        public WorkTaskPhase Phase;
        public ulong WorkerId;
        public int Progress;
        public bool WrongInput;
        public uint Revision;
        public bool Equals(WorkTaskState other) => Phase == other.Phase && WorkerId == other.WorkerId &&
            Progress == other.Progress && WrongInput == other.WrongInput && Revision == other.Revision;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phase);
            serializer.SerializeValue(ref WorkerId);
            serializer.SerializeValue(ref Progress);
            serializer.SerializeValue(ref WrongInput);
            serializer.SerializeValue(ref Revision);
        }
    }
}
