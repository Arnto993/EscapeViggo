using System;
using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;

namespace EscapeViggo.Tasks
{
    public abstract class WorkTask : NetworkInteractable
    {
        protected readonly NetworkVariable<WorkTaskState> TaskState = new(
            new WorkTaskState { WorkerId = ulong.MaxValue },
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private LocalNetworkSession _session;
        public WorkTaskState State => TaskState.Value;
        public event Action<WorkTaskState> StateChanged;
        public event Action<WorkTask, ulong> CompletedOnServer;
        public override string Prompt => "Start electrical task";
        public bool IsWorkday => _session != null && _session.Workday != null && _session.Workday.State.Phase == MatchPhase.Workday;

        public static bool RoleCanWork(PlayerRole role) => role == PlayerRole.Electrician;
        public override bool CanInteract(PlayerInteraction player) => IsSpawned && IsWorkday &&
            State.Phase == WorkTaskPhase.Available && player != null && player.CanAct && RoleCanWork(player.Player.Role);

        public override void OnNetworkSpawn()
        {
            _session = NetworkManager.GetComponent<LocalNetworkSession>();
            TaskState.OnValueChanged += Changed;
            Changed(State, State);
        }

        protected override void OnServerInteract(PlayerInteraction player)
        {
            TaskState.Value = new WorkTaskState
            {
                Phase = WorkTaskPhase.InProgress, WorkerId = player.OwnerClientId,
                Revision = State.Revision + 1
            };
        }

        protected bool ValidateWorker(ulong sender, out PlayerInteraction player)
        {
            player = null;
            if (!IsServer || !IsSpawned || !IsWorkday || State.Phase != WorkTaskPhase.InProgress || State.WorkerId != sender ||
                !NetworkManager.ConnectedClients.TryGetValue(sender, out var client) || client.PlayerObject == null) return false;
            player = client.PlayerObject.GetComponent<PlayerInteraction>();
            return player != null && RoleCanWork(player.Player.Role) && player.ServerCanReach(this);
        }

        private void Update()
        {
            if (IsServer && IsSpawned && State.Phase == WorkTaskPhase.InProgress && !ValidateWorker(State.WorkerId, out _))
                CancelOnServer();
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void CancelRpc(uint revision, RpcParams rpcParams = default)
        {
            if (State.Phase == WorkTaskPhase.InProgress && State.WorkerId == rpcParams.Receive.SenderClientId && State.Revision == revision)
                CancelOnServer();
        }

        private void CancelOnServer() => TaskState.Value = new WorkTaskState
            { WorkerId = ulong.MaxValue, Revision = State.Revision + 1 };

        protected void PublishProgress(WorkTaskState next)
        {
            if (!IsServer || State.Phase != WorkTaskPhase.InProgress) return;
            TaskState.Value = next;
            if (next.Phase == WorkTaskPhase.Completed) CompletedOnServer?.Invoke(this, next.WorkerId);
        }

        private void Changed(WorkTaskState previous, WorkTaskState current) => StateChanged?.Invoke(current);
        public override void OnNetworkDespawn() => TaskState.OnValueChanged -= Changed;
    }
}
