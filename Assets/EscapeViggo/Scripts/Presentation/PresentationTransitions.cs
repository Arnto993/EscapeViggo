using EscapeViggo.Battery;
using EscapeViggo.Tasks;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    public static class PresentationTransitions
    {
        public static AudioCue? Task(WorkTaskState previous, WorkTaskState current)
        {
            if (current.Revision <= previous.Revision) return null;
            if (current.Phase == WorkTaskPhase.Completed && previous.Phase != WorkTaskPhase.Completed) return AudioCue.TaskCompleted;
            if (current.Phase != WorkTaskPhase.InProgress) return null; // Cancel/day reset is not a failure.
            return current.WrongInput ? AudioCue.TaskFailed : AudioCue.Tool;
        }
        public static AudioCue? Battery(BatteryState previous, BatteryState current)
        {
            if (current.Phase == BatteryPhase.Secured && previous.Phase != current.Phase ||
                previous.Phase == BatteryPhase.Carried && current.Phase == BatteryPhase.AtDrill && current.OriginalThiefId != BatteryState.Nobody)
                return AudioCue.BatteryRecovered;
            if (current.TheftId > previous.TheftId) return AudioCue.BatteryStolen;
            if (current.Phase == BatteryPhase.BeingStolen && previous.Phase != current.Phase) return AudioCue.BatteryHandling;
            return null;
        }
    }

    // Distance cadence follows rendered movement on every peer, never awards movement or writes state.
    public sealed class FootstepCadence
    {
        private Vector3 _last;
        private float _distance, _next;
        public void Reset(Vector3 position) { _last = position; _distance = _next = 0; }
        public bool Sample(Vector3 position, bool grounded, bool blocked, float stride, float interval, float now, float deltaTime)
        {
            var delta = position - _last; _last = position;
            var horizontal = new Vector2(delta.x, delta.z).magnitude;
            if (blocked || !grounded || horizontal > Mathf.Max(1, deltaTime * 20) || Mathf.Abs(delta.y) > 0.5f)
            { _distance = 0; return false; }
            _distance += horizontal;
            if (_distance < Mathf.Max(0.1f, stride) || now < _next) return false;
            _distance = 0; _next = now + interval; return true;
        }
    }
}
