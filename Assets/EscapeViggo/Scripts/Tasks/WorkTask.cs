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
        private Viggo.WorkDistractions Distractions => _session?.Workday?.GetComponent<Viggo.WorkDistractions>();
        public bool HasProgress => State.Phase == WorkTaskPhase.InProgress && (State.Progress > 0 || State.Connections != 0);
        public event Action<WorkTaskState> StateChanged;
        public event Action<WorkTask, ulong> CompletedOnServer;
        public override string Prompt => "Start electrical task";
        public virtual string Title => "SIMPLE CIRCUIT";
        public virtual int Reward(Scoring.ScoreSettings settings) => settings.CircuitTaskPoints;
        protected void RefreshPresentation() => StateChanged?.Invoke(State);
        public bool IsWorkday => _session != null && _session.Workday != null && _session.Workday.State.Phase == MatchPhase.Workday;

        public static bool RoleCanWork(PlayerRole role) => role == PlayerRole.Electrician;
        public override string GetPrompt(PlayerInteraction player) => player.Player.Role == PlayerRole.Viggo ? "Interrupt active work" : Prompt;
        public override bool CanInteract(PlayerInteraction player)
        {
            if (!IsSpawned || !IsWorkday || player == null || !player.CanAct) return false;
            if (player.Player.Role == PlayerRole.Viggo) return Distractions != null && Distractions.CanInterrupt(player, this);
            return State.Phase == WorkTaskPhase.Available && RoleCanWork(player.Player.Role) &&
                (Distractions == null || !Distractions.PowerOff) && NetworkManager.ServerTime.Time >= State.InterruptedUntil &&
                !_session.Workday.GetComponent<Scoring.ScoreService>().HasActiveTask(player.OwnerClientId);
        }

        public override void OnNetworkSpawn()
        {
            _session = NetworkManager.GetComponent<LocalNetworkSession>();
            TaskState.OnValueChanged += Changed;
            Changed(State, State);
        }

        protected override void OnServerInteract(PlayerInteraction player)
        {
            if (player.Player.Role == PlayerRole.Viggo) { Distractions?.InterruptOnServer(player, this); return; }
            TaskState.Value = new WorkTaskState
            {
                Phase = WorkTaskPhase.InProgress, WorkerId = player.OwnerClientId,
                Revision = State.Revision + 1
            };
        }

        protected bool ValidateWorker(ulong sender, out PlayerInteraction player)
        {
            player = null;
            if (!IsServer || !IsSpawned || !IsWorkday || (Distractions != null && Distractions.PowerOff) || State.Phase != WorkTaskPhase.InProgress || State.WorkerId != sender ||
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

        internal void StopOnServer()
        {
            if (IsServer && IsSpawned && (State.Phase == WorkTaskPhase.InProgress || State.InterruptedUntil > 0)) CancelOnServer();
        }

        internal void InterruptOnServer(double retryAt)
        {
            if (!IsServer || !IsSpawned || !HasProgress) return;
            TaskState.Value = new WorkTaskState { WorkerId = ulong.MaxValue, Revision = State.Revision + 1, InterruptedUntil = retryAt };
        }

        internal virtual void ResetDayOnServer()
        {
            if (IsServer && IsSpawned) CancelOnServer(); // Keep revisions monotonic across days.
        }

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
