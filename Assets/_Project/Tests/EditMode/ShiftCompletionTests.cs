using NUnit.Framework;

namespace NightShift.Tests
{
    public class ShiftCompletionTests
    {
        private ShiftCompletion completion;

        [SetUp]
        public void SetUp()
        {
            completion = new ShiftCompletion(15f);
        }

        [Test]
        public void TryUnlock_ThirteenOfFourteen_StaysLocked()
        {
            bool unlocked = completion.TryUnlock(13, 14, false);

            Assert.That(unlocked, Is.False);
            Assert.That(completion.IsUnlocked, Is.False);
        }

        [Test]
        public void TryUnlock_AllDoneNoEvent_UnlocksOnce()
        {
            bool first = completion.TryUnlock(14, 14, false);
            bool second = completion.TryUnlock(14, 14, false);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(completion.IsUnlocked, Is.True);
        }

        [Test]
        public void TryUnlock_AllDoneDuringEvent_WaitsForEventEnd()
        {
            bool during = completion.TryUnlock(14, 14, true);
            bool after = completion.TryUnlock(14, 14, false);

            Assert.That(during, Is.False);
            Assert.That(after, Is.True);
        }

        [Test]
        public void TryUnlock_AfterUnlock_StaysUnlocked()
        {
            completion.TryUnlock(14, 14, false);

            bool unlocked = completion.TryUnlock(0, 14, true);

            Assert.That(unlocked, Is.False);
            Assert.That(completion.IsUnlocked, Is.True);
        }

        [Test]
        public void TryUnlock_ZeroTotal_StaysLocked()
        {
            Assert.That(completion.TryUnlock(0, 0, false), Is.False);
        }

        [Test]
        public void TryExit_Locked_NeverExits()
        {
            Assert.That(completion.TryExit(90f), Is.False);
        }

        [Test]
        public void TryExit_BelowExitAngle_DoesNotExit()
        {
            completion.TryUnlock(14, 14, false);

            Assert.That(completion.TryExit(14.9f), Is.False);
        }

        [Test]
        public void TryExit_AtExitAngle_ExitsOnce()
        {
            completion.TryUnlock(14, 14, false);

            bool first = completion.TryExit(15f);
            bool second = completion.TryExit(60f);

            Assert.That(first, Is.True);
            Assert.That(second, Is.False);
            Assert.That(completion.HasExited, Is.True);
        }
    }
}
