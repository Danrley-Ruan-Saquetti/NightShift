using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class CartPlacementTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ToRigSpace_ThenFromSameRig_ReturnsOriginal()
        {
            Pose cart = new(new Vector3(1f, 0f, 2f), Quaternion.Euler(0f, 30f, 0f));
            Pose rig = new(new Vector3(3f, 0f, -1f), Quaternion.Euler(0f, 120f, 0f));

            Pose restored = CartPlacement.FromRigSpace(CartPlacement.ToRigSpace(cart, rig), rig, 0f);

            AssertPose(restored, cart.position, cart.rotation);
        }

        [Test]
        public void FromRigSpace_RigMovedAndTurned_KeepsOffsetInRigFrame()
        {
            Pose cart = new(new Vector3(0f, 0f, 1f), Quaternion.identity);
            Pose relative = CartPlacement.ToRigSpace(cart, Pose.identity);
            Pose movedRig = new(new Vector3(10f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f));

            Pose moved = CartPlacement.FromRigSpace(relative, movedRig, 0f);

            AssertPose(moved, new Vector3(11f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f));
        }

        [Test]
        public void FromRigSpace_UsesGivenHeight()
        {
            Pose rig = new(new Vector3(0f, 0.5f, 0f), Quaternion.identity);
            Pose relative = new(new Vector3(0f, 0.3f, 1f), Quaternion.identity);

            Pose result = CartPlacement.FromRigSpace(relative, rig, 0f);

            Assert.That(result.position.y, Is.EqualTo(0f).Within(Tolerance));
        }

        private static void AssertPose(Pose pose, Vector3 expectedPosition, Quaternion expectedRotation)
        {
            Assert.That(Vector3.Distance(pose.position, expectedPosition), Is.LessThan(Tolerance));
            Assert.That(Quaternion.Angle(pose.rotation, expectedRotation), Is.LessThan(0.1f));
        }
    }
}
