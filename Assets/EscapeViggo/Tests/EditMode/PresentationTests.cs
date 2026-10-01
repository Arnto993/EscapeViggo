using EscapeViggo.Battery;
using EscapeViggo.Presentation;
using EscapeViggo.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace EscapeViggo.Tests
{
    public sealed class PresentationTests
    {
        [Test]
        public void TaskBaselineAndDuplicateRefreshAreSilent()
        {
            var state = new WorkTaskState { Phase = WorkTaskPhase.Completed, Revision = 12 };
            Assert.That(PresentationTransitions.Task(state, state), Is.Null);
            Assert.That(PresentationTransitions.Task(state, new WorkTaskState { Revision = 11 }), Is.Null);
        }
        [Test]
        public void TaskFailureCompletionAndResetRemainDistinct()
        {
            var before = new WorkTaskState { Phase = WorkTaskPhase.InProgress, Revision = 1 };
            var after = new WorkTaskState { Phase = WorkTaskPhase.InProgress, Revision = 2, WrongInput = true };
            Assert.That(PresentationTransitions.Task(before, after), Is.EqualTo(AudioCue.TaskFailed));
            before = after; after.Revision++;
            Assert.That(PresentationTransitions.Task(before, after), Is.EqualTo(AudioCue.TaskFailed));
            after.WrongInput = false;
            Assert.That(PresentationTransitions.Task(before, after), Is.EqualTo(AudioCue.Tool));
            after.Phase = WorkTaskPhase.Completed;
            Assert.That(PresentationTransitions.Task(before, after), Is.EqualTo(AudioCue.TaskCompleted));
            after.Phase = WorkTaskPhase.Available;
            Assert.That(PresentationTransitions.Task(before, after), Is.Null);
        }
        [Test]
        public void BatteryProgressAndDayResetDoNotReplayTheftOrRecovery()
        {
            var before = BatteryState.Initial;
            var after = before; after.Phase = BatteryPhase.BeingStolen;
            Assert.That(PresentationTransitions.Battery(before, after), Is.EqualTo(AudioCue.BatteryHandling));
            before = after; after.Progress = 0.5f;
            Assert.That(PresentationTransitions.Battery(before, after), Is.Null);
            after.TheftId++; after.Phase = BatteryPhase.Carried; after.OriginalThiefId = 2;
            Assert.That(PresentationTransitions.Battery(before, after), Is.EqualTo(AudioCue.BatteryStolen));
            before = after; after.Phase = BatteryPhase.Secured;
            Assert.That(PresentationTransitions.Battery(before, after), Is.EqualTo(AudioCue.BatteryRecovered));
            after.Phase = BatteryPhase.AtDrill;
            Assert.That(PresentationTransitions.Battery(before, after), Is.EqualTo(AudioCue.BatteryRecovered));
            after = BatteryState.Initial; after.TheftId = before.TheftId;
            Assert.That(PresentationTransitions.Battery(before, after), Is.Null);
        }
        [Test]
        public void StepsIgnoreTeleportAirborneAndBlockedMovement()
        {
            var cadence = new FootstepCadence(); cadence.Reset(Vector3.zero);
            Assert.That(cadence.Sample(Vector3.right * 20, true, false, 1, 0.2f, 1, 0.02f), Is.False);
            Assert.That(cadence.Sample(Vector3.right * 20.5f, false, false, 1, 0.2f, 2, 0.02f), Is.False);
            Assert.That(cadence.Sample(Vector3.right * 21, true, true, 1, 0.2f, 3, 0.02f), Is.False);
            Assert.That(cadence.Sample(Vector3.right * 21.5f, true, false, 1, 0.2f, 4, 0.02f), Is.False);
            Assert.That(cadence.Sample(Vector3.right * 22, true, false, 1, 0.2f, 5, 0.02f), Is.True);
            Assert.That(cadence.Sample(Vector3.right * 22.5f, true, false, 0.1f, 0.2f, 5.01f, 0.02f), Is.False);
        }
        [Test]
        public void MissingClipsAndMutedBusAreValidConfiguration()
        {
            var settings = ScriptableObject.CreateInstance<GameFeelSettings>();
            try
            {
                var cue = new AudioCueSettings { Cue = AudioCue.VoiceJavell, Bus = AudioBus.Voice };
                settings.Cues = new[] { cue }; settings.VoiceVolume = 0;
                Assert.That(settings.Find(AudioCue.VoiceJavell), Is.SameAs(cue));
                Assert.That(cue.Clips, Is.Empty);
                Assert.That(settings.Volume(cue), Is.Zero);
                Assert.That(settings.Find(AudioCue.RageStarted), Is.Null);
            }
            finally { Object.DestroyImmediate(settings); }
        }
    }
}
