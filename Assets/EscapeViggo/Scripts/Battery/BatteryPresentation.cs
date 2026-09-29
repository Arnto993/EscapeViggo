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
        private void OnGUI()
        {
            if (!IsSpawned || !NetworkManager.IsConnectedClient) return;
            var local = NetworkManager.LocalClient.PlayerObject;
            if (local == null || local.GetComponent<NetworkPlayer>().Role != PlayerRole.Electrician) return;
            if (_station.State.Phase == BatteryPhase.Carried && _station.State.CarrierId == local.OwnerClientId)
                GUI.Box(new Rect(Screen.width / 2f - 150, 150, 300, 30), "You are carrying Viggo's battery");
            else if (_station.State.Phase == BatteryPhase.BeingStolen && _station.State.ActorId == local.OwnerClientId)
                GUI.Box(new Rect(Screen.width / 2f - 150, 150, 300, 30), $"Stealing battery: {_station.State.Progress:P0}");
        }
    }
}
