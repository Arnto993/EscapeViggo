using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    [RequireComponent(typeof(ViggoStamina))]
    public sealed class ViggoRestFeedback : NetworkBehaviour
    {
        private ViggoStamina _stamina;
        public uint JavellEvents { get; private set; }

        public override void OnNetworkSpawn()
        {
            _stamina = GetComponent<ViggoStamina>();
            _stamina.RestStarted += OnRestStarted;
        }

        private void OnRestStarted()
        {
            JavellEvents++;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Viggo {OwnerClientId}: Javell. Rest { _stamina.State.RestCount}.", this);
#endif
        }



        public override void OnNetworkDespawn() => _stamina.RestStarted -= OnRestStarted;
    }
}
