using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    [RequireComponent(typeof(ViggoStamina))]
    public sealed class ViggoRestFeedback : NetworkBehaviour
    {
        private ViggoStamina _stamina;
        private float _messageUntil;
        public uint JavellEvents { get; private set; }

        public override void OnNetworkSpawn()
        {
            _stamina = GetComponent<ViggoStamina>();
            _stamina.RestStarted += OnRestStarted;
        }

        private void OnRestStarted()
        {
            JavellEvents++;
            _messageUntil = Time.unscaledTime + 3;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Viggo {OwnerClientId}: Javell. Rest { _stamina.State.RestCount}.", this);
#endif
        }

        private void OnGUI()
        {
            if (!IsSpawned || !IsOwner || !_stamina.Applies || GetComponent<Networking.NetworkPlayer>().GameplayStopped) return;
            if (_stamina.IsResting)
                GUI.Box(new Rect(Screen.width / 2f - 150, 70, 300, 30), $"Resting: {_stamina.State.RestRemaining:F1}s");
            if (Time.unscaledTime < _messageUntil)
                GUI.Box(new Rect(Screen.width / 2f - 100, 105, 200, 30), "Javell.");
        }

        public override void OnNetworkDespawn() => _stamina.RestStarted -= OnRestStarted;
    }
}
