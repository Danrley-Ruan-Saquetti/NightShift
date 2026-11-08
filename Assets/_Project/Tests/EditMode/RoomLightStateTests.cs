using System.Collections.Generic;
using NUnit.Framework;

namespace NightShift.Tests
{
    public class RoomLightStateTests
    {
        private RoomLightState state;
        private List<bool> changes;

        [SetUp]
        public void SetUp()
        {
            state = new RoomLightState();
            changes = new List<bool>();
            state.Changed += changes.Add;
        }

        [Test]
        public void StartsOn()
        {
            Assert.That(state.IsOn, Is.True);
            Assert.That(state.IsFlickering, Is.False);
        }

        [Test]
        public void Toggle_TurnsOffAndNotifiesOnce()
        {
            state.Toggle();

            Assert.That(state.IsOn, Is.False);
            Assert.That(changes, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void SetOn_SameValue_DoesNotNotify()
        {
            state.SetOn(true);

            Assert.That(changes, Is.Empty);
        }

        [Test]
        public void Flicker_KeepsIsOn()
        {
            state.StartFlicker(5f);

            Assert.That(state.IsFlickering, Is.True);
            Assert.That(state.IsOn, Is.True);
            Assert.That(changes, Is.Empty);
        }

        [Test]
        public void Flicker_EndsAfterDuration()
        {
            state.StartFlicker(1f);

            state.Tick(0.6f);
            Assert.That(state.IsFlickering, Is.True);

            state.Tick(0.6f);
            Assert.That(state.IsFlickering, Is.False);
            Assert.That(state.IsOn, Is.True);
        }

        [Test]
        public void TurnOffDuringFlicker_StopsFlicker()
        {
            state.StartFlicker(5f);

            state.SetOn(false);

            Assert.That(state.IsFlickering, Is.False);
            Assert.That(state.IsOn, Is.False);
            Assert.That(changes, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void FlickerWhileOff_DoesNothing()
        {
            state.SetOn(false);

            state.StartFlicker(5f);

            Assert.That(state.IsFlickering, Is.False);
        }

        [Test]
        public void StopFlicker_Stops()
        {
            state.StartFlicker(5f);

            state.StopFlicker();

            Assert.That(state.IsFlickering, Is.False);
            Assert.That(state.IsOn, Is.True);
        }

        [Test]
        public void Reset_TurnsOnWithoutFlicker()
        {
            state.SetOn(false);

            state.Reset();

            Assert.That(state.IsOn, Is.True);
            Assert.That(state.IsFlickering, Is.False);
            Assert.That(changes, Is.EqualTo(new[] { false, true }));

            state.StartFlicker(5f);
            state.Reset();

            Assert.That(state.IsFlickering, Is.False);
        }
    }
}
