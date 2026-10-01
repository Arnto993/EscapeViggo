using EscapeViggo.Interaction;
using EscapeViggo.Networking;
using EscapeViggo.Player;

namespace EscapeViggo.Viggo
{
    public sealed class PowerControl : NetworkInteractable
    {
        private WorkDistractions _distractions;
        public override string Prompt => "Worksite power control";
        public override string GetPrompt(PlayerInteraction player) => player.Player.Role == PlayerRole.Viggo ? "Cut worksite power" : "Restore worksite power";
        public override void OnNetworkSpawn() => _distractions = NetworkManager.GetComponent<LocalNetworkSession>().Workday.GetComponent<WorkDistractions>();
        public override bool CanInteract(PlayerInteraction player) => IsSpawned && _distractions != null && _distractions.CanUsePower(player);
        protected override void OnServerInteract(PlayerInteraction player) => _distractions.UsePowerOnServer(player, this);
    }
}
