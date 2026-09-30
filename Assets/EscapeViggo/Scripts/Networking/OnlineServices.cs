using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

namespace EscapeViggo.Networking
{
    public enum OnlineServicesState { Offline, Initializing, Authenticating, Ready, Failed }

    // This small boundary lets readiness/retry be tested without contacting UGS.
    public interface IOnlineServicesBackend
    {
        bool IsSignedIn { get; }
        Task InitializeAsync();
        Task SignInAsync();
    }

    public sealed class OnlineServices
    {
        private readonly IOnlineServicesBackend _backend;
        private Task<bool> _pending;
        public OnlineServicesState State { get; private set; }
        public string Error { get; private set; } = "";

        public OnlineServices(IOnlineServicesBackend backend) => _backend = backend;

        public Task<bool> EnsureReadyAsync()
        {
            if (_pending != null && !_pending.IsCompleted) return _pending;
            return _pending = InitializeAsync();
        }

        private async Task<bool> InitializeAsync()
        {
            Error = "";
            try
            {
                State = OnlineServicesState.Initializing;
                await _backend.InitializeAsync();
                State = OnlineServicesState.Authenticating;
                if (!_backend.IsSignedIn) await _backend.SignInAsync();
                if (!_backend.IsSignedIn) throw new InvalidOperationException("Sign-in did not complete.");
                State = OnlineServicesState.Ready;
                return true;
            }
            catch (Exception exception)
            {
                Error = exception is MissingCloudProjectException
                    ? "Online play needs a linked Unity Cloud Project. See README: Online play setup."
                    : State == OnlineServicesState.Authenticating
                        ? "Anonymous authentication failed. Check your internet connection and retry."
                        : "Unity Services unavailable. Check your connection and project setup, then retry.";
                State = OnlineServicesState.Failed;
                // Never print authentication exception bodies/tokens.
                Debug.LogWarning($"Online readiness failed ({exception.GetType().Name}).");
                return false;
            }
        }
    }

    public sealed class UnityOnlineServicesBackend : IOnlineServicesBackend
    {
        private readonly string _environment;
        private readonly string _profile;
        public UnityOnlineServicesBackend(string environment, string profile)
        {
            _environment = environment;
            _profile = profile;
        }
        public bool IsSignedIn => AuthenticationService.Instance.IsSignedIn;
        public async Task InitializeAsync()
        {
            if (string.IsNullOrWhiteSpace(Application.cloudProjectId)) throw new MissingCloudProjectException();
            if (UnityServices.State == ServicesInitializationState.Initialized) return;
            var options = new InitializationOptions().SetEnvironmentName(_environment);
            if (!string.IsNullOrWhiteSpace(_profile)) options.SetProfile(_profile);
            await UnityServices.InitializeAsync(options);
        }
        public Task SignInAsync() => AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public sealed class MissingCloudProjectException : Exception { }
}
