using System;
using System.Threading.Tasks;
using EscapeViggo.Networking;
using NUnit.Framework;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;

namespace EscapeViggo.Tests
{
    public sealed class OnlineConnectionTests
    {
        private sealed class ServicesBackend : IOnlineServicesBackend
        {
            public bool IsSignedIn { get; set; }
            public int Initializes, SignIns;
            public TaskCompletionSource<bool> Initialization;
            public TaskCompletionSource<bool> Authentication;
            public bool FailInitialization, FailAuthentication;
            public async Task InitializeAsync()
            {
                Initializes++;
                if (Initialization != null) await Initialization.Task;
                if (FailInitialization) throw new InvalidOperationException();
            }
            public async Task SignInAsync()
            {
                SignIns++;
                if (Authentication != null) await Authentication.Task;
                if (FailAuthentication) throw new InvalidOperationException();
                IsSignedIn = true;
            }
        }

        [Test]
        public async Task AuthenticationRemainsPendingUntilSignedInAndExpiredLoginCanRetry()
        {
            var backend = new ServicesBackend { Authentication = new TaskCompletionSource<bool>() };
            var services = new OnlineServices(backend);
            var ready = services.EnsureReadyAsync();
            Assert.That(services.State, Is.EqualTo(OnlineServicesState.Authenticating));
            Assert.That(ready.IsCompleted, Is.False);
            backend.Authentication.SetResult(true);
            Assert.That(await ready, Is.True);
            backend.IsSignedIn = false;
            Assert.That(await services.EnsureReadyAsync(), Is.True);
            Assert.That(backend.SignIns, Is.EqualTo(2));
        }

        [Test]
        public async Task ReadinessSharesInitializationAndAuthenticatesOnce()
        {
            var backend = new ServicesBackend { Initialization = new TaskCompletionSource<bool>() };
            var services = new OnlineServices(backend);
            var first = services.EnsureReadyAsync();
            var second = services.EnsureReadyAsync();
            Assert.That(services.State, Is.EqualTo(OnlineServicesState.Initializing));
            Assert.That(second, Is.SameAs(first));
            backend.Initialization.SetResult(true);
            Assert.That(await first, Is.True);
            Assert.That(services.State, Is.EqualTo(OnlineServicesState.Ready));
            Assert.That(backend.Initializes, Is.EqualTo(1));
            Assert.That(await services.EnsureReadyAsync(), Is.True);
            Assert.That(backend.SignIns, Is.EqualTo(1));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task InitializationAndAuthenticationFailuresCanRetry(bool initialization)
        {
            var backend = new ServicesBackend { FailInitialization = initialization, FailAuthentication = !initialization };
            var services = new OnlineServices(backend);
            Assert.That(await services.EnsureReadyAsync(), Is.False);
            Assert.That(services.State, Is.EqualTo(OnlineServicesState.Failed));
            StringAssert.Contains(initialization ? "Unity Services" : "authentication", services.Error);
            backend.FailInitialization = backend.FailAuthentication = false;
            Assert.That(await services.EnsureReadyAsync(), Is.True);
            Assert.That(services.Error, Is.Empty);
        }

        [TestCase(null, false)]
        [TestCase("   ", false)]
        [TestCase("abc", false)]
        [TestCase("ABC 12", false)]
        [TestCase("ABC!12", false)]
        [TestCase("ÅBC123", false)]
        [TestCase("abcdefg", false)]
        [TestCase("  abC123\r\n", true)]
        public void JoinCodeValidation(string input, bool valid)
        {
            Assert.That(OnlineConnectionState.TryNormalizeCode(input, out var code), Is.EqualTo(valid));
            if (valid) Assert.That(code, Is.EqualTo("ABC123"));
        }

        [Test]
        public void DuplicateCreateJoinBlockedThroughCleanupAndLateSuccessIgnored()
        {
            var connection = new OnlineConnectionState();
            Assert.That(connection.TryBegin(), Is.True);
            Assert.That(connection.TryBegin(), Is.False);
            connection.Connecting(true);
            Assert.That(connection.TryBegin(), Is.False);
            connection.BeginLeave("Connection timed out");
            connection.Connected();
            Assert.That(connection.Phase, Is.EqualTo(OnlineConnectionPhase.Leaving));
            Assert.That(connection.TryBegin(), Is.False);
            connection.FinishLeave(true);
            Assert.That(connection.TryBegin(), Is.True);
        }

        [Test]
        public void DisconnectRecoveryRetainsFailureAndAllowsNewAttempt()
        {
            var connection = new OnlineConnectionState();
            connection.TryBegin();
            connection.Connecting(false);
            connection.Connected();
            Assert.That(connection.Phase, Is.EqualTo(OnlineConnectionPhase.Connected));
            connection.BeginLeave("Host disconnected");
            connection.Connected();
            Assert.That(connection.Phase, Is.EqualTo(OnlineConnectionPhase.Leaving));
            Assert.That(connection.TryBegin(), Is.False);
            connection.FinishLeave(true);
            Assert.That(connection.Message, Is.EqualTo("Host disconnected"));
            Assert.That(connection.TryBegin(), Is.True);
        }

        [TestCase(LobbyExceptionReason.LobbyFull, "Session is full")]
        [TestCase(LobbyExceptionReason.LobbyNotFound, "no longer exists")]
        [TestCase(LobbyExceptionReason.InvalidJoinCode, "Invalid join code")]
        public void WrappedSessionErrorsHaveUsefulFeedback(LobbyExceptionReason reason, string expected)
        {
            var error = new SessionException("raw service body", SessionError.Unknown,
                new LobbyServiceException(reason, "raw service body"));
            var message = OnlineConnectionState.FailureMessage(error);
            StringAssert.Contains(expected, message);
            StringAssert.DoesNotContain("raw service body", message);
        }

        [Test]
        public void TimeoutHasRetryGuidance() => StringAssert.Contains("retry", OnlineConnectionState.FailureMessage(new TimeoutException()));
    }
}
