using NUnit.Framework;

namespace NightShift.Tests
{
    public class VisitorRuleTests
    {
        private const float Step = 0.125f;

        private VisitorRule rule;

        [SetUp]
        public void SetUp()
        {
            rule = new VisitorRule(4.5f, 10f, 8f);
        }

        [Test]
        public void HidesInWindow_StaysEightSeconds_Succeeds()
        {
            Advance(4.5f, false, false);
            Advance(2f, false, false);

            EventOutcome beforeEnd = Advance(8f - Step, true, false);
            EventOutcome atEnd = Advance(Step, true, false);

            Assert.That(beforeEnd.Status, Is.EqualTo(EventStatus.Running));
            Assert.That(atEnd.Status, Is.EqualTo(EventStatus.Succeeded));
        }

        [Test]
        public void NeverHides_FailsNotHiddenAtTenSeconds()
        {
            Advance(4.5f, false, false);

            EventOutcome beforeEnd = Advance(10f - Step, false, false);
            EventOutcome atEnd = Advance(Step, false, false);

            Assert.That(beforeEnd.Status, Is.EqualTo(EventStatus.Running));
            AssertFailed(atEnd, GameOverReason.VisitorNotHidden);
        }

        [Test]
        public void HiddenDuringRingsOnly_DoesNotCount()
        {
            Advance(4f, true, false);
            Advance(0.5f, false, false);

            EventOutcome beforeEnd = Advance(10f - Step, false, false);
            EventOutcome atEnd = Advance(Step, false, false);

            Assert.That(beforeEnd.Status, Is.EqualTo(EventStatus.Running));
            AssertFailed(atEnd, GameOverReason.VisitorNotHidden);
        }

        [Test]
        public void HiddenWhenRingsEnd_StartsHidingImmediately()
        {
            EventOutcome afterRings = Advance(4.5f, true, false);
            VisitorPhase phase = rule.Phase;
            EventOutcome beforeEnd = Advance(8f - Step, true, false);
            EventOutcome atEnd = Advance(Step, true, false);

            Assert.That(afterRings.Status, Is.EqualTo(EventStatus.Running));
            Assert.That(phase, Is.EqualTo(VisitorPhase.Hiding));
            Assert.That(beforeEnd.Status, Is.EqualTo(EventStatus.Running));
            Assert.That(atEnd.Status, Is.EqualTo(EventStatus.Succeeded));
        }

        [Test]
        public void NotHiddenDuringWindow_NoFailureBeforeTenSeconds()
        {
            Advance(4.5f, false, false);

            EventOutcome outcome = Advance(10f - Step, false, false);

            Assert.That(outcome.Status, Is.EqualTo(EventStatus.Running));
            Assert.That(rule.Phase, Is.EqualTo(VisitorPhase.Waiting));
        }

        [Test]
        public void HidesOnLastTickOfWindow_Counts()
        {
            Advance(4.5f, false, false);
            Advance(10f - Step, false, false);

            EventOutcome outcome = rule.Tick(Step, true, false);

            Assert.That(outcome.Status, Is.EqualTo(EventStatus.Running));
            Assert.That(rule.Phase, Is.EqualTo(VisitorPhase.Hiding));
        }

        [Test]
        public void LeavesWhileHiding_FailsLeftEarly()
        {
            Advance(4.5f, false, false);
            Advance(3f, true, false);

            EventOutcome outcome = rule.Tick(Step, false, false);

            AssertFailed(outcome, GameOverReason.VisitorLeftEarly);
        }

        [Test]
        public void HandleHeldDuringRings_Fails()
        {
            Advance(1f, false, false);

            EventOutcome outcome = rule.Tick(Step, false, true);

            AssertFailed(outcome, GameOverReason.VisitorTouchedEntranceHandle);
        }

        [Test]
        public void HandleHeldDuringWindow_Fails()
        {
            Advance(4.5f, false, false);
            Advance(2f, false, false);

            EventOutcome outcome = rule.Tick(Step, false, true);

            AssertFailed(outcome, GameOverReason.VisitorTouchedEntranceHandle);
        }

        [Test]
        public void HandleHeldWhileHiding_Fails()
        {
            Advance(4.5f, false, false);
            Advance(2f, true, false);

            EventOutcome outcome = rule.Tick(Step, true, true);

            AssertFailed(outcome, GameOverReason.VisitorTouchedEntranceHandle);
        }

        [Test]
        public void HandleHeldAtStart_FailsOnFirstTick()
        {
            EventOutcome outcome = rule.Tick(0f, false, true);

            AssertFailed(outcome, GameOverReason.VisitorTouchedEntranceHandle);
        }

        [Test]
        public void LeftEarlyAndHandleSameTick_ReportsLeftEarly()
        {
            Advance(4.5f, false, false);
            Advance(2f, true, false);

            EventOutcome outcome = rule.Tick(Step, false, true);

            AssertFailed(outcome, GameOverReason.VisitorLeftEarly);
        }

        [Test]
        public void AfterOutcome_TickRepeatsIt()
        {
            Advance(4.5f, true, false);
            Advance(8f, true, false);

            EventOutcome outcome = rule.Tick(Step, false, true);

            Assert.That(outcome.Status, Is.EqualTo(EventStatus.Succeeded));
        }

        private EventOutcome Advance(float seconds, bool isHidden, bool isHandleHeld)
        {
            int steps = (int)System.Math.Round(seconds / Step);
            EventOutcome outcome = rule.Outcome;

            for (int i = 0; i < steps; i++)
            {
                outcome = rule.Tick(Step, isHidden, isHandleHeld);
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
