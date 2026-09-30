using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Battery
{
    [RequireComponent(typeof(BatteryStation))]
    public sealed class BatteryPresentation : NetworkBehaviour
    {
        [SerializeField] private GameObject _batteryVisual;
        private BatteryStation _station;
        private void Awake() => _station = GetComponent<BatteryStation>();
        private void Update()
        {
            if (IsSpawned) _batteryVisual.SetActive(_station.State.Phase != BatteryPhase.Carried);
        }

    }
}
