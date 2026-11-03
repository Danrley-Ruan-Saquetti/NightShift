using NUnit.Framework;
using UnityEngine;

namespace NightShift.Tests
{
    public class RoomVolumeTests
    {
        private GameObject volumeObject;
        private RoomVolume volume;

        [SetUp]
        public void SetUp()
        {
            volumeObject = new GameObject("Volume");
            volumeObject.transform.position = new Vector3(2f, 1.2f, 2f);
            volumeObject.transform.localScale = new Vector3(4f, 2.4f, 4f);
            volume = volumeObject.AddComponent<RoomVolume>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(volumeObject);
        }

        [Test]
        public void Contains_PointInside_IsTrue()
        {
            Assert.That(volume.Contains(new Vector3(0.1f, 0.1f, 3.9f)), Is.True);
        }

        [Test]
        public void Contains_PointOutsideHorizontally_IsFalse()
        {
            Assert.That(volume.Contains(new Vector3(4.2f, 1.2f, 2f)), Is.False);
        }

        [Test]
        public void Contains_PointAboveCeiling_IsFalse()
        {
            Assert.That(volume.Contains(new Vector3(2f, 2.5f, 2f)), Is.False);
        }

        [Test]
        public void Contains_RespectsRotation()
        {
            volumeObject.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            volumeObject.transform.localScale = new Vector3(1f, 2.4f, 4f);

            Assert.That(volume.Contains(new Vector3(3.9f, 1.2f, 2f)), Is.True);
            Assert.That(volume.Contains(new Vector3(2f, 1.2f, 3.9f)), Is.False);
        }
    }
}
