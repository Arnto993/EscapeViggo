using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using EscapeViggo.Player;
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
            if (!DevelopmentNetworkPanel.HasArgument("-ev-smoke")) return;
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

        private IEnumerator Until(Func<bool> condition, string description)
        {
            var deadline = Time.realtimeSinceStartup + 30;
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
