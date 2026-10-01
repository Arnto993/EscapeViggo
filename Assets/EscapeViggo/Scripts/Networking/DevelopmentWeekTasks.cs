#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Tasks;
using UnityEngine;

namespace EscapeViggo.Networking
{
    public sealed partial class DevelopmentWeekSmoke
    {
        private static ElectricalAction[] ActionsFor(ElectricalTaskKind kind) => kind switch
        {
            ElectricalTaskKind.VoltageMeasurement => new[] { ElectricalAction.AcVoltage, ElectricalAction.BlackNeutral, ElectricalAction.RedLive, ElectricalAction.Reading230 },
            ElectricalTaskKind.BreakerInstallation => new[] { ElectricalAction.Breaker16, ElectricalAction.Slot2, ElectricalAction.ConnectLoad, ElectricalAction.ConnectSupply, ElectricalAction.ToggleOn, ElectricalAction.ConfirmBreaker },
            _ => new[] { ElectricalAction.RemoveFaceplate, ElectricalAction.DisconnectEarth, ElectricalAction.DisconnectBrown, ElectricalAction.DisconnectBlue,
                ElectricalAction.SelectBlue, ElectricalAction.TerminalNeutral, ElectricalAction.SelectEarth, ElectricalAction.TerminalEarth,
                ElectricalAction.SelectBrown, ElectricalAction.TerminalLive, ElectricalAction.InstallOutlet, ElectricalAction.ReplaceFaceplate }
        };
        private IEnumerator CheckElectricalTask(ElectricalWorkTask task, NetworkPlayer worker, NetworkPlayer viggo)
        {
            var key = "electrical-" + task.NetworkObjectId;
            Check(task.State.Phase == WorkTaskPhase.Available && task.State.Progress == 0 && task.State.Connections == 0, "Electrical task did not reset.");
            Check(task.Kind == task.TypeForDay(_week.Day), "Daily task placement differs from configured layout.");
            var oldScore = worker.GetComponent<PlayerScore>().Value;
            var oldTasks = worker.GetComponent<PlayerScore>().TasksCompleted;
            var expected = task.Reward(_session.Workday.GetComponent<ScoreService>().Settings);
            var actions = ActionsFor(task.Kind);
            Place(new Vector3(task.transform.position.x + (Local == worker ? 0 : Local == viggo ? -1 : 1), 0.03f, task.transform.position.z - 2), task);
            yield return new WaitForSecondsRealtime(0.6f);
            if (Local == viggo) { Request(task); task.SubmitActionRpc(actions[0], task.State.Revision); }
            yield return Barrier(key + "-role");
            Check(task.State.Phase == WorkTaskPhase.Available, $"Viggo rejection failed: phase={task.State.Phase}, worker={task.State.WorkerId}, localRole={Local.Role}.");
            yield return Barrier(key + "-role-checked");
            if (Local == worker) Request(task);
            yield return Until(() => task.State.Phase == WorkTaskPhase.InProgress, "electrical task begin");
            if (Local != worker) { Request(task); task.SubmitActionRpc(actions[0], task.State.Revision); }
            yield return new WaitForSecondsRealtime(0.6f); // Let rejection requests traverse Relay before cancellation.
            yield return Barrier(key + "-contention");
            Check(task.State.WorkerId == worker.OwnerClientId && task.State.Progress == 0, "Task stolen by simultaneous request.");
            yield return Barrier(key + "-contention-checked");
            if (Local == worker)
            {
                Check(task.GetComponent<WorkTaskPanel>().IsOpen, "Electrical UI missing.");
                task.CancelRpc(task.State.Revision);
            }
            yield return Until(() => task.State.Phase == WorkTaskPhase.Available, "electrical cancellation");
            yield return Barrier(key + "-cancel");
            if (Local == worker)
            {
                Check(!task.GetComponent<WorkTaskPanel>().IsOpen, "Cancel left task UI open.");
                Debug.Log($"EV_ELECTRICAL REOPEN kind={task.Kind} phase={task.State.Phase} worker={task.State.WorkerId} clock={_session.Workday.State.ClockText}");
                Request(task);
                yield return Until(() => task.State.Phase == WorkTaskPhase.InProgress && task.State.WorkerId == worker.OwnerClientId, "electrical reopen");
                var revision = task.State.Revision;
                task.SubmitActionRpc(ElectricalAction.ReadingLow, revision);
                yield return Until(() => task.State.Revision > revision, "electrical wrong input");
                Check(task.State.Progress == 0 && task.State.WrongInput, "Incorrect electrical input advanced task.");
                foreach (var action in actions)
                {
                    revision = task.State.Revision;
                    task.SubmitActionRpc(action, revision); task.SubmitActionRpc(action, revision);
                    yield return Until(() => task.State.Revision > revision, "electrical input " + action);
                }
                task.SubmitActionRpc(actions[actions.Length - 1], task.State.Revision);
                Request(task);
            }
            yield return Until(() => task.State.Phase == WorkTaskPhase.Completed && worker.GetComponent<PlayerScore>().Value == oldScore + expected, "electrical completion/reward");
            Check(worker.GetComponent<PlayerScore>().TasksCompleted == oldTasks + 1 && task.State.WorkerId == worker.OwnerClientId, "Electrical reward recipient/count mismatch.");
            if (Local == worker) Check(!task.GetComponent<WorkTaskPanel>().IsOpen, "Electrical UI remained open.");
            Debug.Log($"EV_ELECTRICAL PASSED day={_week.Day} kind={task.Kind} reward={expected}");
            yield return Barrier(key + "-complete");
        }
    }
}
#endif
