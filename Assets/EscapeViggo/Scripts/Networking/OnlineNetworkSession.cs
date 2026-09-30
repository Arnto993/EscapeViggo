using System;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;

namespace EscapeViggo.Networking
{
    /// <summary>Private Sessions own Relay/NGO startup; LocalNetworkSession keeps gameplay lifecycle.</summary>
    [RequireComponent(typeof(LocalNetworkSession))]
    public sealed class OnlineNetworkSession : MonoBehaviour
    {
        [SerializeField] private string _environment = "production";
        [SerializeField, Min(10)] private float _connectionTimeoutSeconds = 60;
        private LocalNetworkSession _local;
        private OnlineServices _services;
        private ISession _session;
        private bool _starting;
        private bool _failed;
        private bool _destroyed;
        private float _deadline;
        private string _originalHost;
        public OnlineConnectionState Connection { get; } = new();
        public bool Active => Connection.Active;
        public string JoinCode => Connection.Phase == OnlineConnectionPhase.Connected ? _session?.Code ?? "" : "";
        public string Status => Connection.Phase == OnlineConnectionPhase.Preparing
            ? _services.State == OnlineServicesState.Authenticating ? "Signing in anonymously..." : "Initializing Unity Services..."
            : Connection.Message;
        public OnlineServicesState ServicesState => _services.State;

        private void Awake()
        {
            _local = GetComponent<LocalNetworkSession>();
            string profile = null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Separate auth profiles are necessary for multiple players on the same Windows account.
            profile = DevelopmentNetworkPanel.Argument("-ev-online-profile");
#endif
            _services = new OnlineServices(new UnityOnlineServicesBackend(_environment, profile));
            _local.Manager.OnClientStopped += NetworkStopped;
            _local.Manager.OnTransportFailure += TransportFailed;
        }

        public Task HostAsync() => ConnectAsync(true, null);
        public Task JoinAsync(string code) => ConnectAsync(false, code);

        private async Task ConnectAsync(bool host, string input)
        {
            if (!_local.CanStart) return;
            var code = "";
            if (!host && !OnlineConnectionState.TryNormalizeCode(input, out code))
            {
                Connection.Reject("Enter a six-character join code (letters and numbers).");
                return;
            }
            if (!Connection.TryBegin()) return;
            _starting = true;
            _failed = false;
            _deadline = Time.realtimeSinceStartup + _connectionTimeoutSeconds;
            _local.PrepareOnlineConnection();
            try
            {
                if (!await _services.EnsureReadyAsync())
                {
                    RequestLeave(_services.Error, true);
                    return;
                }
                if (Connection.Phase == OnlineConnectionPhase.Leaving || _destroyed) return;
                Connection.Connecting(host);
                var network = new NetworkOptions { RelayProtocol = RelayProtocol.DTLS };
                // Do not call StartHost/StartClient: the SDK's default NGO handler does that.
                _session = host
                    ? await MultiplayerService.Instance.CreateSessionAsync(new SessionOptions
                    {
                        Name = "Escape Viggo", IsPrivate = true, MaxPlayers = _local.Capacity
                    }.WithRelayNetwork().WithNetworkOptions(network))
                    : await MultiplayerService.Instance.JoinSessionByCodeAsync(code,
                        new JoinSessionOptions().WithNetworkOptions(network));
                if (_destroyed || Connection.Phase == OnlineConnectionPhase.Leaving) return;
                _originalHost = _session.Host;
                _session.Deleted += SessionLost;
                _session.RemovedFromSession += SessionLost;
                _session.SessionHostChanged += HostChanged;
                _session.Network.StartFailed += NetworkStartFailed;
                // Joining a Session can finish before NGO connects. Keep a bounded connecting state.
                while (!_local.Manager.IsConnectedClient && Connection.Phase == OnlineConnectionPhase.Connecting && !_destroyed)
                    await Task.Delay(50);
                if (!_destroyed && _local.Manager.IsConnectedClient) Connection.Connected();
            }
            catch (Exception exception)
            {
                if (Connection.Phase != OnlineConnectionPhase.Leaving)
                    RequestLeave(OnlineConnectionState.FailureMessage(exception), true);
                Debug.LogWarning($"Online connection failed ({exception.GetType().Name}).");
            }
            finally
            {
                _starting = false;
                if (_destroyed || Connection.Phase == OnlineConnectionPhase.Leaving) await CleanupAsync();
            }
        }

        public void Disconnect() => RequestLeave("Disconnected. Create or join another game.", false);
        private void SessionLost() => RequestLeave("Session ended or host disconnected. Create or join another game.", true);
        private void HostChanged(string host)
        {
            // Lobby host election must not turn into gameplay host migration.
            if (host != _originalHost) SessionLost();
        }
        private void NetworkStartFailed(SessionError error) => RequestLeave("Relay connection failed. Check the host and retry.", true);
        private void TransportFailed()
        {
            if (Active) RequestLeave("Relay connection lost. Check your connection and retry.", true);
        }
        private void NetworkStopped(bool wasHost)
        {
            if (Active && Connection.Phase != OnlineConnectionPhase.Leaving) SessionLost();
        }
        private void RequestLeave(string message, bool failed)
        {
            if (Connection.Phase == OnlineConnectionPhase.Leaving) return;
            _failed = failed;
            Connection.BeginLeave(message);
            // MPS create/join has no cancellation token. Keep the gate until it settles,
            // then dispose its result; a late response must never start a second game.
            if (!_starting) _ = CleanupAsync();
        }
        private async Task CleanupAsync()
        {
            var session = _session;
            _session = null;
            if (session != null)
            {
                session.Deleted -= SessionLost;
                session.RemovedFromSession -= SessionLost;
                session.SessionHostChanged -= HostChanged;
                session.Network.StartFailed -= NetworkStartFailed;
                try
                {
                    // Explicit deletion prevents a graceful host leave from electing another host.
                    if (session.IsHost) await session.AsHost().DeleteAsync();
                    else await session.LeaveAsync();
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Online session cleanup failed ({exception.GetType().Name}); remote membership may expire later.");
                }
            }
            if (!_destroyed)
            {
                // SDK normally stopped NGO. This fallback covers service/transport failure.
                _local.FinishOnlineConnection();
                while (_local.Manager.ShutdownInProgress) await Task.Delay(50);
            }
            Connection.FinishLeave(_failed);
        }
        private void Update()
        {
            if ((Connection.Phase == OnlineConnectionPhase.Preparing || Connection.Phase == OnlineConnectionPhase.Connecting)
                && Time.realtimeSinceStartup >= _deadline)
                RequestLeave("Connection timed out. Check your connection and retry after cleanup.", true);
        }
        private void OnDestroy()
        {
            _destroyed = true;
            _local.Manager.OnClientStopped -= NetworkStopped;
            _local.Manager.OnTransportFailure -= TransportFailed;
            if (Active) Disconnect();
        }
    }
}
