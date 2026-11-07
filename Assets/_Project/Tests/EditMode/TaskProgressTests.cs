using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace NightShift.Tests
{
    public class TaskProgressTests
    {
        private TaskProgress progress;

        [SetUp]
        public void SetUp()
        {
            progress = new TaskProgress(CreateHouseTasks());
        }

        [Test]
        public void NewProgress_CountsMatchHouse()
        {
            Assert.That(progress.TotalCount, Is.EqualTo(14));
            Assert.That(progress.TotalCountOf(TaskType.Stain), Is.EqualTo(5));
            Assert.That(progress.TotalCountOf(TaskType.ToughStain), Is.EqualTo(2));
            Assert.That(progress.TotalCountOf(TaskType.Trash), Is.EqualTo(6));
            Assert.That(progress.TotalCountOf(TaskType.LockedDoor), Is.EqualTo(1));
            Assert.That(progress.CompletedCount, Is.EqualTo(0));
        }

        [Test]
        public void Complete_IncrementsTotalAndType()
        {
            progress.Complete("MD1");

            Assert.That(progress.CompletedCount, Is.EqualTo(1));
            Assert.That(progress.CompletedCountOf(TaskType.ToughStain), Is.EqualTo(1));
            Assert.That(progress.CompletedCountOf(TaskType.Stain), Is.EqualTo(0));
            Assert.That(progress.IsCompleted("MD1"), Is.True);
        }

        [Test]
        public void Complete_Twice_CountsOnceAndRaisesOnce()
        {
            int raisedCount = 0;
            progress.TaskCompleted += (_, __) => raisedCount++;

            progress.Complete("L1");
            progress.Complete("L1");

            Assert.That(progress.CompletedCount, Is.EqualTo(1));
            Assert.That(raisedCount, Is.EqualTo(1));
        }

        [Test]
        public void Complete_RaisesEventWithIdAndNewTotal()
        {
            string lastId = null;
            int lastTotal = 0;
            progress.TaskCompleted += (id, total) =>
            {
                lastId = id;
                lastTotal = total;
            };

            progress.Complete("M1");
            progress.Complete("L2");

            Assert.That(lastId, Is.EqualTo("L2"));
            Assert.That(lastTotal, Is.EqualTo(2));
        }

        [Test]
        public void Complete_UnknownId_Throws()
        {
            Assert.Throws<ArgumentException>(() => progress.Complete("M9"));
        }

        [Test]
        public void Reset_ClearsEverythingAndRaises()
        {
            int resetCount = 0;
            progress.ProgressReset += () => resetCount++;
            progress.Complete("M1");
            progress.Complete("MD2");
            progress.Complete("L3");

            progress.Reset();

            Assert.That(progress.CompletedCount, Is.EqualTo(0));

            foreach (TaskType type in Enum.GetValues(typeof(TaskType)))
            {
                Assert.That(progress.CompletedCountOf(type), Is.EqualTo(0));
            }

            Assert.That(resetCount, Is.EqualTo(1));

            progress.Complete("M1");

            Assert.That(progress.CompletedCount, Is.EqualTo(1));
        }

        private static List<TaskDefinition> CreateHouseTasks()
        {
            return new List<TaskDefinition>
            {
                new("M1", TaskType.Stain),
                new("M2", TaskType.Stain),
                new("M3", TaskType.Stain),
                new("M4", TaskType.Stain),
                new("M5", TaskType.Stain),
                new("MD1", TaskType.ToughStain),
                new("MD2", TaskType.ToughStain),
                new("L1", TaskType.Trash),
                new("L2", TaskType.Trash),
                new("L3", TaskType.Trash),
                new("L4", TaskType.Trash),
                new("L5", TaskType.Trash),
                new("L6", TaskType.Trash),
                new("P3", TaskType.LockedDoor)
            };
        }
    }
}
