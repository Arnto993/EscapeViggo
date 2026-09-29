using System.Collections.Generic;
using EscapeViggo.Battery;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Viggo;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Core
{
    [RequireComponent(typeof(NetworkWorkday))]
    public sealed class DailyResults : NetworkBehaviour
    {
        private NetworkList<DailyResult> _rows;
        private readonly NetworkVariable<DailySummary> _summary = new(default,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkWorkday _workday;
        public DailySummary Summary => _summary.Value;
        public int Count => _rows.Count;
        public DailyResult RowAt(int index) => _rows[index];
        public bool Ready => IsSpawned && Summary.Ready && Count == Summary.PlayerCount;
        private void Awake()
        {
            _rows = new NetworkList<DailyResult>();
            _workday = GetComponent<NetworkWorkday>();
        }
        public override void OnNetworkSpawn() => _workday.StateChanged += PhaseChanged;
        private void PhaseChanged(WorkdayState state)
        {
            if (state.Phase == MatchPhase.EndOfDay) FinalizeOnServer();
        }

        internal void FinalizeOnServer()
        {
            if (!IsServer || !IsSpawned || Summary.Ready || _workday.State.Phase != MatchPhase.EndOfDay) return;
            GetComponent<ViggoRage>().EndOnServer();
            GetComponent<ScoreService>().StopTasksOnServer();
            var battery = NetworkManager.GetComponent<LocalNetworkSession>().Battery;
            battery.StopTheftOnServer();
            var results = new List<DailyResult>();
            foreach (var client in NetworkManager.ConnectedClientsList)
            {
                var player = client.PlayerObject;
                if (player == null) continue;
                player.GetComponent<PlayerCapture>().StopOnServer();
                var score = player.GetComponent<PlayerScore>();
                results.Add(new DailyResult { PlayerId = client.ClientId, Role = player.GetComponent<NetworkPlayer>().Role,
                    Score = score.Value, TasksCompleted = score.TasksCompleted, Captures = score.Captures });
            }
            results.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            foreach (var result in results) _rows.Add(result);
            var outcome = battery.State;
            _summary.Value = new DailySummary { Ready = true, PlayerCount = results.Count, BatteryOutcome = outcome.Phase,
                OriginalThiefId = outcome.OriginalThiefId, CarrierId = outcome.CarrierId, Discovered = outcome.Discovered };
        }

        public override void OnNetworkDespawn() => _workday.StateChanged -= PhaseChanged;
    }
}
