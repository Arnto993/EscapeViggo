using EscapeViggo.Networking;
using EscapeViggo.Player;
using EscapeViggo.Viggo;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    [RequireComponent(typeof(AudioEmitter), typeof(NetworkPlayer))]
    public sealed class PlayerAudioFeedback : NetworkBehaviour
    {
        private AudioEmitter _audio;
        private NetworkPlayer _player;
        private PlayerMovement _movement;
        private ViggoStamina _stamina;
        private PlayerCapture _capture;
        private LocalNetworkSession _session;
        private ViggoRage _rage;
        private uint _captureSequence;
        private bool _resting;
        private Vector3 _position;
        private readonly FootstepCadence _steps = new();
        public override void OnNetworkSpawn()
        {
            _audio = GetComponent<AudioEmitter>(); _player = GetComponent<NetworkPlayer>();
            _movement = GetComponent<PlayerMovement>(); _stamina = GetComponent<ViggoStamina>();
            _capture = GetComponent<PlayerCapture>(); _session = NetworkManager.GetComponent<LocalNetworkSession>();
            _captureSequence = _capture.State.Sequence; _resting = _stamina.IsResting;
            _position = transform.position; _steps.Reset(_position);
            if (IsClient) _stamina.RestStarted += RestStarted; // Same boundary as the existing Javell event.
        }
        private void RestStarted()
        {
            _audio.Play(AudioCue.Exhausted);
            _audio.Play(AudioCue.VoiceJavell);
        }
        private void LateUpdate()
        {
            if (!IsSpawned || !IsClient || _audio.Settings == null) return;
            if (_rage == null && _session.Workday != null) _rage = _session.Workday.GetComponent<ViggoRage>();
            var working = _session.Workday != null && _session.Workday.State.Phase == Core.MatchPhase.Workday;
            var resting = _stamina.IsResting && working;
            if (_resting && !resting && working && _player.Role == PlayerRole.Viggo && !_stamina.IsRaging) _audio.Play(AudioCue.RestEnded);
            _audio.SetLoop(0, AudioCue.RestLoop, resting);
            _resting = resting;
            if (_capture.State.Sequence > _captureSequence)
            {
                _audio.Play(AudioCue.Capture);
                foreach (var obj in NetworkManager.SpawnManager.SpawnedObjectsList)
                    if (obj.IsPlayerObject && obj.OwnerClientId == _capture.State.CaptorId)
                    { obj.GetComponent<AudioEmitter>()?.Play(AudioCue.VoiceCapture); break; }
            }
            _captureSequence = _capture.State.Sequence;
            var position = transform.position;
            var moving = (position - _position).sqrMagnitude > 0.000001f;
            // Remote controllers are disabled. Check rendered feet against the ground only while moving.
            var grounded = moving && (IsOwner ? _movement.IsGrounded :
                Physics.Raycast(position + Vector3.up * 0.15f, Vector3.down, 0.32f, ~0, QueryTriggerInteraction.Ignore));
            var settings = _audio.Settings;
            var viggo = _player.Role == PlayerRole.Viggo;
            if (_steps.Sample(position, grounded, !working || resting || _capture.IsCaptured,
                viggo ? settings.ViggoStride : settings.ElectricianStride, settings.MinimumStepInterval, Time.unscaledTime, Time.deltaTime))
                _audio.Play(viggo ? AudioCue.ViggoStep : AudioCue.ElectricianStep, _player.IsCrouching ? settings.CrouchStepVolume : 1);
            _position = position;
        }
        private void OnGUI()
        {
            if (!IsSpawned || !IsOwner || _player.GameplayStopped || _audio.Settings == null) return;
            var captured = _capture.IsCaptured;
            var raging = _rage != null && _rage.Active;
            if (!captured && !raging && !_resting) return;
            var color = captured ? new Color(1, 0.6f, 0.05f) : raging ? new Color(1, 0.12f, 0.05f) : new Color(0.2f, 0.6f, 1);
            // Slow breathing while resting, steady alarm while raging; no camera motion or flashes.
            color.a = _audio.Settings.EdgeOpacity * (_resting && !raging && !captured ? 0.65f + 0.35f * Mathf.Sin(Time.unscaledTime * 2) : 1);
            var old = GUI.color; GUI.color = color;
            var width = Mathf.Max(6, Screen.height * 0.025f);
            GUI.DrawTexture(new Rect(0, 0, width, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(Screen.width - width, 0, width, Screen.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(0, Screen.height - width, Screen.width, width), Texture2D.whiteTexture);
            GUI.color = old;
        }
        public override void OnNetworkDespawn()
        {
            _stamina.RestStarted -= RestStarted; _audio.StopAll(); _rage = null;
        }
    }
}
