using System.Collections.Generic;
using NUnit.Framework;

namespace NightShift.Tests
{
    public class EventScheduleTests
    {
        private EventSchedule schedule;

        [SetUp]
        public void SetUp()
        {
            schedule = new EventSchedule(EventSchedule.Default, 40f);
        }

        [Test]
        public void Default_IsScriptInOrder()
        {
            IReadOnlyList<ScheduledEvent> entries = EventSchedule.Default;

            Assert.That(entries.Count, Is.EqualTo(4));
            AssertEntry(entries[0], 2, EventKind.Visitor);
            AssertEntry(entries[1], 5, EventKind.Shadow);
            AssertEntry(entries[2], 8, EventKind.Visitor);
            AssertEntry(entries[3], 11, EventKind.Shadow);
        }

        [Test]
        public void NoTasks_NothingStarts()
        {
            Assert.That(schedule.TryTakeNext(0f, false, true, out _), Is.False);
        }

        [Test]
        public void SecondTask_StartsVisitor_WithoutGap()
        {
            schedule.SetCompletedCount(2);

            bool taken = schedule.TryTakeNext(0f, false, true, out EventKind kind);

            Assert.That(taken, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Visitor));
            Assert.That(schedule.IsRunning, Is.True);
        }

        [Test]
        public void CountAboveThreshold_StillQueues()
        {
            List<EventKind> queued = Record();

            schedule.SetCompletedCount(3);
            bool taken = schedule.TryTakeNext(0f, false, true, out EventKind kind);

            Assert.That(queued, Is.EqualTo(new[] { EventKind.Visitor }));
            Assert.That(taken, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Visitor));
        }

        [Test]
        public void SetCompletedCount_JumpToEleven_QueuesWholeScriptInOrder()
        {
            List<EventKind> queued = Record();

            schedule.SetCompletedCount(11);
            schedule.SetCompletedCount(11);

            Assert.That(queued, Is.EqualTo(new[]
            {
                EventKind.Visitor,
                EventKind.Shadow,
                EventKind.Visitor,
                EventKind.Shadow
            }));
        }

        [Test]
        public void WhileRunning_NothingStarts()
        {
            schedule.SetCompletedCount(2);
            schedule.TryTakeNext(0f, false, true, out _);

            schedule.SetCompletedCount(5);

            Assert.That(schedule.TryTakeNext(100f, false, true, out _), Is.False);
        }

        [Test]
        public void NextEvent_WaitsGapFromEnd()
        {
            schedule.SetCompletedCount(2);
            schedule.TryTakeNext(0f, false, true, out _);
            schedule.End(20f);
            schedule.SetCompletedCount(5);

            bool early = schedule.TryTakeNext(59.9f, false, true, out _);
            bool onTime = schedule.TryTakeNext(60f, false, true, out EventKind kind);

            Assert.That(early, Is.False);
            Assert.That(onTime, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Shadow));
        }

        [Test]
        public void Teleporting_Blocks_ThenStartsWhenItEnds()
        {
            schedule.SetCompletedCount(2);

            bool during = schedule.TryTakeNext(0f, true, true, out _);
            bool after = schedule.TryTakeNext(0.3f, false, true, out _);

            Assert.That(during, Is.False);
            Assert.That(after, Is.True);
        }

        [Test]
        public void SeveralPending_StartOneAtATime_WithGap()
        {
            schedule.SetCompletedCount(8);

            schedule.TryTakeNext(0f, false, true, out EventKind first);
            schedule.End(10f);
            schedule.TryTakeNext(50f, false, true, out EventKind second);
            schedule.End(60f);
            bool early = schedule.TryTakeNext(99.9f, false, true, out _);
            schedule.TryTakeNext(100f, false, true, out EventKind third);

            Assert.That(first, Is.EqualTo(EventKind.Visitor));
            Assert.That(second, Is.EqualTo(EventKind.Shadow));
            Assert.That(early, Is.False);
            Assert.That(third, Is.EqualTo(EventKind.Visitor));
        }

        [Test]
        public void ShadowAtHead_BlockedByPorch_HoldsQueue()
        {
            schedule.SetCompletedCount(8);
            schedule.TryTakeNext(0f, false, true, out _);
            schedule.End(0f);

            bool blocked = schedule.TryTakeNext(40f, false, false, out _);
            bool released = schedule.TryTakeNext(40f, false, true, out EventKind kind);

            Assert.That(blocked, Is.False);
            Assert.That(released, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Shadow));
        }

        [Test]
        public void Mirror_AllFree_AcceptedAndConsumesFirstShadow()
        {
            bool accepted = schedule.TryTakeMirror(0f, false);
            schedule.End(10f);
            schedule.SetCompletedCount(5);

            schedule.TryTakeNext(50f, false, true, out EventKind kind);
            schedule.End(60f);
            bool shadowAgain = schedule.TryTakeNext(200f, false, true, out _);

            Assert.That(accepted, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Visitor));
            Assert.That(shadowAgain, Is.False);
        }

        [Test]
        public void Mirror_Blocked_DoesNotConsume()
        {
            schedule.SetCompletedCount(2);
            schedule.TryTakeNext(0f, false, true, out _);

            bool whileRunning = schedule.TryTakeMirror(10f, false);
            schedule.End(0f);
            bool duringGap = schedule.TryTakeMirror(39.9f, false);
            bool duringTeleport = schedule.TryTakeMirror(100f, true);
            schedule.SetCompletedCount(5);
            bool shadowStillScheduled = schedule.TryTakeNext(100f, false, true, out EventKind kind);

            Assert.That(whileRunning, Is.False);
            Assert.That(duringGap, Is.False);
            Assert.That(duringTeleport, Is.False);
            Assert.That(shadowStillScheduled, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Shadow));
        }

        [Test]
        public void Mirror_ConsumesPendingShadow()
        {
            schedule.SetCompletedCount(5);
            schedule.TryTakeNext(0f, false, true, out _);
            schedule.End(0f);
            bool blockedByPorch = schedule.TryTakeNext(40f, false, false, out _);

            bool accepted = schedule.TryTakeMirror(40f, false);
            schedule.End(50f);
            bool shadowAgain = schedule.TryTakeNext(100f, false, true, out _);

            Assert.That(blockedByPorch, Is.False);
            Assert.That(accepted, Is.True);
            Assert.That(shadowAgain, Is.False);
        }

        [Test]
        public void Mirror_AfterBothShadowsUsed_StillAccepted()
        {
            bool first = schedule.TryTakeMirror(0f, false);
            schedule.End(0f);
            bool second = schedule.TryTakeMirror(40f, false);
            schedule.End(40f);
            bool third = schedule.TryTakeMirror(80f, false);
            schedule.End(80f);
            schedule.SetCompletedCount(11);

            List<EventKind> started = new();
            float now = 120f;

            while (schedule.TryTakeNext(now, false, true, out EventKind kind))
            {
                started.Add(kind);
                schedule.End(now);
                now += 40f;
            }

            Assert.That(first && second && third, Is.True);
            Assert.That(started, Is.EqualTo(new[] { EventKind.Visitor, EventKind.Visitor }));
        }

        [Test]
        public void BeginUnscheduled_DoesNotConsumeScript()
        {
            schedule.BeginUnscheduled();
            schedule.End(0f);
            schedule.SetCompletedCount(2);

            bool taken = schedule.TryTakeNext(40f, false, true, out EventKind kind);

            Assert.That(taken, Is.True);
            Assert.That(kind, Is.EqualTo(EventKind.Visitor));
        }

        private List<EventKind> Record()
        {
            List<EventKind> queued = new();
            schedule.Queued += queued.Add;

            return queued;
        }

        private static void AssertEntry(ScheduledEvent entry, int taskCount, EventKind kind)
        {
            Assert.That(entry.TaskCount, Is.EqualTo(taskCount));
            Assert.That(entry.Kind, Is.EqualTo(kind));
        }
    }
}
