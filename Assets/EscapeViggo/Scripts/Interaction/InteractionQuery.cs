using UnityEngine;

namespace EscapeViggo.Interaction
{
    public static class InteractionQuery
    {
        public static NetworkInteractable Raycast(Vector3 origin, Vector3 direction, float range)
        {
            if (!IsFinite(direction) || direction.sqrMagnitude < 0.001f || range <= 0) return null;
            return Physics.Raycast(origin, direction.normalized, out var hit, range, ~0, QueryTriggerInteraction.Ignore)
                ? hit.collider.GetComponentInParent<NetworkInteractable>() : null;
        }

        public static bool IsFinite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
