using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;

namespace EscapeViggo.Networking
{
    public enum OnlineConnectionPhase { Offline, Preparing, Connecting, Connected, Leaving, Failed }

    /// <summary>Single-operation gate. Cleanup must finish before another transport can start.</summary>
    public sealed class OnlineConnectionState
    {
        public OnlineConnectionPhase Phase { get; private set; }
        public string Message { get; private set; } = "Online private game";
        public bool Active => Phase != OnlineConnectionPhase.Offline && Phase != OnlineConnectionPhase.Failed;
        public bool TryBegin()
        {
            if (Active) return false;
            Phase = OnlineConnectionPhase.Preparing;
            Message = "Preparing online services...";
            return true;
        }
        public void Connecting(bool host)
        {
            if (Phase != OnlineConnectionPhase.Preparing) return;
            Phase = OnlineConnectionPhase.Connecting;
            Message = host ? "Creating private Relay session..." : "Joining private Relay session...";
        }
        public void Connected()
        {
            if (Phase != OnlineConnectionPhase.Connecting) return;
            Phase = OnlineConnectionPhase.Connected;
            Message = "Connected online";
        }
        public void BeginLeave(string message)
        {
            Phase = OnlineConnectionPhase.Leaving;
            Message = message;
        }
        public void FinishLeave(bool failed)
        {
            Phase = failed ? OnlineConnectionPhase.Failed : OnlineConnectionPhase.Offline;
        }
        public void Reject(string message)
        {
            if (Active) return;
            Phase = OnlineConnectionPhase.Failed;
            Message = message;
        }

        public static bool TryNormalizeCode(string input, out string code)
        {
            code = input?.Trim().ToUpperInvariant() ?? "";
            // Sessions use the Lobby join code, not the separate Relay allocation code.
            if (code.Length != 6) return false;
            foreach (var character in code)
                if (!(character >= 'A' && character <= 'Z') && !(character >= '0' && character <= '9')) return false;
            return true;
        }

        public static string FailureMessage(Exception exception)
        {
            // MPS wraps backend errors; inspect typed inner errors, never parse/display raw bodies.
            for (var inner = exception; inner != null; inner = inner.InnerException)
            {
                if (inner is LobbyServiceException lobby)
                {
                    switch (lobby.Reason)
                    {
                        case LobbyExceptionReason.LobbyFull: return "Session is full. Ask the host for a free place.";
                        case LobbyExceptionReason.LobbyLocked: return "Session is closed to new players.";
                        case LobbyExceptionReason.InvalidJoinCode: return "Invalid join code. Check the six-character code with the host.";
                        case LobbyExceptionReason.LobbyNotFound: return "Session no longer exists. Ask the host for a new code.";
                    }
                }
                if (inner is AuthenticationException) return "Authentication failed. Check your connection and retry.";
                if (inner is TimeoutException || inner is OperationCanceledException) return "Connection timed out. Check your connection and retry.";
            }
            if (exception is SessionException session)
            {
                switch (session.Error)
                {
                    case SessionError.SessionNotFound:
                    case SessionError.SessionDeleted:
                    case SessionError.AllocationNotFound: return "Session no longer exists. Ask the host for a new code.";
                    case SessionError.InvalidSessionIdentifier: return "Invalid join code. Check the code with the host.";
                    case SessionError.NotAuthorized:
                    case SessionError.Forbidden: return "Online access was refused. Check Authentication and Multiplayer setup in Unity Dashboard.";
                    case SessionError.RateLimitExceeded: return "Too many attempts. Wait a few seconds, then retry.";
                    case SessionError.NetworkManagerStartFailed:
                    case SessionError.NetworkSetupFailed: return "Relay connection failed or timed out. Check the host and retry.";
                }
            }
            return "Unity Services unavailable or session connection failed. Check your connection and retry.";
        }
    }
}
