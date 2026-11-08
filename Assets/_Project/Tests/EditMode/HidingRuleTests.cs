using System;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NightShift.Tests
{
    public class HidingRuleTests
    {
        private static readonly Vector3 VolumeScale = new(0.92f, 2.1f, 0.72f);

        private Transform volumeA;
        private Transform volumeB;

        [SetUp]
        public void SetUp()
        {
            volumeA = CreateVolume("A", new Vector3(0f, 1.05f, 0f));
            volumeB = CreateVolume("B", new Vector3(5f, 1.05f, 0f));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(volumeA.gameObject);
            Object.DestroyImmediate(volumeB.gameObject);
        }

        [Test]
        public void InsideAndClosed_ReturnsIndex()
        {
            int index = HidingRule.FindHidingIndex(new Vector3(0f, 1.6f, 0f), new[] { volumeA }, new[] { true });

            Assert.That(index, Is.EqualTo(0));
        }

        [Test]
        public void InsideAndOpen_ReturnsMinusOne()
        {
            int index = HidingRule.FindHidingIndex(new Vector3(0f, 1.6f, 0f), new[] { volumeA }, new[] { false });

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void OutsideAndClosed_ReturnsMinusOne()
        {
            int index = HidingRule.FindHidingIndex(new Vector3(0f, 1.6f, 1f), new[] { volumeA }, new[] { true });

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void SeveralClosets_OnlyContainingOneCounts()
        {
            Vector3 head = new(5f, 1.6f, 0f);
            Transform[] volumes = { volumeA, volumeB };

            Assert.That(HidingRule.FindHidingIndex(head, volumes, new[] { true, false }), Is.EqualTo(-1));
            Assert.That(HidingRule.FindHidingIndex(head, volumes, new[] { false, true }), Is.EqualTo(1));
        }

        [Test]
        public void NoClosets_ReturnsMinusOne()
        {
            int index = HidingRule.FindHidingIndex(Vector3.zero, new Transform[0], new bool[0]);

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void MismatchedLists_Throws()
        {
            Assert.Throws<ArgumentException>(
                () => HidingRule.FindHidingIndex(Vector3.zero, new[] { volumeA, volumeB }, new[] { true }));
        }

        private static Transform CreateVolume(string name, Vector3 position)
        {
            GameObject volume = new(name);
            volume.transform.position = position;
            volume.transform.localScale = VolumeScale;
            return volume.transform;
        }
    }
}
