using System.Collections.Generic;
using System.Net;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace EscapeViggo.Networking
{
    /// <summary>Local listen-server lifecycle, neutral spawn slots, and connection feedback.</summary>
    public sealed class LocalNetworkSession : MonoBehaviour
    {
        [SerializeField] private NetworkManager _manager;
        [SerializeField] private UnityTransport _transport;
        [SerializeField] private Camera _overview;
        [SerializeField] private Transform[] _spawnPoints;
        private readonly Dictionary<ulong, int> _slots = new();
        private AudioListener _overviewListener;
        public NetworkManager Manager => _manager;
        public string Status { get; private set; } = "Offline";
        public bool CanStart => !_manager.IsListening && !_manager.ShutdownInProgress;
        public int PlayerCount => _manager.SpawnManager == null ? 0 : _manager.SpawnManager.SpawnedObjects.Count;

        private void Awake()
        {
            Application.runInBackground = true;
            _overviewListener = _overview.GetComponent<AudioListener>();
            _manager.ConnectionApprovalCallback += Approve;
            _manager.OnClientConnectedCallback += Connected;
            _manager.OnClientDisconnectCallback += Disconnected;
            _manager.OnClientStopped += Stopped;
            _manager.OnServerStopped += Stopped;
            _manager.OnTransportFailure += TransportFailed;
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
            _transport.SetConnectionData(address, port, host ? address : null);
            Status = host ? "Starting host..." : "Connecting...";
            var started = host ? _manager.StartHost() : _manager.StartClient();
            if (!started) Status = "Connection could not start. Check the Console/player log.";
            return started;
        }

        public void Disconnect()
        {
            Status = "Disconnecting...";
            _manager.Shutdown();
        }

        private void Approve(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var slot = -1;
            for (var i = 0; i < _spawnPoints.Length; i++)
                if (!_slots.ContainsValue(i)) { slot = i; break; }
            response.Approved = slot >= 0;
            response.CreatePlayerObject = response.Approved;
            response.Pending = false;
            if (!response.Approved)
            {
                response.Reason = "All development spawn slots are occupied.";
                return;
            }
            _slots[request.ClientNetworkId] = slot;
            response.Position = _spawnPoints[slot].position;
            response.Rotation = _spawnPoints[slot].rotation;
        }

        private void Connected(ulong clientId)
        {
            if (clientId == _manager.LocalClientId)
                Status = _manager.IsHost ? "Host connected" : "Client connected";
            Debug.Log($"Local session connected client {clientId}.", this);
        }

        private void Disconnected(ulong clientId)
        {
            _slots.Remove(clientId);
            if (clientId == _manager.LocalClientId)
                Status = string.IsNullOrEmpty(_manager.DisconnectReason) ? "Disconnected" : _manager.DisconnectReason;
            Debug.Log($"Local session disconnected client {clientId}.", this);
        }

        private void Stopped(bool wasHost)
        {
            _slots.Clear();
            if (Status == "Disconnecting..." || Status == "Host connected" || Status == "Client connected") Status = "Offline";
        }

        private void TransportFailed() => Status = "Transport failed. Check the address and port, then reconnect.";

        private void Update()
        {
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
        }
    }
}
