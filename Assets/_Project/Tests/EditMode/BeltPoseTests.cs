using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class BeltPoseTests
    {
        private const float DropBelowHead = 0.6f;
        private const float SideOffset = 0.25f;
        private const float ForwardOffset = 0.1f;

        private static readonly Vector3 HeadPosition = new(0f, 1.6f, 0f);
        private static readonly Vector3 EastPosition = new(0.1f, 1f, 0.25f);
        private static readonly Quaternion EastRotation = Quaternion.Euler(0f, 90f, 0f);

        [Test]
        public void Compute_FacingNorth_LeftHipInFront()
        {
            Pose pose = Compute(Quaternion.identity);

            AssertPose(pose, new Vector3(-0.25f, 1f, 0.1f), Quaternion.identity);
        }

        [Test]
        public void Compute_FacingEast_FollowsYaw()
        {
            AssertPose(Compute(EastRotation), EastPosition, EastRotation);
        }

        [Test]
        public void Compute_IgnoresPitch()
        {
            AssertPose(Compute(Quaternion.Euler(60f, 90f, 0f)), EastPosition, EastRotation);
        }

        [Test]
        public void Compute_LookingStraightDown_KeepsYaw()
        {
            AssertPose(Compute(Quaternion.Euler(90f, 90f, 0f)), EastPosition, EastRotation);
        }

        [Test]
        public void Compute_LookingStraightUp_KeepsYaw()
        {
            AssertPose(Compute(Quaternion.Euler(-90f, 90f, 0f)), EastPosition, EastRotation);
        }

        [Test]
        public void Compute_RolledNearLevel_DoesNotJump()
        {
            Pose slightlyUp = Compute(Quaternion.Euler(-1f, 90f, 10f));
            Pose slightlyDown = Compute(Quaternion.Euler(1f, 90f, 10f));

            Assert.That(Quaternion.Angle(slightlyUp.rotation, EastRotation), Is.LessThan(2f));
            Assert.That(Quaternion.Angle(slightlyDown.rotation, EastRotation), Is.LessThan(2f));
        }

        private static Pose Compute(Quaternion headRotation)
        {
            return BeltPose.Compute(HeadPosition, headRotation, DropBelowHead, SideOffset, ForwardOffset);
        }

        private static void AssertPose(Pose pose, Vector3 expectedPosition, Quaternion expectedRotation)
        {
            Assert.That(Vector3.Distance(pose.position, expectedPosition), Is.LessThan(0.001f));
            Assert.That(Quaternion.Angle(pose.rotation, expectedRotation), Is.LessThan(0.1f));
        }
    }
}
