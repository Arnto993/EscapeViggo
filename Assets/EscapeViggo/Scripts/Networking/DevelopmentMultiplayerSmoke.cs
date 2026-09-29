using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using EscapeViggo.Player;
using EscapeViggo.Core;
using EscapeViggo.Interaction;
using EscapeViggo.Tasks;
using EscapeViggo.Scoring;
using EscapeViggo.Viggo;
using EscapeViggo.Battery;
using Unity.Netcode;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

namespace EscapeViggo.Networking
{
    /// <summary>Opt-in, two-process development check. Absent from release behavior.</summary>
    public sealed class DevelopmentMultiplayerSmoke : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private LocalNetworkSession _session;
        private bool _failed;
        private bool _host;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!DevelopmentNetworkPanel.HasArgument("-ev-smoke") && !DevelopmentNetworkPanel.HasArgument("-ev-observer")) return;
            var session = FindFirstObjectByType<LocalNetworkSession>();
            if (session != null) session.gameObject.AddComponent<DevelopmentMultiplayerSmoke>();
        }

        private void Awake()
        {
            _session = GetComponent<LocalNetworkSession>();
            _host = DevelopmentNetworkPanel.HasArgument("-ev-host");
            Application.logMessageReceived += HandleLog;
        }

        private IEnumerator Start()
        {
            if (DevelopmentNetworkPanel.HasArgument("-ev-observer"))
            {
                yield return CheckLateJoin();
                yield break;
            }
            yield return Until(() => _session.PlayerCount == 2, "initial two players");
            yield return Round(1);
            // Leave a stable interval for both processes to record the same state.
            yield return new WaitForSecondsRealtime(2);
            if (_host)
            {
                yield return Until(() => _session.PlayerCount == 1, "client disconnect");
                Check(_session.Manager.IsHost, "Host must survive client disconnect.");
                yield return Until(() => _session.PlayerCount == 2, "client reconnect");
            }
            else
            {
                _session.Disconnect();
                yield return Until(() => _session.CanStart && _session.PlayerCount == 0, "client cleanup");
                yield return new WaitForSecondsRealtime(2);
                Check(_session.StartClient(DevelopmentNetworkPanel.Argument("-ev-address") ?? "127.0.0.1", Port()), "Client restart failed.");
                yield return Until(() => _session.PlayerCount == 2, "reconnected players");
            }
            yield return Round(2);
            yield return CheckInteraction();
            yield return CheckTask();
            yield return CheckScores();
            yield return CheckStamina();
            yield return CheckRest();
            yield return CheckBattery();
            yield return CheckRage();
            yield return Until(() => _session.Workday != null && _session.Workday.State.Phase == MatchPhase.EndOfDay, "authoritative end of day", 120);
            Check(_session.Workday.State.ClockText == "15:00", "End of day must display 15:00.");
            yield return Until(() => !_session.Workday.GetComponent<ViggoRage>().Active, "Rage ended with workday");
            Debug.Log("EV_WORKDAY END PASSED: EndOfDay 15:00");
            var scoring = _session.Workday.GetComponent<ScoreService>();
            var completedTask = FindFirstObjectByType<SimpleCircuitTask>();
            if (_host) Check(!scoring.TryAwardOnce(_session.Manager.LocalClient.PlayerObject.GetComponent<PlayerScore>(),
                completedTask.NetworkObject, 1, scoring.Settings.CircuitTaskPoints), "Scoring continued after EndOfDay.");
            yield return new WaitForSecondsRealtime(2);
            if (_host)
            {
                yield return Until(() => _session.PlayerCount == 1, "final client disconnect");
                _session.Disconnect();
                yield return Until(() => _session.CanStart && _session.PlayerCount == 0, "host cleanup");
                Check(_session.StartHost("127.0.0.1", Port()), "Host restart failed.");
                yield return Until(() => _session.PlayerCount == 1, "host respawn");
                yield return null;
                CheckOwnership();
                Debug.Log("EV_SMOKE HOST_RESTART PASSED");
            }
            _session.Disconnect();
            yield return Until(() => _session.CanStart && _session.PlayerCount == 0, "final cleanup");
            Debug.Log("EV_SMOKE PASSED " + (_host ? "HOST" : "CLIENT"));
            Application.Quit(_failed ? 1 : 0);
        }

        private IEnumerator Round(int round)
        {
            yield return null;
            yield return null;
            CheckOwnership();
            CheckRoles(round);
            Check(_session.Workday != null, "Workday did not synchronize.");
            if (round == 2) Check(_session.Workday.State.MinuteOfDay > WorkdayClock.StartMinute, "Late join reset or lost the clock.");
            Debug.Log($"EV_WORKDAY round={round} {_session.Workday.State.Phase} {_session.Workday.State.ClockText}");
            yield return new WaitForSecondsRealtime(2);
            // Keep the established M3 movement route independent of role spawn locations.
            var ownedPlayer = _session.Manager.LocalClient.PlayerObject;
            if (round == 1)
            {
                var controller = ownedPlayer.GetComponent<CharacterController>();
                controller.enabled = false;
                ownedPlayer.transform.SetPositionAndRotation(new Vector3(_host ? -15 : -13, 0.1f, -12), Quaternion.identity);
                controller.enabled = true;
                yield return new WaitForSecondsRealtime(2);
            }
            var initial = new Dictionary<ulong, Vector3>();
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
                initial.Add(player.OwnerClientId, player.transform.position);
            var local = _session.Manager.LocalClient.PlayerObject;
            var input = local.GetComponent<PlayerInputReader>();
            var movement = local.GetComponent<PlayerMovement>();
            var look = local.GetComponent<PlayerLook>();
            input.enabled = movement.enabled = look.enabled = false;
            var elapsed = 0f;
            while (elapsed < 1)
            {
                var step = Mathf.Min(Time.deltaTime, 1 - elapsed);
                movement.Simulate(new MovementCommand { Move = _host ? Vector2.up : Vector2.right }, step);
                elapsed += step;
                yield return null;
            }
            look.ApplyLook(new Vector2((_host ? 30 : 60) / movement.Settings.MouseSensitivity, 0));
            // Crouch presentation must replicate to the other process as well.
            movement.Simulate(new MovementCommand { Crouch = true }, 1f / 60);
            yield return new WaitForSecondsRealtime(3);
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                Check(Vector3.Distance(initial[player.OwnerClientId], player.transform.position) > 3,
                    $"Round {round}: player {player.OwnerClientId} did not move on this instance.");
                Check(player.IsCrouching, "Remote crouch state did not synchronize.");
                var point = player.transform.position;
                Debug.Log("EV_SNAPSHOT " + JsonUtility.ToJson(new Snapshot { round = round, owner = player.OwnerClientId,
                    x = point.x, y = point.y, z = point.z, yaw = player.transform.eulerAngles.y, crouching = player.IsCrouching,
                    role = player.Role.ToString() }));
            }
            Debug.Log($"EV_SMOKE ROUND {round} PASSED");
            var captureDirectory = DevelopmentNetworkPanel.Argument("-ev-capture-dir");
            if (round == 1 && !string.IsNullOrEmpty(captureDirectory))
                CaptureCamera(local.GetComponentInChildren<Camera>(), System.IO.Path.Combine(captureDirectory, _host ? "host.png" : "client.png"));
            yield return new WaitForSecondsRealtime(2);
            input.enabled = movement.enabled = look.enabled = true;
            input.ReleaseCursor();
        }

        private void CheckOwnership()
        {
            var owned = 0;
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                var local = player.IsOwner;
                if (local) owned++;
                Check(player.GetComponent<PlayerInputReader>().enabled == local, "Input ownership mismatch.");
                Check(player.GetComponent<PlayerMovement>().enabled == local, "Movement ownership mismatch.");
                Check(player.GetComponent<PlayerLook>().enabled == local, "Look ownership mismatch.");
                Check(player.GetComponent<CharacterController>().enabled == local, "Controller ownership mismatch.");
                Check(player.GetComponentInChildren<Camera>(true).enabled == local, "Camera ownership mismatch.");
                Check(player.GetComponentInChildren<AudioListener>(true).enabled == local, "Listener ownership mismatch.");
            }
            Check(owned == 1, "Expected exactly one owned player.");
            var cameras = 0;
            foreach (var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None)) if (camera.enabled) cameras++;
            var listeners = 0;
            foreach (var listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) if (listener.enabled) listeners++;
            Check(cameras == 1 && listeners == 1, "Expected exactly one active camera and listener.");
        }

        private void CheckRoles(int round)
        {
            var viggos = 0;
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                Check(player.Role == RoleAssignment.ForClient(player.OwnerClientId), "Role synchronization mismatch.");
                if (player.Role == PlayerRole.Viggo) viggos++;
                var body = player.transform.Find("Body");
                Check((body.localScale.x > 1) == (player.Role == PlayerRole.Viggo), "Role silhouette mismatch.");
                var roleState = (NetworkVariable<PlayerRole>)typeof(NetworkPlayer)
                    .GetField("_role", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(player);
                Check(!roleState.CanClientWrite(1), "Client must not have role write permission.");
                if (round == 1 || player.Role == PlayerRole.Electrician)
                {
                    var marker = GameObject.Find(player.Role == PlayerRole.Viggo ? "Viggo Spawn" : "Electrician Spawn");
                    Check(Vector3.Distance(player.transform.position, marker.transform.position) < 1, "Wrong role spawn.");
                }
            }
            Check(viggos == 1, "Expected exactly one Viggo.");
            Debug.Log($"EV_ROLES round={round} PASSED: assignment, authority, spawns and silhouettes.");
        }

        private IEnumerator CheckInteraction()
        {
            var target = FindFirstObjectByType<DevelopmentTestInteractable>();
            Check(target != null, "Test interactable was not spawned.");
            var local = _session.Manager.LocalClient.PlayerObject;
            var interaction = local.GetComponent<PlayerInteraction>();
            var input = local.GetComponent<PlayerInputReader>();
            var movement = local.GetComponent<PlayerMovement>();
            var look = local.GetComponent<PlayerLook>();
            movement.enabled = look.enabled = false;
            var controller = local.GetComponent<CharacterController>();
            if (!_host)
            {
                // Submit a forged distant request directly, bypassing local selection.
                controller.enabled = false;
                local.transform.position = new Vector3(-18, 0.03f, -12);
                controller.enabled = true;
                yield return new WaitForSecondsRealtime(1);
                interaction.RequestInteractionRpc(new NetworkObjectReference(target.NetworkObject), target.transform.position - interaction.ServerEyePosition);
                yield return new WaitForSecondsRealtime(1);
                Check(target.Count == 0, "Distant client interaction was accepted.");
                controller.enabled = false;
                local.transform.SetPositionAndRotation(new Vector3(-12, 0.03f, -12), Quaternion.identity);
                controller.enabled = true;
                movement.Simulate(default, 1f / 60);
                var camera = local.GetComponentInChildren<Camera>();
                camera.transform.localPosition = Vector3.up * (movement.Height - movement.Settings.EyeInset);
                camera.transform.LookAt(target.transform.position);
                input.CaptureCursor();
                yield return new WaitForSecondsRealtime(1);
                interaction.RefreshTarget();
                Check(interaction.Target == target && interaction.Prompt.Length > 0, "Aimed interaction prompt is missing.");
                interaction.Interact();
                yield return Until(() => target.Count == 1, "server interaction confirmation");
                camera.transform.rotation = Quaternion.LookRotation(Vector3.right);
                interaction.RefreshTarget();
                Check(interaction.Target == null && interaction.Prompt.Length == 0, "Prompt persists when aiming away.");
                input.ReleaseCursor();
            }
            yield return Until(() => target.Count == 1, "replicated test interaction");
            Debug.Log("EV_INTERACTION PASSED: distance rejection, aimed prompt, server confirmation, synchronization.");
            movement.enabled = look.enabled = true;
        }

        private IEnumerator CheckTask()
        {
            var task = FindFirstObjectByType<SimpleCircuitTask>();
            Check(task != null, "Circuit task was not spawned.");
            var local = _session.Manager.LocalClient.PlayerObject;
            var movement = local.GetComponent<PlayerMovement>();
            var look = local.GetComponent<PlayerLook>();
            var input = local.GetComponent<PlayerInputReader>();
            var interaction = local.GetComponent<PlayerInteraction>();
            movement.enabled = look.enabled = false;
            var controller = local.GetComponent<CharacterController>();
            controller.enabled = false;
            local.transform.SetPositionAndRotation(new Vector3(_host ? -6 : -5, 0.03f, -15), Quaternion.identity);
            controller.enabled = true;
            movement.Simulate(default, 1f / 60);
            var camera = local.GetComponentInChildren<Camera>();
            camera.transform.localPosition = Vector3.up * (movement.Height - movement.Settings.EyeInset);
            camera.transform.LookAt(task.transform.position);
            input.CaptureCursor();
            yield return new WaitForSecondsRealtime(1);
            if (_host)
            {
                Check(!task.CanInteract(interaction), "Viggo has an electrician task prompt.");
                interaction.RequestInteractionRpc(new NetworkObjectReference(task.NetworkObject), camera.transform.forward);
                task.SubmitElementRpc(1, task.State.Revision);
                Check(task.State.Phase == WorkTaskPhase.Available, "Viggo began/completed electrician task.");
            }
            else
            {
                yield return new WaitForSecondsRealtime(1);
                interaction.RefreshTarget();
                Physics.Raycast(camera.transform.position, camera.transform.forward, out var diagnosticHit, interaction.Range);
                Check(interaction.Target == task, $"Electrician cannot aim at task: captured={input.IsCaptured}, input={input.enabled}, " +
                    $"role={interaction.Player.Role}, phase={_session.Workday.State.Phase}, task={task.State.Phase}, " +
                    $"camera={camera.transform.position}, direction={camera.transform.forward}, taskPosition={task.transform.position}, " +
                    $"rayTarget={InteractionQuery.Raycast(camera.transform.position, camera.transform.forward, interaction.Range)?.name}, " +
                    $"hit={diagnosticHit.collider?.name}, colliderEnabled={task.InteractionCollider.enabled}, bounds={task.InteractionCollider.bounds}.");
                interaction.Interact();
                yield return Until(() => task.State.Phase == WorkTaskPhase.InProgress, "task opened");
                Check(task.GetComponent<SimpleCircuitTaskPanel>().IsOpen && input.IsTaskModal && !input.IsCaptured, "Task UI/control capture failed.");
                task.SubmitElementRpc(2, task.State.Revision);
                yield return Until(() => task.State.WrongInput, "wrong-input feedback");
                Check(task.State.Progress == 0, "Wrong input did not reset progress.");
                foreach (var element in new[] { 1, 3, 2 })
                {
                    var revision = task.State.Revision;
                    task.SubmitElementRpc(element, revision);
                    task.SubmitElementRpc(element, revision); // Deliberate duplicate command.
                    yield return Until(() => task.State.Revision > revision, "circuit element");
                }
                yield return Until(() => task.State.Phase == WorkTaskPhase.Completed, "task completion");
                Check(!task.GetComponent<SimpleCircuitTaskPanel>().IsOpen && !input.IsTaskModal, "Task UI did not close.");
                var completedRevision = task.State.Revision;
                interaction.RequestInteractionRpc(new NetworkObjectReference(task.NetworkObject), camera.transform.forward);
                task.SubmitElementRpc(2, completedRevision);
                yield return new WaitForSecondsRealtime(1);
                Check(task.State.Revision == completedRevision, "Completed task accepted repeated completion.");
            }
            yield return Until(() => task.State.Phase == WorkTaskPhase.Completed, "replicated task completion");
            Check(task.State.WorkerId != NetworkManager.ServerClientId && task.State.Progress == 3, "Wrong task completer or sequence.");
            Debug.Log($"EV_TASK PASSED: completedBy={task.State.WorkerId}, progress={task.State.Progress}, revision={task.State.Revision}");
            input.ReleaseCursor();
            movement.enabled = look.enabled = true;
        }

        private IEnumerator CheckScores()
        {
            var task = FindFirstObjectByType<SimpleCircuitTask>();
            var scoring = _session.Workday.GetComponent<ScoreService>();
            PlayerScore recipient = null;
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
                if (player.OwnerClientId == task.State.WorkerId) recipient = player.GetComponent<PlayerScore>();
            Check(recipient != null, "Task reward recipient is missing.");
            yield return Until(() => recipient.Value == scoring.Settings.CircuitTaskPoints, "task score replication");
            if (_host)
            {
                var viggoScore = _session.Manager.LocalClient.PlayerObject.GetComponent<PlayerScore>();
                Check(!scoring.TryAwardOnce(viggoScore, task.NetworkObject, 0, scoring.Settings.CircuitTaskPoints),
                    "Duplicate task award was redirected to Viggo.");
            }
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                var score = player.GetComponent<PlayerScore>();
                var state = (NetworkVariable<int>)typeof(PlayerScore)
                    .GetField("_score", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(score);
                Check(!state.CanClientWrite(1), "Client has authoritative score write permission.");
                Check(score.Value == (player.OwnerClientId == task.State.WorkerId ? scoring.Settings.CircuitTaskPoints : 0),
                    "Wrong score recipient or duplicated reward.");
                Debug.Log($"EV_SCORE owner={player.OwnerClientId} score={score.Value}");
            }
            var captureDirectory = DevelopmentNetworkPanel.Argument("-ev-capture-dir");
            if (!string.IsNullOrEmpty(captureDirectory))
                CaptureCamera(_session.Manager.LocalClient.PlayerObject.GetComponentInChildren<Camera>(),
                    System.IO.Path.Combine(captureDirectory, _host ? "host-task.png" : "client-task.png"));
        }

        private IEnumerator CheckStamina()
        {
            ViggoStamina viggo = null;
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                var stamina = player.GetComponent<ViggoStamina>();
                Check(stamina != null, "Missing stamina component.");
                var state = (NetworkVariable<ViggoMobilityState>)typeof(ViggoStamina)
                    .GetField("_stamina", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(stamina);
                Check(!state.CanClientWrite(1), "Client can write authoritative stamina.");
                if (player.Role == PlayerRole.Viggo) viggo = stamina;
                else Check(!stamina.Applies && stamina.Value == 0, "Electrician affected by stamina.");
            }
            Check(viggo != null, "Missing Viggo stamina.");
            yield return Until(() => viggo.Value >= viggo.Settings.MaximumStamina - 0.1f, "full stamina before movement");
            if (_host)
            {
                var movement = viggo.GetComponent<PlayerMovement>();
                movement.enabled = false;
                var controller = viggo.GetComponent<CharacterController>();
                controller.enabled = false;
                viggo.transform.SetPositionAndRotation(new Vector3(-15, 0.03f, -12), Quaternion.identity);
                controller.enabled = true;
                yield return new WaitForSecondsRealtime(2);
                var start = viggo.transform.position;
                var elapsed = 0f;
                while (elapsed < 2.5f)
                {
                    var step = Mathf.Min(Time.deltaTime, 2.5f - elapsed);
                    movement.Simulate(new MovementCommand { Move = Vector2.up }, step);
                    elapsed += step;
                    yield return null;
                }
                Check(Vector3.Distance(start, viggo.transform.position) > 7, "Viggo did not move at configured speed.");
                Check(viggo.Value < 70, "Viggo movement did not drain stamina.");
                movement.enabled = true;
            }
            else yield return Until(() => viggo.Value < 70, "replicated stamina drain");
            yield return Until(() => viggo.Value >= viggo.Settings.MaximumStamina - 0.1f, "stationary stamina recovery");
            Debug.Log("EV_STAMINA PASSED: server authority, Viggo movement/drain/recovery, electrician exclusion, replication.");
        }

        private IEnumerator CheckRest()
        {
            ViggoStamina viggo = null;
            foreach (var stamina in FindObjectsByType<ViggoStamina>(FindObjectsSortMode.None))
                if (stamina.Applies) viggo = stamina;
            var feedback = viggo.GetComponent<ViggoRestFeedback>();
            var before = feedback.JavellEvents;
            if (_host)
            {
                var movement = viggo.GetComponent<PlayerMovement>();
                movement.enabled = false;
                var controller = viggo.GetComponent<CharacterController>();
                controller.enabled = false;
                viggo.transform.SetPositionAndRotation(new Vector3(-15, 0.03f, -12), Quaternion.identity);
                controller.enabled = true;
                var started = Time.realtimeSinceStartup;
                while (!viggo.IsResting)
                {
                    Check(Time.realtimeSinceStartup - started < 10, "Movement failed to exhaust Viggo.");
                    var forward = Mathf.FloorToInt((Time.realtimeSinceStartup - started) / 1.5f) % 2 == 0;
                    movement.Simulate(new MovementCommand { Move = forward ? Vector2.up : Vector2.down, Sprint = true }, Time.deltaTime);
                    yield return null;
                }
                var restPosition = viggo.transform.position;
                var elapsed = 0f;
                while (elapsed < 1)
                {
                    movement.Simulate(new MovementCommand { Move = Vector2.one, Sprint = true, Jump = true }, Time.deltaTime);
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                Check(Vector3.Distance(restPosition, viggo.transform.position) < 0.1f, "Input bypassed forced rest.");
                movement.enabled = true;
            }
            else yield return Until(() => viggo.IsResting, "replicated forced rest");
            Check(!viggo.CanCapture, "Viggo can capture while resting.");
            yield return Until(() => !viggo.IsResting, "rest recovery");
            Check(viggo.Value >= viggo.Settings.MaximumStamina - 0.1f && viggo.CanCapture, "Rest failed to restore stamina/capture eligibility.");
            Check(feedback.JavellEvents == before + 1, "Javell did not fire exactly once per rest.");
            Debug.Log("EV_REST PASSED: exhaustion, forced input restriction, recovery, capture gate, one Javell event, replication.");
        }

        private void PlaceForBattery(float x = 17, float z = 5.5f)
        {
            var local = _session.Manager.LocalClient.PlayerObject;
            local.GetComponent<PlayerMovement>().enabled = false;
            local.GetComponent<PlayerLook>().enabled = false;
            var controller = local.GetComponent<CharacterController>();
            controller.enabled = false;
            local.transform.SetPositionAndRotation(new Vector3(x, 0.03f, z), Quaternion.identity);
            controller.enabled = true;
            var movement = local.GetComponent<PlayerMovement>();
            movement.Simulate(default, 1f / 60);
            var camera = local.GetComponentInChildren<Camera>();
            camera.transform.localPosition = Vector3.up * (movement.Height - movement.Settings.EyeInset);
            camera.transform.LookAt(_session.Battery.InteractionCollider.bounds.center);
            Physics.SyncTransforms();
        }

        private void BatteryPulse()
        {
            var local = _session.Manager.LocalClient.PlayerObject;
            var interaction = local.GetComponent<PlayerInteraction>();
            interaction.RequestInteractionRpc(new NetworkObjectReference(_session.Battery.NetworkObject),
                _session.Battery.InteractionCollider.bounds.center - interaction.ServerEyePosition);
        }

        private IEnumerator CheckBattery()
        {
            var battery = _session.Battery;
            Check(battery != null && FindObjectsByType<BatteryStation>(FindObjectsSortMode.None).Length == 1, "Expected one battery.");
            var variable = (NetworkVariable<BatteryState>)typeof(BatteryStation)
                .GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(battery);
            Check(!variable.CanClientWrite(1), "Client can write battery state.");
            PlaceForBattery(_host ? 16 : 17);
            yield return new WaitForSecondsRealtime(1);
            var local = _session.Manager.LocalClient.PlayerObject;
            var interaction = local.GetComponent<PlayerInteraction>();
            if (_host)
            {
                BatteryPulse();
                Check(battery.State.Phase == BatteryPhase.AtDrill, "Viggo stole his battery.");
                Check(!battery.State.Discovered && !_session.Workday.GetComponent<ViggoRage>().Active, "Checking present battery triggered Rage.");
            }
            else
            {
                yield return new WaitForSecondsRealtime(1);
                var input = local.GetComponent<PlayerInputReader>();
                input.CaptureCursor();
                interaction.RefreshTarget();
                Check(interaction.Target == battery, "Battery hold prompt is missing.");
                // A press without continuing to hold must be cancelled by the shared input path.
                interaction.Interact();
                yield return new WaitForSecondsRealtime(0.7f);
                Check(battery.State.Phase == BatteryPhase.AtDrill, "Released hold was not cancelled.");
                input.ReleaseCursor();
                BatteryPulse();
                yield return Until(() => battery.State.Phase == BatteryPhase.BeingStolen, "theft start");
                yield return new WaitForSecondsRealtime(0.7f);
                Check(battery.State.Phase == BatteryPhase.AtDrill, "Missing hold pulses did not cancel theft.");
                BatteryPulse();
                yield return Until(() => battery.State.Phase == BatteryPhase.BeingStolen, "range cancellation start");
                PlaceForBattery(17, 2);
                yield return Until(() => battery.State.Phase == BatteryPhase.AtDrill, "out-of-range theft cancellation");
                PlaceForBattery();
                yield return new WaitForSecondsRealtime(1);
                var started = Time.realtimeSinceStartup;
                while (battery.State.Phase != BatteryPhase.Carried)
                {
                    Check(Time.realtimeSinceStartup - started < 8, "Held theft did not complete.");
                    BatteryPulse();
                    yield return new WaitForSecondsRealtime(0.1f);
                }
                Check(Time.realtimeSinceStartup - started >= battery.Settings.TheftSeconds - 0.15f, "Theft completed too early.");
                BatteryPulse(); // Repeated completion cannot create another carrier/theft.
            }
            yield return Until(() => battery.State.Phase == BatteryPhase.Carried, "replicated battery theft");
            Check(!battery.State.Discovered && battery.State.TheftId == 1 && battery.State.CarrierId == 2 &&
                battery.State.OriginalThiefId == 2, "Incorrect carrier, original thief, secrecy or duplicate theft.");
            Check(!_session.Workday.GetComponent<ViggoRage>().Active, "Theft triggered Rage before discovery.");
            Check(!_session.Manager.LocalClient.PlayerObject.GetComponent<ViggoStamina>().IsResting, "Unexpected rest after theft.");
            local.GetComponent<PlayerMovement>().enabled = local.GetComponent<PlayerLook>().enabled = true;
            local.GetComponent<PlayerInputReader>().ReleaseCursor();
            Debug.Log("EV_BATTERY PASSED: prompt, Viggo rejection, hold duration, release/timeout/range cancellation, unique carrier/thief, undiscovered replication.");
        }

        private IEnumerator CheckRage()
        {
            var rage = _session.Workday.GetComponent<ViggoRage>();
            var battery = _session.Battery;
            var state = (NetworkVariable<RageState>)typeof(ViggoRage)
                .GetField("_state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(rage);
            Check(!state.CanClientWrite(1), "Client can set Rage.");
            if (!_host)
            {
                BatteryPulse();
                yield return new WaitForSecondsRealtime(1);
                Check(!rage.Active && !battery.State.Discovered, "Electrician triggered discovery.");
            }
            else
            {
                yield return new WaitForSecondsRealtime(2);
                Check(!rage.Active && !battery.State.Discovered, "Theft auto-discovered.");
                PlaceForBattery(16);
                var interaction = _session.Manager.LocalClient.PlayerObject.GetComponent<PlayerInteraction>();
                _session.Manager.LocalClient.PlayerObject.GetComponent<PlayerInputReader>().CaptureCursor();
                interaction.RefreshTarget();
                Check(interaction.Target == battery && interaction.Prompt.Contains("Check"), "Viggo inspection prompt is missing.");
                interaction.Interact();
            }
            yield return Until(() => rage.Active && battery.State.Discovered, "discovery and Rage replication");
            Check(rage.Sequence == 1, "Discovery repeated.");
            ViggoStamina viggo = null;
            foreach (var stamina in FindObjectsByType<ViggoStamina>(FindObjectsSortMode.None)) if (stamina.Applies) viggo = stamina;
            var restCount = viggo.State.RestCount;
            if (_host)
            {
                BatteryPulse();
                BatteryPulse();
                Check(rage.Sequence == 1, "Duplicate discovery produced extra Rage events.");
                PlaceForBattery(-15, -12);
                var movement = viggo.GetComponent<PlayerMovement>();
                var initialStamina = viggo.Value;
                var travelled = 0f;
                var started = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - started < 6)
                {
                    var previous = viggo.transform.position;
                    var forward = Mathf.FloorToInt((Time.realtimeSinceStartup - started) / 0.7f) % 2 == 0;
                    movement.Simulate(new MovementCommand { Move = forward ? Vector2.up : Vector2.down }, Time.deltaTime);
                    travelled += Vector3.Distance(previous, viggo.transform.position);
                    Check(!viggo.IsResting && viggo.MovementSpeed(false, false) == viggo.Settings.RageSpeed, "Rage mobility override failed.");
                    yield return null;
                }
                Check(travelled > 35 && Mathf.Abs(viggo.Value - initialStamina) < 0.01f, "Rage pursuit still consumes stamina or uses normal speed.");
                movement.enabled = viggo.GetComponent<PlayerLook>().enabled = true;
                viggo.GetComponent<PlayerInputReader>().ReleaseCursor();
            }
            else yield return new WaitForSecondsRealtime(7);
            Check(viggo.State.RestCount == restCount && !viggo.IsResting && rage.Sequence == 1, "Rage re-entered exhaustion or repeated discovery.");
            Debug.Log("EV_RAGE PASSED: no automatic/electrician discovery, authoritative inspection, one event, shared Rage, continuous pursuit.");
        }

        private IEnumerator CheckTheftContention()
        {
            var battery = _session.Battery;
            yield return Until(() => battery.State.Phase == BatteryPhase.BeingStolen, "observer theft contention", 90);
            PlaceForBattery(18);
            yield return new WaitForSecondsRealtime(0.15f);
            // Attempt only while another worker owns the live attempt; never acquire an abandoned attempt.
            var attempts = 0;
            var deadline = Time.realtimeSinceStartup + 20;
            while (battery.State.Phase != BatteryPhase.Carried)
            {
                Check(Time.realtimeSinceStartup < deadline, "Contention did not finish.");
                if (battery.State.Phase == BatteryPhase.BeingStolen && battery.State.Progress > 0.1f && battery.State.Progress < 0.8f)
                {
                    BatteryPulse();
                    attempts++;
                }
                yield return new WaitForSecondsRealtime(0.1f);
            }
            Check(attempts > 0 && battery.State.CarrierId == 2 && battery.State.OriginalThiefId == 2 && battery.State.TheftId == 1,
                "Competing electrician acquired duplicate battery ownership.");
            Debug.Log("EV_BATTERY_CONTENTION PASSED: competing electrician requests rejected.");
        }

        private IEnumerator CheckLateJoin()
        {
            yield return Until(() => _session.PlayerCount == 3, "late observer and existing players");
            yield return new WaitForSecondsRealtime(1);
            CheckOwnership();
            var task = FindFirstObjectByType<SimpleCircuitTask>();
            Check(task != null && task.State.Phase == WorkTaskPhase.Completed, "Late join lost completed task state.");
            Check(_session.Workday.State.MinuteOfDay > WorkdayClock.StartMinute, "Late join lost workday time.");
            var viggos = 0;
            foreach (var player in FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                Check(player.Role == RoleAssignment.ForClient(player.OwnerClientId), "Late join role mismatch.");
                if (player.Role == PlayerRole.Viggo) viggos++;
                var expected = player.OwnerClientId == task.State.WorkerId ? _session.Workday.GetComponent<ScoreService>().Settings.CircuitTaskPoints : 0;
                Check(player.GetComponent<PlayerScore>().Value == expected, "Late join score mismatch.");
            }
            Check(viggos == 1, "Three players must still have exactly one Viggo.");
            yield return CheckTheftContention();
            yield return Until(() => _session.Workday.State.Phase == MatchPhase.EndOfDay, "late observer end of day", 120);
            _session.Disconnect();
            yield return Until(() => _session.CanStart && _session.PlayerCount == 0, "observer cleanup");
            Debug.Log("EV_OBSERVER PASSED: three players, roles, completed task, scores, clock and EndOfDay.");
            Application.Quit(_failed ? 1 : 0);
        }

        private void CaptureCamera(Camera camera, string path)
        {
            var target = RenderTexture.GetTemporary(960, 540, 24, RenderTextureFormat.ARGB32);
            var pixels = new Texture2D(960, 540, TextureFormat.RGB24, false);
            var previous = RenderTexture.active;
            try
            {
                var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
                Check(RenderPipeline.SupportsRenderRequest(camera, request), "Camera render requests are unavailable.");
                RenderPipeline.SubmitRenderRequest(camera, request);
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, 960, 540), 0, 0);
                pixels.Apply();
                System.IO.File.WriteAllBytes(path, pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(target);
                Destroy(pixels);
            }
        }

        private IEnumerator Until(Func<bool> condition, string description, float timeout = 30)
        {
            var deadline = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                Check(Time.realtimeSinceStartup < deadline, "Timed out waiting for " + description);
                yield return null;
            }
        }

        private void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("EV_SMOKE FAILED: " + message);
        }

        private void HandleLog(string message, string stack, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            _failed = true;
            Application.Quit(1);
        }

        private static ushort Port() => ushort.TryParse(DevelopmentNetworkPanel.Argument("-ev-port"), out var port) ? port : (ushort)7777;
        private void OnDestroy() => Application.logMessageReceived -= HandleLog;

        [Serializable]
        private struct Snapshot
        {
            public int round;
            public ulong owner;
            public float x, y, z, yaw;
            public bool crouching;
            public string role;
        }
#endif
    }
}
