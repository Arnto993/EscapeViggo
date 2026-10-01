using System;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    public enum AudioCue
    {
        ElectricianStep, ViggoStep, Ambience, Tool, TaskFailed, TaskCompleted,
        BatteryHandling, BatteryStolen, BatteryDiscovered, BatteryRecovered,
        Exhausted, RestLoop, RestEnded, RageStarted, RageLoop, RageEnded,
        Capture, PowerCut, PowerRestored, WorkInterrupted, DayStarted, DayEnded, WeekResults,
        VoiceJavell, VoiceRage, VoiceCapture, VoiceBatteryRecovered
    }
    public enum AudioBus { Effects, Ambience, Voice }

    [Serializable]
    public sealed class AudioCueSettings
    {
        public AudioCue Cue;
        public AudioBus Bus;
        public AudioClip[] Clips = Array.Empty<AudioClip>();
        [Range(0, 1)] public float Volume = 0.5f;
        [Range(0.5f, 2)] public float Pitch = 1;
        [Range(0, 0.2f)] public float PitchVariation;
        [Min(0)] public float Cooldown = 0.08f;
        [Min(1)] public float Range = 18;
    }

    [CreateAssetMenu(menuName = "Escape Viggo/Audio and Game Feel")]
    public sealed class GameFeelSettings : ScriptableObject
    {
        [Range(0, 1)] public float MasterVolume = 0.7f;
        [Range(0, 1)] public float EffectsVolume = 1;
        [Range(0, 1)] public float AmbienceVolume = 0.35f;
        [Range(0, 1)] public float VoiceVolume = 1;
        [Min(0.1f)] public float ElectricianStride = 1.8f;
        [Min(0.1f)] public float ViggoStride = 2.1f;
        [Range(0, 1)] public float CrouchStepVolume = 0.35f;
        [Range(0.1f, 1)] public float MinimumStepInterval = 0.2f;
        [Range(0, 0.5f)] public float EdgeOpacity = 0.22f;
        public AudioCueSettings[] Cues = Array.Empty<AudioCueSettings>();

        public AudioCueSettings Find(AudioCue cue)
        {
            foreach (var entry in Cues) if (entry != null && entry.Cue == cue) return entry;
            return null;
        }
        public float Volume(AudioCueSettings cue) => Mathf.Clamp01(MasterVolume) * Mathf.Clamp01(cue.Volume) *
            Mathf.Clamp01(cue.Bus == AudioBus.Voice ? VoiceVolume : cue.Bus == AudioBus.Ambience ? AmbienceVolume : EffectsVolume);
    }
}
