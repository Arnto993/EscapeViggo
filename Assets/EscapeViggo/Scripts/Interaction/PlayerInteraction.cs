using EscapeViggo.Networking;
using EscapeViggo.Player;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Interaction
{
    public sealed class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField, Min(0.1f)] private float _range = 3;
        private PlayerInputReader _input;
        private PlayerMovement _movement;
        private CharacterController _controller;
        private PlayerCapture _capture;
        private NetworkInteractable _heldTarget;
        private float _nextHoldPulse;
        private readonly RaycastHit[] _rayHits = new RaycastHit[32];
        public NetworkPlayer Player { get; private set; }
        public NetworkInteractable Target { get; private set; }
        public string Prompt => Target == null ? string.Empty : "E - " + Target.GetPrompt(this);
        public float Range => _range;
        public bool CanAct => !Player.GameplayStopped && (_capture == null || !_capture.IsCaptured);
        public Vector3 ServerEyePosition => transform.position + Vector3.up *
            ((Player.IsCrouching ? _movement.Settings.CrouchingHeight : _movement.Settings.StandingHeight) - _movement.Settings.EyeInset);

        private void Awake()
        {
            _input = GetComponent<PlayerInputReader>();
            _movement = GetComponent<PlayerMovement>();
            Player = GetComponent<NetworkPlayer>();
            _controller = GetComponent<CharacterController>();
            _capture = GetComponent<PlayerCapture>();
        }

        private void Update()
        {
            RefreshTarget();
            if (_heldTarget != null && (Target != _heldTarget || !_input.InteractHeld))
            {
                EndInteractionRpc(new NetworkObjectReference(_heldTarget.NetworkObject));
                _heldTarget = null;
            }
            if (Target != null && _input.InteractPressed) Interact();
            else if (_heldTarget != null && Time.unscaledTime >= _nextHoldPulse) SendHoldPulse();
        }

        public void RefreshTarget()
        {
            Target = null;
            if (!IsSpawned || !IsOwner || !CanAct || !_input.IsCaptured || !_input.enabled) return;
            var target = Cast(_camera.transform.position, _camera.transform.forward, _range);
            if (target != null && target.CanInteract(this)) Target = target;
        }

        public void Interact()
        {
            if (IsSpawned && IsOwner && Target != null)
            {
                RequestInteractionRpc(new NetworkObjectReference(Target.NetworkObject), _camera.transform.forward);
                if (Target.RequiresHold(this))
                {
                    _heldTarget = Target;
                    _nextHoldPulse = Time.unscaledTime + 0.1f;
                }
            }
        }

        private void SendHoldPulse()
        {
            _nextHoldPulse = Time.unscaledTime + 0.1f;
            RequestInteractionRpc(new NetworkObjectReference(_heldTarget.NetworkObject), _camera.transform.forward);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void EndInteractionRpc(NetworkObjectReference reference, RpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId == OwnerClientId && reference.TryGet(out var target, NetworkManager))
                target.GetComponent<NetworkInteractable>()?.EndOnServer(this);
        }

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void RequestInteractionRpc(NetworkObjectReference reference, Vector3 aimDirection, RpcParams rpcParams = default)
        {
            if (!CanAct || rpcParams.Receive.SenderClientId != OwnerClientId || !reference.TryGet(out var targetObject, NetworkManager)) return;
            var target = targetObject.GetComponent<NetworkInteractable>();
            if (target == null || !ServerCanReach(target) || !InteractionQuery.IsFinite(aimDirection)) return;
            // Origin and range come from server state. Only aim direction is supplied by the owner.
            if (Cast(ServerEyePosition, aimDirection, _range) != target) return;
            target.InteractOnServer(this);
        }

        public bool ServerCanReach(NetworkInteractable target)
        {
            if (!IsServer || !IsSpawned || !CanAct || target == null || !target.IsSpawned) return false;
            var origin = ServerEyePosition;
            var point = target.InteractionCollider.ClosestPoint(origin);
            if ((point - origin).sqrMagnitude > _range * _range) return false;
            return Cast(origin, target.InteractionCollider.bounds.center - origin,
                Vector3.Distance(origin, target.InteractionCollider.bounds.center) + 0.01f) == target;
        }

        private NetworkInteractable Cast(Vector3 origin, Vector3 direction, float range) =>
            InteractionQuery.Raycast(origin, direction, range, _controller, _rayHits);



        public override void OnNetworkDespawn() { Target = null; _heldTarget = null; }
    }
}
