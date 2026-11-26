using System;
using NUnit.Framework;

namespace NightShift.Tests
{
    public class JumpscareKillersTests
    {
        [TestCase(GameOverReason.Debug, Killer.Visitor)]
        [TestCase(GameOverReason.VisitorNotHidden, Killer.Visitor)]
        [TestCase(GameOverReason.VisitorLeftEarly, Killer.Visitor)]
        [TestCase(GameOverReason.VisitorTouchedEntranceHandle, Killer.Visitor)]
        [TestCase(GameOverReason.ShadowLightOn, Killer.Shadow)]
        [TestCase(GameOverReason.ShadowWrongRoomSwitch, Killer.Shadow)]
        [TestCase(GameOverReason.ShadowTeleported, Killer.Shadow)]
        [TestCase(GameOverReason.ShadowHidden, Killer.Shadow)]
        public void For_Reason_ReturnsItsKiller(GameOverReason reason, Killer expected)
        {
            Assert.That(JumpscareKillers.For(reason), Is.EqualTo(expected));
        }

        [Test]
        public void For_EveryReason_HasAKiller()
        {
            foreach (GameOverReason reason in Enum.GetValues(typeof(GameOverReason)))
            {
                Assert.DoesNotThrow(() => JumpscareKillers.For(reason), reason.ToString());
            }
        }
    }
}
