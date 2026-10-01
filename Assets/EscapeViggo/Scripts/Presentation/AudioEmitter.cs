using System;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    // Local presentation only. Fixed voice budget; no network messages or gameplay callbacks.
    public sealed class AudioEmitter : MonoBehaviour
    {
        [SerializeField] private GameFeelSettings _settings;
        [SerializeField] private bool _spatial = true;
        private readonly AudioSource[] _shots = new AudioSource[4];
        private readonly AudioSource[] _loops = new AudioSource[2];
        private readonly AudioCue?[] _loopCues = new AudioCue?[2];
        private AudioSource _voice;
        private float[] _readyAt;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool _trace;
#endif
        public GameFeelSettings Settings => _settings;
        // Also useful to local captions/accessibility and development validation, including empty voice slots.
        public event Action<AudioCue> CueRequested;

        private void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var args = Environment.GetCommandLineArgs();
            _trace = Array.IndexOf(args, "-ev-smoke") >= 0 || Array.IndexOf(args, "-ev-week-smoke") >= 0;
#endif
            _readyAt = new float[Enum.GetValues(typeof(AudioCue)).Length];
            for (var i = 0; i < _shots.Length; i++) _shots[i] = CreateSource();
            for (var i = 0; i < _loops.Length; i++) { _loops[i] = CreateSource(); _loops[i].loop = true; }
            _voice = CreateSource();
        }
        private AudioSource CreateSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.dopplerLevel = 0;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 2;
            return source;
        }
        private void Configure(AudioSource source, AudioCueSettings cue, float scale)
        {
            source.spatialBlend = _spatial ? 1 : 0;
            source.maxDistance = Mathf.Max(2, cue.Range);
            source.volume = _settings.Volume(cue) * Mathf.Clamp01(scale);
            source.pitch = Mathf.Clamp(cue.Pitch + UnityEngine.Random.Range(-cue.PitchVariation, cue.PitchVariation), 0.5f, 2);
        }
        private static AudioClip Clip(AudioCueSettings cue) => cue.Clips == null || cue.Clips.Length == 0
            ? null : cue.Clips[UnityEngine.Random.Range(0, cue.Clips.Length)];

        public void Play(AudioCue cue, float scale = 1)
        {
            if (!isActiveAndEnabled || _settings == null || (int)cue < 0 || (int)cue >= _readyAt.Length) return;
            var entry = _settings.Find(cue);
            if (entry == null || Time.unscaledTime < _readyAt[(int)cue]) return;
            _readyAt[(int)cue] = Time.unscaledTime + Mathf.Max(0, entry.Cooldown);
            CueRequested?.Invoke(cue);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_trace) Debug.Log($"EV_AUDIO cue={cue} object={GetComponent<Unity.Netcode.NetworkObject>()?.NetworkObjectId}", this);
#endif
            var clip = Clip(entry);
            if (clip == null) return; // Approved voice assets are optional.
            AudioSource source = entry.Bus == AudioBus.Voice ? _voice : null;
            if (source == null) foreach (var candidate in _shots) if (!candidate.isPlaying) { source = candidate; break; }
            if (source == null) return; // Bound overlapping effects instead of stacking unbounded one-shots.
            Configure(source, entry, scale);
            source.clip = clip;
            source.Play();
        }

        public void SetLoop(int channel, AudioCue cue, bool active)
        {
            if (channel < 0 || channel >= _loops.Length || _loops[channel] == null) return;
            var source = _loops[channel];
            var entry = _settings != null ? _settings.Find(cue) : null;
            if (!active || !isActiveAndEnabled || entry == null)
            {
                source.Stop(); _loopCues[channel] = null; return;
            }
            source.volume = _settings.Volume(entry);
            if (_loopCues[channel] == cue) return;
            source.Stop(); _loopCues[channel] = cue;
            source.clip = Clip(entry);
            if (source.clip == null) return;
            Configure(source, entry, 1);
            source.Play();
        }
        public void StopAll()
        {
            foreach (var source in _shots) if (source != null) source.Stop();
            foreach (var source in _loops) if (source != null) source.Stop();
            if (_voice != null) _voice.Stop();
            Array.Clear(_loopCues, 0, _loopCues.Length);
            if (_readyAt != null) Array.Clear(_readyAt, 0, _readyAt.Length);
        }
        private void OnDisable() => StopAll();
    }
}
