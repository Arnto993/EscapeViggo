using EscapeViggo.Battery;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    [RequireComponent(typeof(AudioEmitter), typeof(BatteryStation))]
    public sealed class BatteryAudioFeedback : NetworkBehaviour
    {
        private BatteryStation _station;
        private AudioEmitter _audio;
        private BatteryState _previous;
        public override void OnNetworkSpawn()
        {
            _station = GetComponent<BatteryStation>(); _audio = GetComponent<AudioEmitter>();
            _previous = _station.State;
        }
        private void Update()
        {
            if (!IsSpawned || !IsClient) return;
            var current = _station.State;
            if (_station.IsWorkday)
            {
                var cue = PresentationTransitions.Battery(_previous, current);
                if (cue.HasValue) _audio.Play(cue.Value);
                if (!_previous.Discovered && current.Discovered) _audio.Play(AudioCue.BatteryDiscovered);
                if (cue == AudioCue.BatteryRecovered) _audio.Play(AudioCue.VoiceBatteryRecovered);
            }
            _previous = current;
        }
        public override void OnNetworkDespawn() => _audio.StopAll();
    }
}
