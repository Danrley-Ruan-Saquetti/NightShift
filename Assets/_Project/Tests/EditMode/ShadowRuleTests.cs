using NUnit.Framework;

namespace NightShift.Tests
{
    public class ShadowRuleTests
    {
        private const float Step = 0.125f;

        private ShadowRule rule;

        [SetUp]
        public void SetUp()
        {
            rule = new ShadowRule(5f, 8f);
        }

        [Test]
        public void SwitchesOffAndWaits_Succeeds()
        {
            Advance(2f, true, false);

            EventOutcome beforeEnd = Advance(8f - Step, false, false);
            ShadowPhase phase = rule.Phase;
            EventOutcome atEnd = Advance(Step, false, false);

            Assert.That(phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(beforeEnd.Status, Is.EqualTo(EventStatus.Running));
            Assert.That(atEnd.Status, Is.EqualTo(EventStatus.Succeeded));
        }

        [Test]
        public void HiddenAtStart_LeavesAndSwitchesOff_Succeeds()
        {
            Advance(2f, true, true);
            Advance(1f, true, false);

            Advance(8f - Step, false, false);
            EventOutcome atEnd = Advance(Step, false, false);

            Assert.That(rule.Phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(atEnd.Status, Is.EqualTo(EventStatus.Succeeded));
        }

        [Test]
        public void SwitchOffThenLeaveCloset_Succeeds()
        {
            Advance(1f, false, true);
            ShadowPhase whileHidden = rule.Phase;

            Advance(1f, false, false);

            Assert.That(whileHidden, Is.EqualTo(ShadowPhase.Reacting));
            Assert.That(rule.Phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(rule.Outcome.Status, Is.EqualTo(EventStatus.Running));
        }

        [Test]
        public void LeaveClosetThenSwitchOff_Succeeds()
        {
            Advance(1f, true, true);
            Advance(1f, true, false);
            ShadowPhase whileLit = rule.Phase;

            Advance(1f, false, false);

            Assert.That(whileLit, Is.EqualTo(ShadowPhase.Reacting));
            Assert.That(rule.Phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(rule.Outcome.Status, Is.EqualTo(EventStatus.Running));
        }

        [Test]
        public void LightStillOnAtFiveSeconds_FailsLightOn()
        {
            EventOutcome beforeEnd = Advance(5f - Step, true, false);
            EventOutcome atEnd = Advance(Step, true, false);

            Assert.That(beforeEnd.Status, Is.EqualTo(EventStatus.Running));
            AssertFailed(atEnd, GameOverReason.ShadowLightOn);
        }

        [Test]
        public void LightOnAndHiddenAtFiveSeconds_FailsLightOn()
        {
            EventOutcome outcome = Advance(5f, true, true);

            AssertFailed(outcome, GameOverReason.ShadowLightOn);
        }

        [Test]
        public void LightOffButHiddenAtFiveSeconds_FailsHidden()
        {
            EventOutcome outcome = Advance(5f, false, true);

            AssertFailed(outcome, GameOverReason.ShadowHidden);
        }

        [Test]
        public void ToggleOnAndOffDuringWindow_DoesNotFail()
        {
            for (int second = 0; second < 4; second++)
            {
                Advance(1f, second % 2 == 0, true);
            }

            EventOutcome outcome = rule.Tick(Step, false, false, false, false);

            Assert.That(rule.Phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(outcome.Status, Is.EqualTo(EventStatus.Running));
        }

        [Test]
        public void TurnsLightBackOnWhileHolding_FailsLightOn()
        {
            Advance(1f, false, false);

            EventOutcome outcome = rule.Tick(Step, true, false, false, false);

            AssertFailed(outcome, GameOverReason.ShadowLightOn);
        }

        [Test]
        public void HidesWhileHolding_FailsHidden()
        {
            Advance(1f, false, false);

            EventOutcome outcome = rule.Tick(Step, false, true, false, false);

            AssertFailed(outcome, GameOverReason.ShadowHidden);
        }

        [Test]
        public void SwitchesOffOtherRoom_DuringWindow_Fails()
        {
            Advance(1f, true, false);

            EventOutcome outcome = rule.Tick(Step, true, false, true, false);

            AssertFailed(outcome, GameOverReason.ShadowWrongRoomSwitch);
        }

        [Test]
        public void SwitchesOffOtherRoom_WhileHolding_Fails()
        {
            Advance(1f, false, false);

            EventOutcome outcome = rule.Tick(Step, false, false, true, false);

            AssertFailed(outcome, GameOverReason.ShadowWrongRoomSwitch);
        }

        [Test]
        public void Teleports_DuringWindow_Fails()
        {
            Advance(1f, true, false);

            EventOutcome outcome = rule.Tick(Step, true, false, false, true);

            AssertFailed(outcome, GameOverReason.ShadowTeleported);
        }

        [Test]
        public void Teleports_WhileHolding_Fails()
        {
            Advance(1f, false, false);

            EventOutcome outcome = rule.Tick(Step, false, false, false, true);

            AssertFailed(outcome, GameOverReason.ShadowTeleported);
        }

        [Test]
        public void LightAlreadyOff_NotHidden_HoldsImmediately()
        {
            rule.Tick(0f, false, false, false, false);
            ShadowPhase phase = rule.Phase;

            EventOutcome outcome = Advance(8f, false, false);

            Assert.That(phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(outcome.Status, Is.EqualTo(EventStatus.Succeeded));
        }

        [Test]
        public void LightAlreadyOff_Hidden_HasFiveSecondsToLeave()
        {
            ShadowRule stayingHidden = new(5f, 8f);

            Advance(4f, false, true);
            Advance(Step, false, false);
            EventOutcome stayed = Advance(stayingHidden, 5f, false, true);

            Assert.That(rule.Phase, Is.EqualTo(ShadowPhase.Holding));
            Assert.That(rule.Outcome.Status, Is.EqualTo(EventStatus.Running));
            AssertFailed(stayed, GameOverReason.ShadowHidden);
        }

        private EventOutcome Advance(float seconds, bool isLightOn, bool isHidden)
        {
            return Advance(rule, seconds, isLightOn, isHidden);
        }

        private static EventOutcome Advance(ShadowRule target, float seconds, bool isLightOn, bool isHidden)
        {
            int steps = (int)System.Math.Round(seconds / Step);
            EventOutcome outcome = target.Outcome;

            for (int i = 0; i < steps; i++)
            {
                outcome = target.Tick(Step, isLightOn, isHidden, false, false);
            }

            return outcome;
        }

        private static void AssertFailed(EventOutcome outcome, GameOverReason reason)
        {
            Assert.That(outcome.Status, Is.EqualTo(EventStatus.Failed));
            Assert.That(outcome.Reason, Is.EqualTo(reason));
        }
    }
}
