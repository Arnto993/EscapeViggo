using EscapeViggo.Core;
using EscapeViggo.Viggo;
using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Presentation
{
    [RequireComponent(typeof(AudioEmitter), typeof(NetworkWorkday))]
    public sealed class WorksiteAudioFeedback : NetworkBehaviour
    {
        private AudioEmitter _audio;
        private NetworkWorkday _workday;
        private WorkWeek _week;
        private ViggoRage _rage;
        private WorkDistractions _power;
        private MatchPhase _phase;
        private bool _raging, _final;
        public override void OnNetworkSpawn()
        {
            _audio = GetComponent<AudioEmitter>(); _workday = GetComponent<NetworkWorkday>();
            _week = GetComponent<WorkWeek>(); _rage = GetComponent<ViggoRage>(); _power = GetComponent<WorkDistractions>();
            _phase = _workday.State.Phase; _raging = _rage.Active;
            // Results lists may arrive after the initial variables. A Friday results join stays silent.
            _final = _phase == MatchPhase.EndOfDay && _week.Day == WeekRules.LastDay;
            if (!IsClient) return;
            _power.PowerCut += Cut; _power.PowerRestored += Restored; _power.WorkInterrupted += Interrupted;
        }
        private void Cut() => _audio.Play(AudioCue.PowerCut);
        private void Restored() { if (_workday.State.Phase == MatchPhase.Workday) _audio.Play(AudioCue.PowerRestored); }
        private void Interrupted(ulong victim) => _audio.Play(AudioCue.WorkInterrupted);
        private void Update()
        {
            if (!IsSpawned || !IsClient) return;
            var phase = _workday.State.Phase;
            var working = phase == MatchPhase.Workday;
            if (phase != _phase)
            {
                if (working) _audio.Play(AudioCue.DayStarted);
                if (phase == MatchPhase.EndOfDay) _audio.Play(AudioCue.DayEnded);
            }
            if (_rage.Active != _raging && working)
            {
                _audio.Play(_rage.Active ? AudioCue.RageStarted : AudioCue.RageEnded);
                if (_rage.Active) _audio.Play(AudioCue.VoiceRage);
            }
            if (!_final && _week.IsFinal) { _audio.Play(AudioCue.WeekResults); _final = true; }
            _audio.SetLoop(0, AudioCue.Ambience, working && !_power.PowerOff);
            _audio.SetLoop(1, AudioCue.RageLoop, working && _rage.Active);
            _phase = phase; _raging = _rage.Active;
        }
        public override void OnNetworkDespawn()
        {
            _power.PowerCut -= Cut; _power.PowerRestored -= Restored; _power.WorkInterrupted -= Interrupted;
            _audio.StopAll();
        }
    }
}
