using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Tasks
{
    public sealed class SimpleCircuitTask : WorkTask
    {
        [SerializeField] private int[] _sequence = { 1, 3, 2 };
        public string SequenceText => string.Join(" > ", _sequence);
        public bool HasValidSequence => CircuitSequence.IsValid(_sequence);

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (!HasValidSequence) Debug.LogError("Simple Circuit Task requires exactly three entries, each between 1 and 3.", this);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SubmitElementRpc(int element, uint expectedRevision, RpcParams rpcParams = default)
        {
            var sender = rpcParams.Receive.SenderClientId;
            if (!HasValidSequence || !ValidateWorker(sender, out _)) return;
            if (CircuitSequence.TryAdvance(State, _sequence, sender, element, expectedRevision, out var next)) PublishProgress(next);
        }
    }
}
