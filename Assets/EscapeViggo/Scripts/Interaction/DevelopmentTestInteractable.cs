using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Interaction
{
    public sealed class DevelopmentTestInteractable : NetworkInteractable
    {
        [SerializeField] private TextMesh _label;
        private readonly NetworkVariable<int> _count = new(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public int Count => _count.Value;
        public override string Prompt => $"Test interaction ({Count})";
        protected override void OnServerInteract(PlayerInteraction player) => _count.Value++;
        public override void OnNetworkSpawn()
        {
            _count.OnValueChanged += Changed;
            Changed(0, Count);
        }
        private void Changed(int previous, int current)
        {
            if (_label != null) _label.text = $"TEST INTERACTION\nConfirmed: {current}";
        }
        public override void OnNetworkDespawn() => _count.OnValueChanged -= Changed;
    }
}
