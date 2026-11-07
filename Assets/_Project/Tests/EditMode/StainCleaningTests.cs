using NUnit.Framework;

namespace NightShift.Tests
{
    public class StainCleaningTests
    {
        private const float RequiredScrubTime = 3f;
        private const float Tolerance = 0.0001f;

        [Test]
        public void Scrub_Accumulates()
        {
            StainCleaning cleaning = new(RequiredScrubTime, false);

            cleaning.Scrub(1f);

            Assert.That(cleaning.Progress, Is.EqualTo(1f / 3f).Within(Tolerance));
            Assert.That(cleaning.IsCleaned, Is.False);
        }

        [Test]
        public void Scrub_ResumesAfterPause()
        {
            StainCleaning cleaning = new(RequiredScrubTime, false);

            cleaning.Scrub(1f);
            cleaning.Scrub(1f);

            Assert.That(cleaning.Progress, Is.EqualTo(2f / 3f).Within(Tolerance));
        }

        [Test]
        public void Scrub_ReachingRequiredTime_Cleans()
        {
            StainCleaning cleaning = new(RequiredScrubTime, false);
            int cleanedCount = 0;
            cleaning.Cleaned += () => cleanedCount++;

            cleaning.Scrub(2f);
            Assert.That(cleaning.IsCleaned, Is.False);

            cleaning.Scrub(0.5f);
            Assert.That(cleaning.IsCleaned, Is.False);

            cleaning.Scrub(0.5f);
            Assert.That(cleaning.IsCleaned, Is.True);
            Assert.That(cleanedCount, Is.EqualTo(1));
        }

        [Test]
        public void Scrub_AfterCleaned_DoesNotRaiseAgain()
        {
            StainCleaning cleaning = new(RequiredScrubTime, false);
            int cleanedCount = 0;
            cleaning.Cleaned += () => cleanedCount++;
            cleaning.Scrub(3f);

            cleaning.Scrub(1f);

            Assert.That(cleanedCount, Is.EqualTo(1));
            Assert.That(cleaning.Progress, Is.EqualTo(1f));
        }

        [Test]
        public void ToughStain_ScrubBeforeSpray_DoesNothing()
        {
            StainCleaning cleaning = new(RequiredScrubTime, true);

            cleaning.Scrub(5f);

            Assert.That(cleaning.Progress, Is.EqualTo(0f));
            Assert.That(cleaning.IsCleaned, Is.False);
        }

        [Test]
        public void ToughStain_AfterSpray_CleansNormally()
        {
            StainCleaning cleaning = new(RequiredScrubTime, true);

            cleaning.Spray();
            cleaning.Scrub(3f);

            Assert.That(cleaning.IsCleaned, Is.True);
        }

        [Test]
        public void ToughStain_SprayTwice_SameAsOnce()
        {
            StainCleaning cleaning = new(RequiredScrubTime, true);

            cleaning.Spray();
            cleaning.Spray();
            cleaning.Scrub(1f);

            Assert.That(cleaning.Progress, Is.EqualTo(1f / 3f).Within(Tolerance));
            Assert.That(cleaning.IsSprayed, Is.True);
        }

        [Test]
        public void ToughStain_ScrubBeforeSprayDoesNotCountLater()
        {
            StainCleaning cleaning = new(RequiredScrubTime, true);

            cleaning.Scrub(2f);
            cleaning.Spray();
            cleaning.Scrub(1f);

            Assert.That(cleaning.Progress, Is.EqualTo(1f / 3f).Within(Tolerance));
        }
    }
}
