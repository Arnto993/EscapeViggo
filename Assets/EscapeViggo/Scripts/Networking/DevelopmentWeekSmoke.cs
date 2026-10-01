using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using EscapeViggo.Battery;
using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Player;
using EscapeViggo.Scoring;
using EscapeViggo.Tasks;
using EscapeViggo.Viggo;
using Unity.Netcode;
#endif

namespace EscapeViggo.Networking
{
    public sealed partial class DevelopmentWeekSmoke : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private LocalNetworkSession _session;
        private WorkWeek _week;
        private string _directory;
        private int _peers;
        private NetworkPlayer Local => _session.Manager.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
        private NetworkPlayer[] Players => FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None).OrderBy(p => p.OwnerClientId).ToArray();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!DevelopmentNetworkPanel.HasArgument("-ev-week-smoke")) return;
            FindFirstObjectByType<LocalNetworkSession>().gameObject.AddComponent<DevelopmentWeekSmoke>();
        }
        private void Awake()
        {
            _session = GetComponent<LocalNetworkSession>();
            _directory = DevelopmentNetworkPanel.Argument("-ev-week-sync-dir");
            _peers = int.Parse(DevelopmentNetworkPanel.Argument("-ev-week-peers") ?? "3");
            Application.logMessageReceived += OnLog;
        }
        private void OnLog(string message, string stack, LogType type)
        {
            if (type == LogType.Exception || type == LogType.Error) Application.Quit(1);
        }
        private void OnDestroy() => Application.logMessageReceived -= OnLog;
        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("EV_WEEK FAILED: " + message);
        }
        private IEnumerator Until(Func<bool> condition, string description, float seconds = 60)
        {
            var deadline = Time.realtimeSinceStartup + seconds;
            while (!condition())
            {
                Check(Time.realtimeSinceStartup < deadline, "Timeout: " + description);
                yield return null;
            }
        }
        private IEnumerator Barrier(string step)
        {
            var stepDirectory = Path.Combine(_directory, $"day{_week.Day}-{step}");
            Directory.CreateDirectory(stepDirectory);
            File.WriteAllText(Path.Combine(stepDirectory, Local.OwnerClientId + ".ready"), "ready");
            yield return Until(() => Directory.GetFiles(stepDirectory, "*.ready").Length == _peers, step);
        }
        private void Place(Vector3 position, NetworkInteractable target = null)
        {
            Local.GetComponent<PlayerMovement>().enabled = false;
            Local.GetComponent<PlayerLook>().enabled = false;
            var controller = Local.GetComponent<CharacterController>();
            controller.enabled = false;
            Local.transform.SetPositionAndRotation(position, Quaternion.identity);
            controller.enabled = true;
            Local.GetComponent<PlayerMovement>().Simulate(default, 1f / 60);
            var camera = Local.GetComponentInChildren<Camera>();
            var movement = Local.GetComponent<PlayerMovement>();
            camera.transform.localPosition = Vector3.up * (movement.Height - movement.Settings.EyeInset);
            if (target != null) camera.transform.LookAt(target.InteractionCollider.bounds.center);
            Physics.SyncTransforms();
        }
        private void Request(NetworkInteractable target)
        {
            var interaction = Local.GetComponent<PlayerInteraction>();
            interaction.RequestInteractionRpc(new NetworkObjectReference(target.NetworkObject), target.InteractionCollider.bounds.center - interaction.ServerEyePosition);
        }
        private IEnumerator Start()
        {
            yield return Until(() => _session.PlayerCount == _peers && _session.Workday != null, "connected players", 110);
            _week = _session.Workday.GetComponent<WorkWeek>();
            if (_session.Manager.IsServer)
            {
                Check(!_session.Workday.GetComponent<DailyResults>().Ready, "Players joined after initial workday ended.");
                _session.Workday.RestartDayOnServer(); // Explicit test fixture: start timing after all peers arrive.
            }
            yield return Barrier("initial-clock");
            var networkId = _session.Workday.NetworkObjectId;
            var connectionId = Local.OwnerClientId;
            var totals = new System.Collections.Generic.Dictionary<ulong, int>();
            ulong previousViggo = ulong.MaxValue;
            for (var day = 1; day <= 5; day++)
            {
                yield return Until(() => _week.Day == day, "day advance");
                yield return new WaitForSecondsRealtime(0.8f);
                var players = Players;
                Check(players.Length == _peers && Local.OwnerClientId == connectionId && _session.Workday.NetworkObjectId == networkId, "Session or players recreated between days.");
                var viggo = players.Single(p => p.Role == PlayerRole.Viggo);
                var worker = players.First(p => p.Role == PlayerRole.Electrician);
                Check(viggo.OwnerClientId != previousViggo, "Consecutive Viggo assignment.");
                previousViggo = viggo.OwnerClientId;
                var task = FindFirstObjectByType<SimpleCircuitTask>();
                var battery = _session.Battery;
                var rage = _session.Workday.GetComponent<ViggoRage>();
                Check(task.State.Phase == WorkTaskPhase.Available && task.State.Progress == 0, "Task did not reset.");
                Check(battery.State.Phase == BatteryPhase.AtDrill && battery.State.CarrierId == BatteryState.Nobody &&
                    battery.State.OriginalThiefId == BatteryState.Nobody && !battery.State.Discovered && !rage.Active, "Battery/Rage reset failed.");
                Check(players.All(p => !p.GetComponent<PlayerCapture>().IsCaptured && p.GetComponent<PlayerScore>().Value == 0), "Capture/daily score reset failed.");
                Check(!viggo.GetComponent<ViggoStamina>().IsResting && viggo.GetComponent<ViggoStamina>().Value == viggo.GetComponent<ViggoStamina>().Settings.MaximumStamina, "Stamina/rest reset failed.");
                if (day > 1)
                {
                    Check(_session.Workday.State.ClockText == "07:00", "New day clock did not reset.");
                    if (_session.Manager.IsServer)
                        foreach (var player in players) Check(Vector3.Distance(player.transform.position, _session.SpawnPointFor(player.OwnerClientId).position) < 0.3f, "Rotated player spawn mismatch.");
                    Check(!Local.GetComponent<PlayerInputReader>().IsTaskModal && !task.GetComponent<SimpleCircuitTaskPanel>().IsOpen, "Stale task UI.");
                }
                Check(!_week.ContinueOnServer(), "Week advanced during gameplay or on client.");
                var distractions = _session.Workday.GetComponent<WorkDistractions>();
                Check(!distractions.PowerOff && distractions.State.InterruptReadyAt == 0 && distractions.State.PowerReadyAt == 0, "Distractions did not reset.");
                yield return Barrier("reset");
                yield return Until(() => _session.Workday.State.Phase == MatchPhase.Workday, "workday");
                yield return CheckDistractions(task, worker, viggo);
                var distractionReward = viggo.GetComponent<PlayerScore>().Value;
                Place(new Vector3(Local == worker ? -5 : Local == viggo ? -6 : -9, 0.03f, -15), task);
                yield return new WaitForSecondsRealtime(0.7f);
                if (Local == viggo) { Request(task); task.SubmitElementRpc(1, task.State.Revision); }
                yield return Barrier("role-rejection");
                Check(task.State.Phase == WorkTaskPhase.Available, "Viggo started work.");
                yield return Barrier("role-checked");
                if (Local == worker)
                {
                    Request(task);
                    yield return Until(() => task.State.Phase == WorkTaskPhase.InProgress, "begin circuit");
                    var revision = task.State.Revision;
                    task.SubmitElementRpc(2, revision);
                    yield return Until(() => task.State.Revision > revision, "incorrect circuit input");
                    Check(task.State.Progress == 0 && task.State.WrongInput, "Incorrect input accepted.");
                    foreach (var element in new[] { 1, 3, 2 })
                    {
                        revision = task.State.Revision;
                        task.SubmitElementRpc(element, revision); task.SubmitElementRpc(element, revision);
                        yield return Until(() => task.State.Revision > revision, "circuit step");
                    }
                }
                yield return Until(() => task.State.Phase == WorkTaskPhase.Completed, "replicated circuit");
                var reward = _session.Workday.GetComponent<ScoreService>().Settings.CircuitTaskPoints;
                yield return Until(() => worker.GetComponent<PlayerScore>().Value == reward, "task reward");
                yield return Barrier("task");
                var electricalTasks = FindObjectsByType<ElectricalWorkTask>(FindObjectsSortMode.None).OrderBy(t => t.NetworkObjectId).ToArray();
                var completedTypes = electricalTasks.GroupBy(t => t.Kind).Select(group => group.First()).ToArray();
                foreach (var electrical in completedTypes)
                {
                    yield return CheckElectricalTask(electrical, worker, viggo);
                    reward += electrical.Reward(_session.Workday.GetComponent<ScoreService>().Settings);
                }
                var unfinished = electricalTasks.FirstOrDefault(t => !completedTypes.Contains(t));
                var observer = players.FirstOrDefault(p => p != worker && p != viggo);
                if (unfinished != null && observer != null)
                {
                    if (Local == observer)
                    {
                        Place(new Vector3(unfinished.transform.position.x, 0.03f, unfinished.transform.position.z - 2), unfinished);
                        yield return new WaitForSecondsRealtime(0.5f);
                        Request(unfinished);
                        yield return Until(() => unfinished.State.Phase == WorkTaskPhase.InProgress, "unfinished task begin");
                        unfinished.SubmitActionRpc(ActionsFor(unfinished.Kind)[0], unfinished.State.Revision);
                    }
                    yield return Until(() => unfinished.State.Progress == 1, "unfinished task progress");
                }
                if (unfinished == null || Local != observer) Place(new Vector3(Local == worker ? 17 : Local == viggo ? 16 : 13, 0.03f, 5.5f), battery);
                yield return new WaitForSecondsRealtime(0.8f);
                if (Local == worker)
                {
                    var deadline = Time.realtimeSinceStartup + 8;
                    while (battery.State.Phase != BatteryPhase.Carried)
                    {
                        Check(Time.realtimeSinceStartup < deadline, "Battery theft timed out.");
                        Request(battery); yield return new WaitForSecondsRealtime(0.1f);
                    }
                }
                yield return Until(() => battery.State.Phase == BatteryPhase.Carried, "battery replication");
                Check(!rage.Active, "Theft discovered automatically.");
                yield return Barrier("theft");
                if (Local == viggo) Request(battery);
                yield return Until(() => rage.Active, "rotated Viggo discovery");
                yield return Barrier("rage");
                var captureReward = 0;
                if (day % 2 == 0)
                {
                    var target = worker.GetComponent<PlayerCapture>();
                    if (_session.Manager.IsServer)
                    {
                        // Opt-in runtime fixture: leave timeout/rest active through cutoff to test cleanup.
                        target.Settings.DisableSeconds = 120;
                        viggo.GetComponent<ViggoStamina>().Settings.RestSeconds = 120;
                    }
                    if (unfinished == null || Local != observer) Place(new Vector3(Local == worker ? -13 : Local == viggo ? -14.5f : -19, 0.03f, -12), target);
                    yield return new WaitForSecondsRealtime(0.8f);
                    if (Local == viggo) { Request(target); Request(target); }
                    yield return Until(() => target.IsCaptured && !rage.Active, "rotated Viggo capture and recovery");
                    var settings = _session.Workday.GetComponent<ScoreService>().Settings;
                    captureReward = settings.CapturePoints + settings.CarrierCaptureBonus;
                    reward -= settings.CapturedPenalty;
                    yield return Until(() => viggo.GetComponent<PlayerScore>().Value == captureReward + distractionReward && worker.GetComponent<PlayerScore>().Value == reward, "capture score");
                    if (Local == viggo)
                    {
                        var untilRest = Time.realtimeSinceStartup + 9;
                        var started = Time.realtimeSinceStartup;
                        while (!viggo.GetComponent<ViggoStamina>().IsResting)
                        {
                            Check(Time.realtimeSinceStartup < untilRest, "Viggo did not exhaust.");
                            var direction = (int)(Time.realtimeSinceStartup - started) % 2 == 0 ? 1 : -1;
                            Local.GetComponent<PlayerMovement>().Simulate(new MovementCommand { Move = new Vector2(direction, 0) }, Time.deltaTime);
                            yield return null;
                        }
                    }
                    yield return Until(() => viggo.GetComponent<ViggoStamina>().IsResting, "replicated Javell rest");
                    yield return Barrier("capture-rest");
                }
                // Odd days retain Rage and battery ownership; even days retain capture/rest until cutoff.
                if (day % 2 == 1 && unfinished != null && observer != null)
                {
                    var control = FindFirstObjectByType<PowerControl>();
                    if (Local == viggo) Place(new Vector3(-1, 0.03f, -12), control);
                    yield return Until(() => _session.Workday.State.MinuteOfDay >= 14 * 60 + 40, "late outage fixture");
                    if (Local == viggo) Request(control);
                    yield return Until(() => distractions.PowerOff, "outage before cutoff");
                    distractionReward += _session.Workday.GetComponent<ScoreService>().Settings.PowerCutPoints;
                }
                yield return Until(() => _session.Workday.GetComponent<DailyResults>().Ready, "daily results");
                yield return new WaitForSecondsRealtime(0.4f);
                Check(!rage.Active && _session.Workday.GetComponent<DailyResultsPanel>().IsOpen, "Results/Rage cutoff failed.");
                Check(!distractions.PowerOff, "Power outage survived cutoff.");
                var results = _session.Workday.GetComponent<DailyResults>();
                Check(results.Count == _peers && results.Summary.CarrierId == (day % 2 == 0 ? BatteryState.Nobody : worker.OwnerClientId), "Frozen daily results mismatch.");
                Check(!worker.GetComponent<PlayerCapture>().IsCaptured, "EndOfDay did not clear capture.");
                if (unfinished != null && observer != null)
                    Check(unfinished.State.Phase == WorkTaskPhase.Available && !unfinished.GetComponent<WorkTaskPanel>().IsOpen, "Unfinished task/UI did not stop at EndOfDay.");
                totals.TryGetValue(worker.OwnerClientId, out var prior);
                totals[worker.OwnerClientId] = prior + reward;
                totals.TryGetValue(viggo.OwnerClientId, out var viggoPrior);
                totals[viggo.OwnerClientId] = viggoPrior + captureReward + distractionReward;
                foreach (var player in players)
                {
                    totals.TryGetValue(player.OwnerClientId, out var expected);
                    Check(_week.TotalFor(player.OwnerClientId).Score == expected, "Weekly accumulation mismatch.");
                }
                Debug.Log($"EV_WEEK_DAY {day} viggo={viggo.OwnerClientId} rows=" + string.Join(";", Enumerable.Range(0, _week.Count).Select(i => _week.RowAt(i).ToString())));
                yield return Barrier("results");
                if (day < 5 && _session.Manager.IsServer)
                {
                    yield return Until(() => _week.CanContinue, "results review delay");
                    Check(_week.ContinueOnServer(), "Host could not continue.");
                    Check(!_week.ContinueOnServer(), "Duplicate continue advanced twice.");
                }
            }
            Check(_week.IsFinal && !_week.ContinueOnServer() && _week.Day == 5, "Friday did not stop at final results.");
            yield return Barrier("final");
            var frozen = string.Join(";", Enumerable.Range(0, _week.Count).Select(i => _week.RowAt(i).ToString()));
            var rejoinMarker = Path.Combine(_directory, "friday-rejoin-passed");
            if (Local.OwnerClientId == Players.Max(p => p.OwnerClientId))
            {
                _session.Disconnect();
                yield return Until(() => _session.CanStart, "Friday disconnect");
                if (DevelopmentNetworkPanel.HasArgument("-ev-online"))
                {
                    var joining = _session.Online.JoinAsync(DevelopmentNetworkPanel.Argument("-ev-join-code"));
                    yield return Until(() => joining.IsCompleted, "Friday Relay rejoin");
                }
                else Check(_session.StartClient("127.0.0.1", ushort.Parse(DevelopmentNetworkPanel.Argument("-ev-port"))), "Friday rejoin failed.");
                yield return Until(() => _session.Workday != null && _session.Workday.GetComponent<WorkWeek>().IsFinal, "Friday frozen week late join");
                _week = _session.Workday.GetComponent<WorkWeek>();
                yield return new WaitForSecondsRealtime(0.4f);
                Check(frozen == string.Join(";", Enumerable.Range(0, _week.Count).Select(i => _week.RowAt(i).ToString())), "Friday reconnect changed weekly results.");
                File.WriteAllText(rejoinMarker, "passed");
            }
            yield return Until(() => File.Exists(rejoinMarker), "peer Friday rejoin");
            Debug.Log("EV_WEEK PASSED: five days, same connection, rotation/spawns, task rewards, battery/Rage, daily reset and weekly results.");
            yield return new WaitForSecondsRealtime(1);
            _session.Disconnect();
            yield return Until(() => _session.CanStart, "cleanup");
            Application.Quit(0);
        }
#endif
    }
}
