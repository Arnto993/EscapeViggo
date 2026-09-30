using Unity.Netcode;
using UnityEngine;

namespace EscapeViggo.Viggo
{
    [RequireComponent(typeof(ViggoRage))]
    public sealed class RagePresentation : NetworkBehaviour
    {
        private ViggoRage _rage;
        private void Awake() => _rage = GetComponent<ViggoRage>();
        public string WarningFor(bool viggo) => !IsSpawned || !_rage.Active ? string.Empty : viggo
            ? "BATTERY MISSING - RAGE!\nPursue workers without resting."
            : "VIGGO IS RAGING!\nRun, hide, and avoid being captured.";
    }
}
