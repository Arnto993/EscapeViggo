using EscapeViggo.Core;
using EscapeViggo.Networking;
using EscapeViggo.Scoring;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Tasks
{
    public sealed class ElectricalWorkTask : WorkTask
    {
        [SerializeField] private ElectricalTaskKind[] _dailyTypes = { ElectricalTaskKind.VoltageMeasurement, ElectricalTaskKind.OutletReplacement };
        private readonly NetworkVariable<ElectricalTaskKind> _kind = new();
        public ElectricalTaskKind Kind => _kind.Value;
        public ElectricalTaskKind TypeForDay(int day) => TaskPlacement.ForDay(_dailyTypes, day);
        public bool HasValidLayout => _dailyTypes != null && _dailyTypes.Length > 0 && System.Array.TrueForAll(_dailyTypes, k => System.Enum.IsDefined(typeof(ElectricalTaskKind), k));
        public override string Title => Kind switch { ElectricalTaskKind.VoltageMeasurement => "VOLTAGE MEASUREMENT", ElectricalTaskKind.OutletReplacement => "OUTLET REPLACEMENT", _ => "BREAKER INSTALLATION" };
        public override string Prompt => "Start " + Title.ToLowerInvariant();
        public override int Reward(ScoreSettings settings) => Kind switch { ElectricalTaskKind.VoltageMeasurement => settings.VoltageTaskPoints, ElectricalTaskKind.OutletReplacement => settings.OutletTaskPoints, _ => settings.BreakerTaskPoints };
        public override void OnNetworkSpawn()
        {
            if (IsServer && HasValidLayout) _kind.Value = TaskPlacement.ForDay(_dailyTypes, 1);
            base.OnNetworkSpawn();
            _kind.OnValueChanged += KindChanged;
            if (!HasValidLayout) Debug.LogError("ElectricalWorkTask needs a valid daily type layout.", this);
        }
        private void KindChanged(ElectricalTaskKind before, ElectricalTaskKind after) => RefreshPresentation();
        public override void OnNetworkDespawn() { _kind.OnValueChanged -= KindChanged; base.OnNetworkDespawn(); }
        internal override void ResetDayOnServer()
        {
            if (!IsServer || !IsSpawned) return;
            base.ResetDayOnServer();
            var nextDay = NetworkManager.GetComponent<LocalNetworkSession>().Workday.GetComponent<WorkWeek>().Day + 1;
            _kind.Value = TaskPlacement.ForDay(_dailyTypes, nextDay);
        }
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        public void SubmitActionRpc(ElectricalAction action, uint revision, RpcParams rpcParams = default)
        {
            if (!ValidateWorker(rpcParams.Receive.SenderClientId, out _)) return;
            if (ElectricalTaskRules.TryAdvance(Kind, State, rpcParams.Receive.SenderClientId, action, revision, out var next)) PublishProgress(next);
        }
    }
}
