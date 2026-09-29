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

        public static NetworkInteractable Raycast(Vector3 origin, Vector3 direction, float range, Collider ignoredCollider, RaycastHit[] hits)
        {
            if (!IsFinite(direction) || direction.sqrMagnitude < 0.001f || range <= 0) return null;
            var count = Physics.RaycastNonAlloc(origin, direction.normalized, hits, range, ~0, QueryTriggerInteraction.Collide);
            // An overflow may have omitted a closer blocker; fail closed.
            if (count == hits.Length) return null;
            var closest = float.PositiveInfinity;
            Collider collider = null;
            for (var i = 0; i < count; i++)
            {
                var candidate = hits[i].collider;
                if ((ignoredCollider != null && (candidate == ignoredCollider || candidate.transform.IsChildOf(ignoredCollider.transform))) ||
                    hits[i].distance >= closest) continue;
                if (candidate.isTrigger && candidate.GetComponentInParent<NetworkInteractable>() == null) continue;
                closest = hits[i].distance;
                collider = hits[i].collider;
            }
            return collider == null ? null : collider.GetComponentInParent<NetworkInteractable>();
        }

        public static bool IsFinite(Vector3 value) => float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
    }
}
