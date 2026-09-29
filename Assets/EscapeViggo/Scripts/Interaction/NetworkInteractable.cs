using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Interaction
{
    [RequireComponent(typeof(NetworkObject), typeof(BoxCollider))]
    public abstract class NetworkInteractable : NetworkBehaviour
    {
        private Collider _interactionCollider;
        public virtual Collider InteractionCollider => _interactionCollider != null ? _interactionCollider : _interactionCollider = GetComponent<Collider>();
        public abstract string Prompt { get; }
        public virtual string GetPrompt(PlayerInteraction player) => Prompt;
        public virtual bool RequiresHold(PlayerInteraction player) => false;
        public virtual bool CanInteract(PlayerInteraction player) => IsSpawned;
        internal virtual void EndOnServer(PlayerInteraction player) { }

        internal void InteractOnServer(PlayerInteraction player)
        {
            if (IsServer && IsSpawned && CanInteract(player)) OnServerInteract(player);
        }

        protected abstract void OnServerInteract(PlayerInteraction player);
    }
}
