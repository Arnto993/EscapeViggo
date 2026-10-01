using EscapeViggo.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    [RequireComponent(typeof(AudioEmitter))]
    public sealed class TaskAudioFeedback : NetworkBehaviour
    {
        private WorkTask _task;
        private AudioEmitter _audio;
        private WorkTaskState _previous;
        public override void OnNetworkSpawn()
        {
            _task = GetComponent<WorkTask>(); _audio = GetComponent<AudioEmitter>();
            _previous = _task.State; // A late join receives a baseline, not historical sounds.
            if (IsClient) _task.StateChanged += Changed;
        }
        private void Changed(WorkTaskState current)
        {
            var cue = PresentationTransitions.Task(_previous, current);
            _previous = current;
            if (cue.HasValue) _audio.Play(cue.Value);
        }
        public override void OnNetworkDespawn()
        {
            _task.StateChanged -= Changed; _audio.StopAll();
        }
    }
}
