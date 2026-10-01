using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Tasks;
using EscapeViggo.Viggo;
#endif

namespace EscapeViggo.Networking
{
    public sealed partial class DevelopmentWeekSmoke
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private IEnumerator CheckDistractions(SimpleCircuitTask task, NetworkPlayer worker, NetworkPlayer viggo)
        {
            var service = _session.Workday.GetComponent<WorkDistractions>();
            var control = FindFirstObjectByType<PowerControl>();
            var score = viggo.GetComponent<PlayerScore>();
            var settings = _session.Workday.GetComponent<ScoreService>().Settings;
            if (_week.Day == 1)
            {
                Place(new Vector3(Local == viggo ? -1 : -4, 0.03f, -12), control);
                yield return new WaitForSecondsRealtime(0.7f);
                if (Local == viggo) { Request(control); Request(control); }
                yield return Until(() => service.PowerOff, "empty power cut");
                Check(!service.State.Meaningful && score.Value == 0, "Empty power cut awarded points.");
                yield return Until(() => !service.PowerOff, "empty outage recovery");
                yield return Barrier("empty-power");
            }
            Place(new Vector3(Local == worker ? -5 : Local == viggo ? -6 : -9, 0.03f, -15), task);
            yield return new WaitForSecondsRealtime(0.7f);
            if (Local == worker) Request(task);
            yield return Until(() => task.State.Phase == WorkTaskPhase.InProgress, "distraction task begin");
            if (Local == viggo) { Request(task); Request(task); }
            yield return new WaitForSecondsRealtime(0.5f);
            Check(task.State.Phase == WorkTaskPhase.InProgress && score.Value == 0, "Empty interruption awarded points.");
            yield return Barrier("distraction-empty");
            if (Local == worker) task.SubmitElementRpc(1, task.State.Revision);
            yield return Until(() => task.State.Progress == 1, "interruptible progress");
            if (Local == viggo) Place(new Vector3(-10, 0.03f, -15), task);
            yield return new WaitForSecondsRealtime(0.6f);
            if (Local == viggo) Request(task);
            yield return new WaitForSecondsRealtime(0.5f);
            Check(task.State.Progress == 1 && score.Value == 0, "Distant interruption succeeded.");
            yield return Barrier("distraction-range");
            if (Local == viggo) Place(new Vector3(-6, 0.03f, -15), task);
            yield return new WaitForSecondsRealtime(0.6f);
            if (Local == viggo) { Request(task); Request(task); }
            yield return Until(() => task.State.Phase == WorkTaskPhase.Available && score.Value == settings.WorkInterruptionPoints, "meaningful interruption");
            Check(task.State.Progress == 0 && !task.GetComponent<WorkTaskPanel>().IsOpen, "Interrupted work/UI did not clear.");
            if (Local == worker) Request(task);
            yield return new WaitForSecondsRealtime(0.4f);
            Check(task.State.Phase == WorkTaskPhase.Available, "Retry delay bypassed.");
            yield return Barrier("distraction-retry");
            yield return Until(() => _session.Manager.ServerTime.Time >= task.State.InterruptedUntil, "retry recovery");
            if (Local == worker) Request(task);
            yield return Until(() => task.State.Phase == WorkTaskPhase.InProgress, "retry begins");
            if (Local == worker) task.SubmitElementRpc(1, task.State.Revision);
            yield return Until(() => task.State.Progress == 1, "retry progress");
            if (Local == viggo) Request(task);
            yield return new WaitForSecondsRealtime(0.5f);
            Check(task.State.Progress == 1 && score.Value == settings.WorkInterruptionPoints, "Interrupt cooldown bypassed.");
            yield return Barrier("distraction-cooldown");
            // Route behind the worker: network interpolation through their sight line legitimately cancels work.
            if (Local != worker) Place(new Vector3(-8, 0.03f, -17));
            yield return new WaitForSecondsRealtime(0.5f);
            if (Local != worker) Place(new Vector3(Local == viggo ? -1 : 0, 0.03f, -17));
            yield return new WaitForSecondsRealtime(0.5f);
            if (Local != worker) Place(new Vector3(Local == viggo ? -1 : 0, 0.03f, -12), control);
            yield return new WaitForSecondsRealtime(0.6f);
            if (Local != worker && Local != viggo) Request(control);
            yield return new WaitForSecondsRealtime(0.4f);
            Check(!service.PowerOff, "Electrician cut power.");
            yield return Barrier("power-role");
            yield return Until(() => _session.Manager.ServerTime.Time >= service.State.PowerReadyAt, "power cooldown expires");
            yield return Barrier("power-ready");
            Check(task.State.Progress == 1, "Power fixture lost worker line of sight before the cut.");
            Debug.Log($"EV_POWER REQUEST day={_week.Day} local={Local.OwnerClientId} role={Local.Role} canUse={service.CanUsePower(Local.GetComponent<Interaction.PlayerInteraction>())} now={_session.Manager.ServerTime.Time} state={service.State.PowerUntil}/{service.State.PowerReadyAt} task={task.State.Phase}/{task.State.Progress} clock={_session.Workday.State.ClockText}");
            if (Local == viggo) { Request(control); Request(control); }
            yield return Until(() => service.PowerOff && task.State.Phase == WorkTaskPhase.Available && score.Value == settings.WorkInterruptionPoints + settings.PowerCutPoints, "meaningful power cut");
            Check(task.State.Progress == 0 && !task.GetComponent<WorkTaskPanel>().IsOpen, "Power cut did not close/reset work.");
            if (Local == worker) Request(task);
            else if (Local != viggo) Request(control);
            yield return new WaitForSecondsRealtime(0.5f);
            Check(service.PowerOff && task.State.Phase == WorkTaskPhase.Available, "Outage/safety delay bypassed.");
            yield return Barrier("power-delay");
            if (_week.Day % 2 == 0 && _peers > 2)
            {
                yield return Until(() => _session.Manager.ServerTime.Time >= service.State.RestoreAt, "restore safety delay");
                if (Local != worker && Local != viggo) Request(control);
                yield return Until(() => !service.PowerOff, "electrician restoration", 2);
            }
            else yield return Until(() => !service.PowerOff, "automatic restoration", 8);
            if (Local == viggo) Request(control);
            yield return new WaitForSecondsRealtime(0.5f);
            Check(!service.PowerOff && score.Value == settings.WorkInterruptionPoints + settings.PowerCutPoints, "Power cooldown bypassed.");
            yield return Barrier("power-recovered");
            Debug.Log($"EV_DISTRACTIONS PASSED day={_week.Day} score={score.Value} recovery={(_week.Day % 2 == 0 ? "manual" : "automatic")}");
        }
#endif
    }
}
