using Unity.Netcode;

namespace EscapeViggo.Scoring
{
    public sealed class PlayerScore : NetworkBehaviour
    {
        private readonly NetworkVariable<int> _score = new(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public int Value => _score.Value;
        private readonly NetworkVariable<int> _captures = new(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public int Captures => _captures.Value;
        internal void RecordCaptureOnServer() { if (IsServer && IsSpawned) _captures.Value++; }

        // No client RPC or public score setter. ScoreService supplies validated server outcomes.
        internal void SetOnServer(int value)
        {
            if (IsServer && IsSpawned) _score.Value = value;
        }
    }
}
