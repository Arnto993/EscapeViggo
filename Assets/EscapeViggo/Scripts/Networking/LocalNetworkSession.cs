using System.Collections.Generic;
using System.Net;
using EscapeViggo.Core;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace EscapeViggo.Networking
{
    /// <summary>Local listen-server lifecycle, role spawn slots, and connection feedback.</summary>
    public sealed class LocalNetworkSession : MonoBehaviour
    {
        [SerializeField] private NetworkManager _manager;
        [SerializeField] private UnityTransport _transport;
        [SerializeField] private Camera _overview;
        [SerializeField] private Transform[] _spawnPoints;
        [SerializeField] private NetworkWorkday _workdayPrefab;
        private readonly Dictionary<ulong, int> _slots = new();
        private AudioListener _overviewListener;
        private bool _connecting;
        private bool _connectionFailed;
        private float _connectionDeadline;
        private OnlineNetworkSession _online;
        private float _restartAvailableAt;
        private const float TransportReuseDelaySeconds = 0.25f;
        [SerializeField, Min(1)] private float _connectionTimeoutSeconds = 12;
        public NetworkManager Manager => _manager;
        public string Status { get; private set; } = "Offline";
        public bool CanStart => Time.realtimeSinceStartup >= _restartAvailableAt && !_connecting && !_manager.IsListening &&
            !_manager.ShutdownInProgress && !(_online != null && _online.Active);
        public int Capacity => _spawnPoints.Length;
        public OnlineNetworkSession Online => _online;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public double? PlaytestDayMinutes { get; private set; }
        public bool ConfigurePlaytestDay(string minutes)
        {
            if (!CanStart) return false;
            if (string.IsNullOrEmpty(minutes)) { PlaytestDayMinutes = null; return true; }
            if (!PlaytestOptions.TryParseDayMinutes(minutes, out var duration))
            {
                Status = "Day length must be 0.1-60 minutes.";
                return false;
            }
            PlaytestDayMinutes = duration;
            return true;
        }
#endif
        public NetworkWorkday Workday { get; private set; }
        public Battery.BatteryStation Battery { get; private set; }
        internal void RegisterBattery(Battery.BatteryStation battery) => Battery = battery;
        internal void RegisterWorkday(NetworkWorkday workday) => Workday = workday;
        internal Transform SpawnPointFor(ulong clientId) => _slots.TryGetValue(clientId, out var slot) ? _spawnPoints[slot] : _spawnPoints[1];
        internal void PromoteViggoOnServer(ulong id)
        {
            if (!_manager.IsServer || !_manager.ConnectedClients.TryGetValue(id, out var client) || client.PlayerObject == null) return;
            _slots[id] = 0;
            client.PlayerObject.GetComponent<NetworkPlayer>().AssignRoleOnServer(Player.PlayerRole.Viggo);
            client.PlayerObject.GetComponent<Player.PlayerCapture>().ResetDayOnServer();
            client.PlayerObject.GetComponent<Viggo.ViggoStamina>().ResetDayOnServer();
        }
        internal void RotateRolesOnServer(ulong viggo)
        {
            if (!_manager.IsServer) return;
            _slots.Clear();
            _slots[viggo] = 0;
            var workerSlot = 1;
            foreach (var client in _manager.ConnectedClientsList)
            {
                if (client.PlayerObject == null) continue;
                if (client.ClientId != viggo) _slots[client.ClientId] = workerSlot++;
                var player = client.PlayerObject.GetComponent<NetworkPlayer>();
                player.AssignRoleOnServer(client.ClientId == viggo ? Player.PlayerRole.Viggo : Player.PlayerRole.Electrician);
                player.GetComponent<Player.PlayerCapture>().ResetDayOnServer();
                player.GetComponent<Viggo.ViggoStamina>().ResetDayOnServer();
            }
        }
        public int PlayerCount
        {
            get
            {
                var count = 0;
                if (_manager.SpawnManager != null)
                    foreach (var spawned in _manager.SpawnManager.SpawnedObjects.Values)
                        if (spawned.IsPlayerObject) count++;
                return count;
            }
        }

        private void Awake()
        {
            Application.runInBackground = true;
            _online = GetComponent<OnlineNetworkSession>();
            _overviewListener = _overview.GetComponent<AudioListener>();
            _manager.ConnectionApprovalCallback += Approve;
            _manager.OnClientConnectedCallback += Connected;
            _manager.OnClientDisconnectCallback += Disconnected;
            _manager.OnClientStopped += Stopped;
            _manager.OnServerStopped += Stopped;
            _manager.OnTransportFailure += TransportFailed;
            _manager.OnServerStarted += SpawnWorkday;
        }

        private void SpawnWorkday()
        {
            var workday = Instantiate(_workdayPrefab);
            workday.NetworkObject.Spawn();
        }

        public bool StartHost(string address = "127.0.0.1", ushort port = 7777) => StartConnection(true, address, port);
        public bool StartClient(string address = "127.0.0.1", ushort port = 7777) => StartConnection(false, address, port);

        private bool StartConnection(bool host, string address, ushort port)
        {
            if (!CanStart) return false;
            if (!IPAddress.TryParse(address, out _) || port == 0)
            {
                Status = "Enter a valid IP address and port (1-65535).";
                return false;
            }
            _slots.Clear();
            _connectionFailed = false;
            // SetConnectionData also restores direct transport mode after a Relay session.
            _transport.SetConnectionData(address, port, host ? address : null);
            Status = host ? "Starting host..." : "Connecting...";
            _connecting = !host;
            _connectionDeadline = Time.unscaledTime + _connectionTimeoutSeconds;
            var started = host ? _manager.StartHost() : _manager.StartClient();
            if (!started)
            {
                _connecting = false;
                Status = "Could not start. Check the IP/port; another host may already use this port.";
            }
            return started;
        }

        public void Disconnect()
        {
            if (_online != null && _online.Active) { _online.Disconnect(); return; }
            Status = "Disconnecting...";
            _connecting = false;
            _manager.Shutdown();
        }

        internal void PrepareOnlineConnection()
        {
            _slots.Clear();
            _connectionFailed = false;
            _connecting = false;
        }

        internal void FinishOnlineConnection()
        {
            _connecting = false;
            if (_manager.IsListening && !_manager.ShutdownInProgress) _manager.Shutdown();
            _slots.Clear();
            Status = "Offline";
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var slot = -1;
            // Slot zero is reserved for the listen-server Viggo; other slots are electricians.
            var firstSlot = request.ClientNetworkId == NetworkManager.ServerClientId ? 0 : 1;
            for (var i = firstSlot; i < _spawnPoints.Length; i++)
                if (!_slots.ContainsValue(i)) { slot = i; break; }
            response.Approved = slot >= 0;
            response.CreatePlayerObject = response.Approved;
            response.Pending = false;
            if (!response.Approved)
            {
                response.Reason = "Session is full. All player places are occupied.";
                return;
            }
            _slots[request.ClientNetworkId] = slot;
            response.Position = _spawnPoints[slot].position;
            response.Rotation = _spawnPoints[slot].rotation;
        }

        private void Connected(ulong clientId)
        {
            if (clientId == _manager.LocalClientId)
            {
                _connecting = false;
                Status = _manager.IsHost ? "Host connected" : "Client connected";
            }
            Debug.Log($"Local session connected client {clientId}.", this);
        }

        private void Disconnected(ulong clientId)
        {
            _slots.Remove(clientId);
            if (clientId == _manager.LocalClientId)
            {
                if (_connecting)
                    Status = string.IsNullOrEmpty(_manager.DisconnectReason)
                        ? "Could not connect. Start the host and check the IP/port, then retry." : _manager.DisconnectReason;
                else if (!_connectionFailed)
                    Status = string.IsNullOrEmpty(_manager.DisconnectReason) ? "Disconnected. Start or join a host to play again." : _manager.DisconnectReason;
                _connecting = false;
            }
            Debug.Log($"Network session disconnected client {clientId} ({_transport.DisconnectEvent}).", this);
        }

        private void Stopped(bool wasHost)
        {
            // Let the peer consume the old disconnect before reusing the local transport.
            _restartAvailableAt = Time.realtimeSinceStartup + TransportReuseDelaySeconds;
            _slots.Clear();
            _connecting = false;
            if (Status == "Connecting...") Status = "Could not connect. Start the host and check the IP/port.";
            if (Status == "Disconnecting..." || Status == "Host connected" || Status == "Client connected") Status = "Offline";
        }

        private void TransportFailed()
        {
            _connectionFailed = true;
            _connecting = false;
            Status = "Transport failed. Check the address and port, then reconnect.";
        }

        private void Update()
        {
            if (_connecting && Time.unscaledTime >= _connectionDeadline)
            {
                _connecting = false;
                _connectionFailed = true;
                Status = "Connection timed out. Start the host and check the IP/port, then retry.";
                _manager.Shutdown();
            }
            var hasLocalPlayer = _manager.IsConnectedClient && _manager.LocalClient.PlayerObject != null;
            _overview.enabled = !hasLocalPlayer;
            _overviewListener.enabled = !hasLocalPlayer;
        }

        private void OnDestroy()
        {
            if (_manager == null) return;
            _manager.ConnectionApprovalCallback -= Approve;
            _manager.OnClientConnectedCallback -= Connected;
            _manager.OnClientDisconnectCallback -= Disconnected;
            _manager.OnClientStopped -= Stopped;
            _manager.OnServerStopped -= Stopped;
            _manager.OnTransportFailure -= TransportFailed;
            _manager.OnServerStarted -= SpawnWorkday;
        }
    }
}
