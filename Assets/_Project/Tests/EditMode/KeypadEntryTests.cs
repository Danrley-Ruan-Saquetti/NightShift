using System;
using NUnit.Framework;

namespace NightShift.Tests
{
    public class KeypadEntryTests
    {
        private const string Code = "8362";

        private KeypadEntry entry;
        private int unlockedCount;
        private int rejectedCount;

        [SetUp]
        public void SetUp()
        {
            entry = new KeypadEntry(Code);
            unlockedCount = 0;
            rejectedCount = 0;
            entry.Unlocked += () => unlockedCount++;
            entry.Rejected += () => rejectedCount++;
        }

        [Test]
        public void PressDigit_Accumulates()
        {
            PressAll("836");

            Assert.That(entry.Digits, Is.EqualTo("836"));
        }

        [Test]
        public void PressDigit_FifthDigit_Ignored()
        {
            PressAll("83621");

            Assert.That(entry.Digits, Is.EqualTo("8362"));
        }

        [Test]
        public void PressDigit_OutOfRange_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => entry.PressDigit(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => entry.PressDigit(10));
        }

        [Test]
        public void Erase_RemovesLast()
        {
            PressAll("83");

            entry.Erase();

            Assert.That(entry.Digits, Is.EqualTo("8"));
        }

        [Test]
        public void Erase_WhenEmpty_DoesNothing()
        {
            entry.Erase();
            entry.Erase();

            Assert.That(entry.Digits, Is.EqualTo(""));
        }

        [Test]
        public void Confirm_Correct_UnlocksOnce()
        {
            PressAll(Code);

            entry.Confirm();

            Assert.That(entry.IsUnlocked, Is.True);
            Assert.That(unlockedCount, Is.EqualTo(1));
            Assert.That(rejectedCount, Is.EqualTo(0));

            entry.Confirm();

            Assert.That(unlockedCount, Is.EqualTo(1));
        }

        [Test]
        public void AfterUnlock_KeysIgnored()
        {
            PressAll(Code);
            entry.Confirm();

            entry.PressDigit(1);
            entry.Erase();

            Assert.That(entry.Digits, Is.EqualTo(Code));
        }

        [Test]
        public void Confirm_Wrong_RejectsAndClears()
        {
            PressAll("1234");

            entry.Confirm();

            Assert.That(rejectedCount, Is.EqualTo(1));
            Assert.That(entry.Digits, Is.EqualTo(""));
            Assert.That(entry.IsUnlocked, Is.False);
        }

        [Test]
        public void Confirm_Incomplete_Rejects()
        {
            PressAll("836");

            entry.Confirm();

            Assert.That(rejectedCount, Is.EqualTo(1));
            Assert.That(entry.Digits, Is.EqualTo(""));
        }

        [Test]
        public void Confirm_Empty_Rejects()
        {
            entry.Confirm();

            Assert.That(rejectedCount, Is.EqualTo(1));
        }

        [Test]
        public void Confirm_AfterWrongThenCorrect_Unlocks()
        {
            PressAll("1111");
            entry.Confirm();
            PressAll(Code);

            entry.Confirm();

            Assert.That(entry.IsUnlocked, Is.True);
        }

        [Test]
        public void Reset_ReturnsToInitialState()
        {
            PressAll(Code);
            entry.Confirm();

            entry.Reset();

            Assert.That(entry.IsUnlocked, Is.False);
            Assert.That(entry.Digits, Is.EqualTo(""));

            PressAll(Code);
            entry.Confirm();

            Assert.That(unlockedCount, Is.EqualTo(2));
        }

        [Test]
        public void Constructor_InvalidCode_Throws()
        {
            Assert.Throws<ArgumentException>(() => new KeypadEntry(null));
            Assert.Throws<ArgumentException>(() => new KeypadEntry(""));
            Assert.Throws<ArgumentException>(() => new KeypadEntry("83a2"));
        }

        private void PressAll(string digits)
        {
            foreach (char digit in digits)
            {
                entry.PressDigit(digit - '0');
            }
        }
    }
}
