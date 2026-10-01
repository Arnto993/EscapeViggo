using System;
using System.Collections.Generic;
using EscapeViggo.Networking;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Viggo;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Core
{
    public struct WeeklyResult : INetworkSerializable, IEquatable<WeeklyResult>
    {
        public ulong PlayerId;
        public int Score, Tasks, Captures, DaysAsViggo, BatteryThefts, BatterySurvivals;
        public bool Equals(WeeklyResult other) => PlayerId == other.PlayerId && Score == other.Score && Tasks == other.Tasks &&
            Captures == other.Captures && DaysAsViggo == other.DaysAsViggo && BatteryThefts == other.BatteryThefts && BatterySurvivals == other.BatterySurvivals;
        public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
        {
            s.SerializeValue(ref PlayerId); s.SerializeValue(ref Score); s.SerializeValue(ref Tasks);
            s.SerializeValue(ref Captures); s.SerializeValue(ref DaysAsViggo); s.SerializeValue(ref BatteryThefts); s.SerializeValue(ref BatterySurvivals);
        }
        public override string ToString() => $"{PlayerId}:{Score}:{Tasks}:{Captures}:{DaysAsViggo}:{BatteryThefts}:{BatterySurvivals}";
    }

    public static class WeekRules
    {
        public const int LastDay = 5;
        public static bool CanAdvance(int day, MatchPhase phase, bool resultsReady) => day >= 1 && day < LastDay && phase == MatchPhase.EndOfDay && resultsReady;
        public static string DayName(int day) => day switch { 1 => "Monday", 2 => "Tuesday", 3 => "Wednesday", 4 => "Thursday", 5 => "Friday", _ => "Unknown" };
        public static ulong ChooseViggo(IReadOnlyList<WeeklyResult> candidates, ulong previous)
        {
            if (candidates.Count == 0) throw new ArgumentException("A workday needs a player.", nameof(candidates));
            WeeklyResult? best = null;
            foreach (var candidate in candidates)
            {
                if (candidates.Count > 1 && candidate.PlayerId == previous) continue;
                if (!best.HasValue || candidate.DaysAsViggo < best.Value.DaysAsViggo ||
                    candidate.DaysAsViggo == best.Value.DaysAsViggo && candidate.PlayerId < best.Value.PlayerId) best = candidate;
            }
            return best.Value.PlayerId;
        }
        public static WeeklyResult Accumulate(WeeklyResult total, DailyResult day)
        {
            total.PlayerId = day.PlayerId;
            total.Score = (int)Math.Clamp((long)total.Score + day.Score, int.MinValue, int.MaxValue);
            total.Tasks += day.TasksCompleted;
            total.Captures += day.Captures;
            return total;
        }
    }

    [RequireComponent(typeof(NetworkWorkday), typeof(DailyResults))]
    public sealed class WorkWeek : NetworkBehaviour
    {
        [SerializeField, Min(0)] private float _minimumReviewSeconds = 3;
        private readonly NetworkVariable<int> _day = new(1);
        private NetworkList<WeeklyResult> _totals;
        private int _finalizedDay;
        private double _reviewStarted;
        private ulong _viggo = NetworkManager.ServerClientId;
        public int Day => _day.Value;
        public string DayName => WeekRules.DayName(Day);
        public int Count => _totals.Count;
        public WeeklyResult RowAt(int index) => _totals[index];
        public bool IsFinal => Day == WeekRules.LastDay && GetComponent<DailyResults>().Ready;
        public bool CanContinue => IsServer && WeekRules.CanAdvance(Day, GetComponent<NetworkWorkday>().State.Phase,
            GetComponent<DailyResults>().Ready) && NetworkManager.ServerTime.Time >= _reviewStarted + _minimumReviewSeconds;
        private void Awake() => _totals = new NetworkList<WeeklyResult>();

        public WeeklyResult TotalFor(ulong id)
        {
            foreach (var row in _totals) if (row.PlayerId == id) return row;
            return new WeeklyResult { PlayerId = id };
        }
        private void Save(WeeklyResult row)
        {
            for (var i = 0; i < _totals.Count; i++) if (_totals[i].PlayerId == row.PlayerId) { _totals[i] = row; return; }
            _totals.Add(row);
        }
        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;
            var row = TotalFor(_viggo); row.DaysAsViggo++; Save(row);
            NetworkManager.OnClientDisconnectCallback += ReplaceDisconnectedViggo;
        }
        public override void OnNetworkDespawn()
        {
            if (IsServer) NetworkManager.OnClientDisconnectCallback -= ReplaceDisconnectedViggo;
        }
        private void ReplaceDisconnectedViggo(ulong id)
        {
            if (!IsServer || NetworkManager.ShutdownInProgress || id != _viggo || GetComponent<NetworkWorkday>().State.Phase == MatchPhase.EndOfDay) return;
            var candidates = new List<WeeklyResult>();
            foreach (var client in NetworkManager.ConnectedClientsList)
                if (client.ClientId != id && client.PlayerObject != null) candidates.Add(TotalFor(client.ClientId));
            if (candidates.Count == 0) return;
            _viggo = WeekRules.ChooseViggo(candidates, id);
            var row = TotalFor(_viggo); row.DaysAsViggo++; Save(row);
            NetworkManager.GetComponent<LocalNetworkSession>().PromoteViggoOnServer(_viggo);
        }
        internal void FinalizeDayOnServer(DailyResults results)
        {
            if (!IsServer || !results.Ready || _finalizedDay == Day) return;
            for (var i = 0; i < results.Count; i++)
            {
                var daily = results.RowAt(i);
                var total = WeekRules.Accumulate(TotalFor(daily.PlayerId), daily);
                if (results.Summary.OriginalThiefId == daily.PlayerId) total.BatteryThefts++;
                if (results.Summary.CarrierId == daily.PlayerId) total.BatterySurvivals++;
                Save(total);
            }
            _finalizedDay = Day;
            _reviewStarted = NetworkManager.ServerTime.Time;
        }
        public bool ContinueOnServer()
        {
            if (!CanContinue) return false;
            var candidates = new List<WeeklyResult>();
            foreach (var client in NetworkManager.ConnectedClientsList) if (client.PlayerObject != null) candidates.Add(TotalFor(client.ClientId));
            if (candidates.Count == 0) return false;
            _viggo = WeekRules.ChooseViggo(candidates, _viggo);
            var total = TotalFor(_viggo); total.DaysAsViggo++; Save(total);
            var session = NetworkManager.GetComponent<LocalNetworkSession>();
            GetComponent<DailyResults>().ResetOnServer();
            GetComponent<ScoreService>().ResetDayOnServer();
            GetComponent<WorkDistractions>()?.ResetDayOnServer();
            GetComponent<ViggoRage>().EndOnServer();
            session.Battery.ResetDayOnServer();
            session.RotateRolesOnServer(_viggo);
            _day.Value++;
            GetComponent<NetworkWorkday>().RestartDayOnServer();
            return true;
        }
    }
}
