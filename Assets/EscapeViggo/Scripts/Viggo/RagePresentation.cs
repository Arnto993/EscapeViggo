using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    [RequireComponent(typeof(ViggoRage))]
    public sealed class RagePresentation : NetworkBehaviour
    {
        private ViggoRage _rage;
        private void Awake() => _rage = GetComponent<ViggoRage>();
        private void OnGUI()
        {
            if (!IsSpawned || !_rage.Active || !NetworkManager.IsConnectedClient) return;
            var local = NetworkManager.LocalClient.PlayerObject;
            if (local == null) return;
            var viggo = local.GetComponent<NetworkPlayer>().Role == PlayerRole.Viggo;
            GUI.Box(new Rect(Screen.width / 2f - 225, 25, 450, 35), viggo ?
                "BATTERY MISSING! RAGE - pursue without resting" : "WARNING: VIGGO IS RAGING!");
        }
    }
}
