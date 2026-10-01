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
        public int Connections, Selection;
        public bool WrongInput;
        public uint Revision;
        public double InterruptedUntil;
        public bool Equals(WorkTaskState other) => Phase == other.Phase && WorkerId == other.WorkerId &&
            Progress == other.Progress && Connections == other.Connections && Selection == other.Selection && WrongInput == other.WrongInput && Revision == other.Revision && InterruptedUntil == other.InterruptedUntil;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Phase);
            serializer.SerializeValue(ref WorkerId);
            serializer.SerializeValue(ref Progress);
            serializer.SerializeValue(ref Connections);
            serializer.SerializeValue(ref Selection);
            serializer.SerializeValue(ref WrongInput);
            serializer.SerializeValue(ref Revision);
            serializer.SerializeValue(ref InterruptedUntil);
        }
    }
}
