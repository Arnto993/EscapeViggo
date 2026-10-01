using System;
using System.IO;
using EscapeViggo.Presentation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EscapeViggo.Editor
{
    public static class AudioSetup
    {
        public const string SettingsPath = "Assets/EscapeViggo/ScriptableObjects/GameFeel.asset";
        private const string AudioPath = "Assets/EscapeViggo/Audio/Development";
        private static readonly string[] TaskPaths = { TaskSetup.PrefabPath, ElectricalTaskSetup.VoltagePath,
            ElectricalTaskSetup.OutletPath, ElectricalTaskSetup.BreakerPath };

        [MenuItem("Escape Viggo/Setup/Apply M21 Audio and Game Feel")]
        public static void Apply()
        {
            var settings = AssetDatabase.LoadAssetAtPath<GameFeelSettings>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<GameFeelSettings>();
                Directory.CreateDirectory(AudioPath);
                var cues = (AudioCue[])Enum.GetValues(typeof(AudioCue));
                settings.Cues = new AudioCueSettings[cues.Length];
                for (var i = 0; i < cues.Length; i++)
                {
                    var cue = cues[i];
                    var voice = cue >= AudioCue.VoiceJavell;
                    var loop = cue == AudioCue.Ambience || cue == AudioCue.RestLoop || cue == AudioCue.RageLoop;
                    var entry = new AudioCueSettings { Cue = cue, Bus = voice ? AudioBus.Voice : loop ? AudioBus.Ambience : AudioBus.Effects,
                        Volume = loop ? 0.18f : 0.5f, PitchVariation = cue <= AudioCue.ViggoStep ? 0.08f : 0,
                        Cooldown = voice ? 0.5f : 0.08f };
                    // Never generate speech. All voice slots remain assignable and empty until approved.
                    if (!voice)
                    {
                        var path = AudioPath + "/" + cue + ".wav";
                        if (!File.Exists(path)) WritePlaceholder(path, cue, loop);
                        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                        entry.Clips = new[] { AssetDatabase.LoadAssetAtPath<AudioClip>(path) };
                    }
                    settings.Cues[i] = entry;
                }
                AssetDatabase.CreateAsset(settings, SettingsPath);
            }
            Wire<PlayerAudioFeedback>(NetworkSetup.PrefabPath, settings, true);
            Wire<WorksiteAudioFeedback>(WorkdaySetup.PrefabPath, settings, false);
            Wire<BatteryAudioFeedback>(BatterySetup.PrefabPath, settings, true);
            foreach (var path in TaskPaths) Wire<TaskAudioFeedback>(path, settings, true);
            AssetDatabase.SaveAssets(); Validate();
        }
        private static void Wire<T>(string path, GameFeelSettings settings, bool spatial) where T : Component
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var emitter = root.GetComponent<AudioEmitter>() ?? root.AddComponent<AudioEmitter>();
                PlayerBuilder.SetReference(emitter, "_settings", settings);
                var serialized = new SerializedObject(emitter);
                serialized.FindProperty("_spatial").boolValue = spatial;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                if (root.GetComponent<T>() == null) root.AddComponent<T>();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static void WritePlaceholder(string path, AudioCue cue, bool loop)
        {
            const int rate = 22050;
            var step = cue <= AudioCue.ViggoStep;
            var seconds = loop ? 2f : step ? 0.09f : cue == AudioCue.DayEnded || cue == AudioCue.WeekResults ? 0.7f : 0.25f;
            var count = (int)(rate * seconds);
            using var writer = new BinaryWriter(File.Create(path));
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
            writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
            var frequency = cue switch { AudioCue.ElectricianStep => 150f, AudioCue.ViggoStep => 75f,
                AudioCue.Ambience => 80f, AudioCue.RestLoop => 140f, AudioCue.RageLoop => 60f,
                AudioCue.TaskFailed => 120f, AudioCue.TaskCompleted => 880f, AudioCue.Exhausted => 180f,
                AudioCue.Capture => 100f, AudioCue.RageStarted => 90f, _ => 260f + (int)cue * 29f };
            for (var i = 0; i < count; i++)
            {
                var t = (float)i / rate;
                var envelope = loop ? 0.35f + 0.25f * Mathf.Sin(2 * Mathf.PI * t) :
                    Mathf.Min(1, t / 0.008f) * Mathf.Pow(1 - t / seconds, step ? 3 : 1);
                var chirp = loop || step ? 0 : (cue == AudioCue.TaskFailed || cue == AudioCue.Exhausted ? -80f : 100f);
                var wave = Mathf.Sin(2 * Mathf.PI * (frequency * t + chirp * t * t));
                writer.Write((short)(wave * envelope * 0.25f * short.MaxValue));
            }
        }
        [MenuItem("Escape Viggo/Validation/Validate M21 Audio")]
        public static void Validate()
        {
            DistractionSetup.Validate();
            var settings = AssetDatabase.LoadAssetAtPath<GameFeelSettings>(SettingsPath);
            WorksiteValidation.Require(settings != null, "Missing GameFeel settings.");
            foreach (AudioCue cue in Enum.GetValues(typeof(AudioCue)))
            {
                var entry = settings.Find(cue);
                WorksiteValidation.Require(entry != null, "Missing audio hook: " + cue);
                WorksiteValidation.Require(Array.FindAll(settings.Cues, x => x != null && x.Cue == cue).Length == 1, "Duplicate audio hook: " + cue);
                if (cue < AudioCue.VoiceJavell) WorksiteValidation.Require(entry.Clips.Length > 0 && entry.Clips[0] != null, "Missing placeholder: " + cue);
            }
            Check<PlayerAudioFeedback>(NetworkSetup.PrefabPath, settings);
            Check<WorksiteAudioFeedback>(WorkdaySetup.PrefabPath, settings);
            Check<BatteryAudioFeedback>(BatterySetup.PrefabPath, settings);
            foreach (var path in TaskPaths) Check<TaskAudioFeedback>(path, settings);
            Debug.Log("M21 ASSET VALIDATION PASSED: cues, optional voices, player/world/battery/all task prefabs.");
        }
        private static void Check<T>(string path, GameFeelSettings settings) where T : Component
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            WorksiteValidation.Require(prefab.GetComponent<T>() != null && prefab.GetComponent<AudioEmitter>()?.Settings == settings,
                "Missing M21 wiring: " + path);
            foreach (var child in prefab.GetComponentsInChildren<Transform>(true))
                WorksiteValidation.Require(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0, "Missing script: " + path);
        }
    }
}
