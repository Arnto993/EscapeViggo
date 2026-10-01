#if UNITY_EDITOR
using System.Collections;
using EscapeViggo.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace EscapeViggo.Tests
{
    public sealed class AudioEmitterTests
    {
        [UnityTest]
        public IEnumerator EmptyVoiceCooldownAndDisableAreSafe()
        {
            var root = new GameObject("Audio test"); root.SetActive(false);
            var settings = ScriptableObject.CreateInstance<GameFeelSettings>();
            settings.Cues = new[] { new AudioCueSettings { Cue = AudioCue.VoiceJavell, Bus = AudioBus.Voice, Cooldown = 100 } };
            var emitter = root.AddComponent<AudioEmitter>();
            var serialized = new SerializedObject(emitter);
            serialized.FindProperty("_settings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            root.SetActive(true);
            var count = 0; emitter.CueRequested += _ => count++;
            try
            {
                emitter.Play(AudioCue.VoiceJavell); emitter.Play(AudioCue.VoiceJavell);
                Assert.That(count, Is.EqualTo(1), "Duplicate in cooldown must not replay.");
                Assert.That(root.GetComponents<AudioSource>().Length, Is.EqualTo(7), "Source budget must remain bounded.");
                emitter.SetLoop(0, AudioCue.VoiceJavell, true);
                yield return null;
                root.SetActive(false);
                foreach (var source in root.GetComponents<AudioSource>()) Assert.That(source.isPlaying, Is.False);
                emitter.Play(AudioCue.VoiceJavell); Assert.That(count, Is.EqualTo(1));
                root.SetActive(true); emitter.Play(AudioCue.VoiceJavell);
                Assert.That(count, Is.EqualTo(2), "Reused emitters reset cooldown on disable.");
            }
            finally { Object.Destroy(root); Object.Destroy(settings); }
        }
        [UnityTest]
        public IEnumerator LoopReplacementAndCleanupKeepFixedSourceCount()
        {
            var root = new GameObject("Loop test"); root.SetActive(false);
            var settings = ScriptableObject.CreateInstance<GameFeelSettings>();
            var clip = AudioClip.Create("Test tone", 22050, 1, 22050, false);
            settings.Cues = new[] { new AudioCueSettings { Cue = AudioCue.Ambience, Clips = new[] { clip } },
                new AudioCueSettings { Cue = AudioCue.RageLoop, Clips = new[] { clip } } };
            var emitter = root.AddComponent<AudioEmitter>();
            var serialized = new SerializedObject(emitter);
            serialized.FindProperty("_settings").objectReferenceValue = settings;
            serialized.ApplyModifiedPropertiesWithoutUndo(); root.SetActive(true);
            try
            {
                for (var i = 0; i < 20; i++) emitter.SetLoop(0, AudioCue.Ambience, true);
                emitter.SetLoop(0, AudioCue.RageLoop, true);
                yield return null;
                Assert.That(root.GetComponents<AudioSource>().Length, Is.EqualTo(7));
                emitter.StopAll();
                foreach (var source in root.GetComponents<AudioSource>()) Assert.That(source.isPlaying, Is.False);
            }
            finally { Object.Destroy(root); Object.Destroy(settings); Object.Destroy(clip); }
        }
    }
}
#endif
