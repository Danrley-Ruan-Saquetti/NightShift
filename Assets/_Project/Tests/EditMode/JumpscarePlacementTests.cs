using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class JumpscarePlacementTests
    {
        private const float Tolerance = 1e-4f;

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(Tolerance), $"{actual} != {expected}");
        }

        [Test]
        public void FlatForward_IgnoresPitch()
        {
            Vector3 level = JumpscarePlacement.FlatForward(Quaternion.Euler(0f, 30f, 0f));

            AssertVector(JumpscarePlacement.FlatForward(Quaternion.Euler(-60f, 30f, 0f)), level);
            AssertVector(JumpscarePlacement.FlatForward(Quaternion.Euler(60f, 30f, 0f)), level);
            Assert.That(level.y, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(level.magnitude, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void FlatForward_NearlyVertical_StillUnit()
        {
            Vector3 level = JumpscarePlacement.FlatForward(Quaternion.Euler(0f, 30f, 0f));

            foreach (float pitch in new[] { 89f, -89f })
            {
                Vector3 result = JumpscarePlacement.FlatForward(Quaternion.Euler(pitch, 30f, 0f));
                Assert.That(result.magnitude, Is.EqualTo(1f).Within(Tolerance));
                Assert.That(result.y, Is.EqualTo(0f).Within(Tolerance));
                AssertVector(result, level);
            }
        }

        [Test]
        public void Place_IsAtDistanceAndEyeHeight()
        {
            Vector3 head = new Vector3(1f, 1.6f, 2f);
            Vector3 direction = JumpscarePlacement.FlatForward(Quaternion.Euler(-45f, 90f, 0f));

            Pose pose = JumpscarePlacement.Place(head, direction, 0.6f);

            AssertVector(pose.position, new Vector3(1.6f, 1.6f, 2f));
            Vector3 flat = pose.position - head;
            flat.y = 0f;
            Assert.That(flat.magnitude, Is.EqualTo(0.6f).Within(Tolerance));
        }

        [Test]
        public void Place_FacesHead()
        {
            Vector3 head = new Vector3(1f, 1.6f, 2f);
            Vector3 direction = JumpscarePlacement.FlatForward(Quaternion.Euler(-45f, 90f, 0f));

            Pose pose = JumpscarePlacement.Place(head, direction, 0.6f);

            AssertVector(pose.rotation * Vector3.forward, (head - pose.position).normalized);
        }

        [Test]
        public void ResolveDistance_NoObstacle_KeepsPreferred()
        {
            Assert.That(JumpscarePlacement.ResolveDistance(0.6f, float.PositiveInfinity, 0.1f, 0.35f), Is.EqualTo(0.6f).Within(Tolerance));
        }

        [Test]
        public void ResolveDistance_ObstacleAtHalfMeter_PullsCloser()
        {
            Assert.That(JumpscarePlacement.ResolveDistance(0.6f, 0.5f, 0.1f, 0.35f), Is.EqualTo(0.4f).Within(Tolerance));
        }

        [Test]
        public void ResolveDistance_ObstacleTooClose_ClampsToMinimum()
        {
            Assert.That(JumpscarePlacement.ResolveDistance(0.6f, 0.2f, 0.1f, 0.35f), Is.EqualTo(0.35f).Within(Tolerance));
        }

        [Test]
        public void ResolveDistance_ObstacleBeyondPreferred_KeepsPreferred()
        {
            Assert.That(JumpscarePlacement.ResolveDistance(0.6f, 0.9f, 0.1f, 0.35f), Is.EqualTo(0.6f).Within(Tolerance));
        }

        [Test]
        public void AdvancedDistance_MovesCloser()
        {
            Assert.That(JumpscarePlacement.AdvancedDistance(0.6f, 0.1f, 0.3f), Is.EqualTo(0.5f).Within(Tolerance));
        }

        [Test]
        public void AdvancedDistance_NeverBelowMinimum()
        {
            Assert.That(JumpscarePlacement.AdvancedDistance(0.35f, 0.1f, 0.3f), Is.EqualTo(0.3f).Within(Tolerance));
        }

        [Test]
        public void ChooseDirection_InCloset_UsesExitAxis()
        {
            Vector3 exit = new Vector3(0f, 0.2f, 1f);

            foreach (Quaternion look in new[] { Quaternion.Euler(0f, 0f, 0f), Quaternion.Euler(0f, 180f, 0f), Quaternion.Euler(70f, 90f, 0f) })
            {
                AssertVector(JumpscarePlacement.ChooseDirection(look, exit), new Vector3(0f, 0f, 1f));
            }
        }

        [Test]
        public void ChooseDirection_OutsideCloset_UsesYaw()
        {
            AssertVector(JumpscarePlacement.ChooseDirection(Quaternion.Euler(20f, 90f, 0f), null), new Vector3(1f, 0f, 0f));
        }
    }
}
