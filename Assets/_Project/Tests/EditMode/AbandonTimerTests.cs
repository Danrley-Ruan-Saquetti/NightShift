using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class AbandonTimerTests
    {
        private AbandonTimer timer;

        [SetUp]
        public void SetUp()
        {
            timer = new AbandonTimer(1.5f, 3f);
        }

        [Test]
        public void Tick_FreeAtExactlyMaxDistance_NeverExpires()
        {
            timer.Tick(10f, true, 1.5f);

            Assert.That(timer.IsExpired, Is.False);
        }

        [Test]
        public void Tick_FreeAndFarForRequiredTime_Expires()
        {
            timer.Tick(1.5f, true, 2f);
            timer.Tick(1.5f, true, 2f);

            Assert.That(timer.IsExpired, Is.True);
        }

        [Test]
        public void Tick_FreeAndFarForLess_DoesNotExpire()
        {
            timer.Tick(2f, true, 2f);

            Assert.That(timer.IsExpired, Is.False);
        }

        [Test]
        public void Tick_GrabbedInBetween_Restarts()
        {
            timer.Tick(2f, true, 2f);
            timer.Tick(0.1f, false, 2f);
            timer.Tick(2f, true, 2f);

            Assert.That(timer.IsExpired, Is.False);
        }

        [Test]
        public void Tick_ApproachedInBetween_Restarts()
        {
            timer.Tick(2f, true, 2f);
            timer.Tick(0.1f, true, 1f);
            timer.Tick(2f, true, 2f);

            Assert.That(timer.IsExpired, Is.False);
        }

        [Test]
        public void Tick_InSocketAndFar_NeverExpires()
        {
            timer.Tick(10f, false, 5f);

            Assert.That(timer.IsExpired, Is.False);
        }

        [Test]
        public void HorizontalDistance_ItemAtFeet_IsNear()
        {
            float distance = AbandonTimer.HorizontalDistance(new Vector3(0f, 0f, 0.2f), new Vector3(0f, 1.7f, 0f));

            Assert.That(distance, Is.EqualTo(0.2f).Within(0.0001f));
        }

        [Test]
        public void Reset_ClearsExpired()
        {
            timer.Tick(3f, true, 2f);

            timer.Reset();

            Assert.That(timer.IsExpired, Is.False);
        }
    }
}
