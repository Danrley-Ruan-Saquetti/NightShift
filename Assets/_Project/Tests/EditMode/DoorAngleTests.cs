using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class DoorAngleTests
    {
        [Test]
        public void FromHandPosition_AlongClosedDirection_IsZero()
        {
            float angle = DoorAngle.FromHandPosition(new Vector3(0.7f, 0f, 0f));

            Assert.That(angle, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void FromHandPosition_AtNegativeZ_IsNinety()
        {
            float angle = DoorAngle.FromHandPosition(new Vector3(0f, 0f, -0.7f));

            Assert.That(angle, Is.EqualTo(90f).Within(0.01f));
        }

        [Test]
        public void FromHandPosition_MatchesUnityYRotation()
        {
            float angle = DoorAngle.FromHandPosition(Quaternion.Euler(0f, 30f, 0f) * Vector3.right);

            Assert.That(angle, Is.EqualTo(30f).Within(0.01f));
        }

        [Test]
        public void FromHandPosition_IgnoresHeight()
        {
            float angle = DoorAngle.FromHandPosition(new Vector3(0f, 1.5f, -0.7f));

            Assert.That(angle, Is.EqualTo(90f).Within(0.01f));
        }

        [Test]
        public void FromHandPosition_OppositeSide_IsNegative()
        {
            float angle = DoorAngle.FromHandPosition(new Vector3(0f, 0f, 0.7f));

            Assert.That(angle, Is.EqualTo(-90f).Within(0.01f));
        }
    }
}
