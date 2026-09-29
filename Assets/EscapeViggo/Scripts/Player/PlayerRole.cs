using Unity.Netcode;

namespace EscapeViggo.Player
{
    public enum PlayerRole : byte { Electrician, Viggo }

    public static class RoleAssignment
    {
        // Listen-server prototype: the host is always present until the session ends.
        // Replace this policy when role selection/rotation is introduced.
        public static PlayerRole ForClient(ulong clientId) =>
            clientId == NetworkManager.ServerClientId ? PlayerRole.Viggo : PlayerRole.Electrician;
    }
}
