using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class HeadOcclusionTests
    {
        private static readonly Vector3 Start = new(0f, 1.6f, 0f);
        private static readonly Vector3 Ahead = new(0f, 1.6f, 0.5f);
        private static readonly Vector3 Beyond = new(0f, 1.6f, 1f);

        [Test]
        public void New_IsClearAtStart()
        {
            HeadOcclusion occlusion = new(Start);

            AssertOcclusion(occlusion, HeadOcclusionState.Clear, false, Start);
        }

        [Test]
        public void Overlapping_IsInside_KeepsLastValid()
        {
            HeadOcclusion occlusion = new(Start);

            occlusion.Evaluate(Ahead, true, true);

            AssertOcclusion(occlusion, HeadOcclusionState.Inside, true, Start);
        }

        [Test]
        public void FreeWithLine_IsClear_RecordsHead()
        {
            HeadOcclusion occlusion = new(Start);

            occlusion.Evaluate(Ahead, false, true);

            AssertOcclusion(occlusion, HeadOcclusionState.Clear, false, Ahead);
        }

        [Test]
        public void PassedThroughBlind_StaysDark()
        {
            HeadOcclusion occlusion = new(Start);

            occlusion.Evaluate(Ahead, true, false);
            occlusion.Evaluate(Beyond, false, false);

            AssertOcclusion(occlusion, HeadOcclusionState.Blind, true, Start);
        }

        [Test]
        public void BlindThenLineRestored_Clears()
        {
            HeadOcclusion occlusion = CreateBlind();
            Vector3 returned = new(0.1f, 1.6f, 0f);

            occlusion.Evaluate(returned, false, true);

            AssertOcclusion(occlusion, HeadOcclusionState.Clear, false, returned);
        }

        [Test]
        public void Reset_ClearsAndMovesLastValid()
        {
            HeadOcclusion occlusion = CreateBlind();
            Vector3 teleported = new(5f, 1.6f, 5f);

            occlusion.Reset(teleported);

            AssertOcclusion(occlusion, HeadOcclusionState.Clear, false, teleported);
        }

        private static HeadOcclusion CreateBlind()
        {
            HeadOcclusion occlusion = new(Start);
            occlusion.Evaluate(Beyond, false, false);
            return occlusion;
        }

        private static void AssertOcclusion(
            HeadOcclusion occlusion,
            HeadOcclusionState expectedState,
            bool expectedDark,
            Vector3 expectedLastValid)
        {
            Assert.That(occlusion.State, Is.EqualTo(expectedState));
            Assert.That(occlusion.IsDark, Is.EqualTo(expectedDark));
            Assert.That(occlusion.LastValidPosition, Is.EqualTo(expectedLastValid));
        }
    }
}
