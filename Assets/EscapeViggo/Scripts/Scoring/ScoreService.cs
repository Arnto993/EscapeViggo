using System.Collections.Generic;
using EscapeViggo.Core;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using EscapeViggo.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Scoring
{
    [RequireComponent(typeof(NetworkWorkday))]
    public sealed class ScoreService : NetworkBehaviour
    {
        [SerializeField] private ScoreSettings _settings;
        private readonly List<WorkTask> _tasks = new();
        private readonly ScoreAwardLedger _ledger = new();
        private NetworkWorkday _workday;
        public ScoreSettings Settings => _settings;
        private void Awake() => _workday = GetComponent<NetworkWorkday>();

        internal void RegisterTask(WorkTask task)
        {
            if (!IsServer || !IsSpawned || task == null || _tasks.Contains(task)) return;
            _tasks.Add(task);
            task.CompletedOnServer += TaskCompleted;
        }

        private void TaskCompleted(WorkTask task, ulong workerId)
        {
            if (!IsServer || !IsSpawned || _workday.State.Phase != MatchPhase.Workday ||
                task.State.Phase != WorkTaskPhase.Completed || task.State.WorkerId != workerId ||
                !NetworkManager.ConnectedClients.TryGetValue(workerId, out var client) || client.PlayerObject == null) return;
            var player = client.PlayerObject.GetComponent<NetworkPlayer>();
            if (player.Role != PlayerRole.Electrician) return;
            TryAwardOnce(player.GetComponent<PlayerScore>(), task.NetworkObject, 0, Mathf.Max(0, _settings.CircuitTaskPoints));
        }

        // Future server-validated gameplay events can use this role-independent path.
        // Source plus server-generated occurrence number identifies the event; no client values enter here.
        internal bool TryAwardOnce(PlayerScore recipient, NetworkObject source, uint occurrence, int points)
        {
            if (!IsServer || !IsSpawned || _workday.State.Phase != MatchPhase.Workday || recipient == null ||
                !recipient.IsSpawned || !recipient.IsServer || source == null || !source.IsSpawned) return false;
            if (!_ledger.TryApply(source.NetworkObjectId, occurrence, recipient.Value, points, out var updated)) return false;
            recipient.SetOnServer(updated);
            return true;
        }

        public override void OnNetworkDespawn()
        {
            foreach (var task in _tasks) if (task != null) task.CompletedOnServer -= TaskCompleted;
            _tasks.Clear();
        }
    }
}
