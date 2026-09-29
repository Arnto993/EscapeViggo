using EscapeViggo.Player;
using NUnit.Framework;
using Unity.Netcode;

namespace EscapeViggo.Tests
{
    public sealed class RoleAssignmentTests
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(8)]
        public void ListenServerHasExactlyOneViggo(int count)
        {
            var viggos = 0;
            for (ulong id = 0; id < (ulong)count; id++)
            {
                var role = RoleAssignment.ForClient(id);
                if (role == PlayerRole.Viggo) viggos++;
                else Assert.That(role, Is.EqualTo(PlayerRole.Electrician));
            }
            Assert.That(viggos, Is.EqualTo(1));
            Assert.That(RoleAssignment.ForClient(NetworkManager.ServerClientId), Is.EqualTo(PlayerRole.Viggo));
        }

        [Test]
        public void ReconnectingClientDoesNotReplaceViggo()
        {
            Assert.That(RoleAssignment.ForClient(23), Is.EqualTo(PlayerRole.Electrician));
            Assert.That(RoleAssignment.ForClient(ulong.MaxValue), Is.EqualTo(PlayerRole.Electrician));
        }
    }
}
