using System;
using System.Text;
using NUnit.Framework;

namespace NightShift.Tests
{
    public class AudioBakingTests
    {
        private const float Tolerance = 1e-5f;

        private static float[] Ramp(int length)
        {
            float[] samples = new float[length];

            for (int i = 0; i < length; i++)
            {
                samples[i] = i;
            }

            return samples;
        }

        private static void AssertSamples(float[] actual, params float[] expected)
        {
            Assert.That(actual, Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        public void ToMono_Stereo_AveragesChannels()
        {
            float[] stereo = { 1f, 0f, 0.5f, -0.5f };

            float[] mono = AudioBaking.ToMono(stereo, 2);

            AssertSamples(mono, 0.5f, 0f);
        }

        [Test]
        public void ToMono_Mono_ReturnsCopy()
        {
            float[] source = { 0.1f, 0.2f };

            float[] mono = AudioBaking.ToMono(source, 1);

            AssertSamples(mono, 0.1f, 0.2f);
            Assert.That(mono, Is.Not.SameAs(source));
        }

        [Test]
        public void Slice_CopiesRange()
        {
            float[] source = Ramp(5);

            float[] slice = AudioBaking.Slice(source, 1, 3);

            AssertSamples(slice, 1f, 2f, 3f);
        }

        [Test]
        public void Slice_PastEnd_Throws()
        {
            float[] source = Ramp(5);

            TestDelegate slice = () => AudioBaking.Slice(source, 3, 3);

            Assert.Throws<ArgumentOutOfRangeException>(slice);
        }

        [Test]
        public void Sequence_PlacesEachClipAtSpacing()
        {
            float[][] clips = { new[] { 1f, 1f }, new[] { 2f } };

            float[] sequence = AudioBaking.Sequence(clips, 3);

            AssertSamples(sequence, 1f, 1f, 0f, 2f, 0f, 0f);
        }

        [Test]
        public void Sequence_LongClip_IsTruncated()
        {
            float[][] clips = { new[] { 1f, 2f, 3f, 4f } };

            float[] sequence = AudioBaking.Sequence(clips, 2);

            AssertSamples(sequence, 1f, 2f);
        }

        [Test]
        public void LoopCut_AfterCrossfade_CopiesSource()
        {
            float[] source = Ramp(20);

            float[] loop = AudioBaking.LoopCut(source, 2, 10, 4);

            Assert.That(loop.Length, Is.EqualTo(10));
            AssertSamples(AudioBaking.Slice(loop, 4, 6), 6f, 7f, 8f, 9f, 10f, 11f);
        }

        [Test]
        public void LoopCut_Crossfade_BlendsTailIntoHead()
        {
            float[] source = Ramp(20);

            float[] loop = AudioBaking.LoopCut(source, 2, 10, 4);

            Assert.That(loop[0], Is.EqualTo(12f).Within(Tolerance));
            Assert.That(loop[2], Is.EqualTo(4f * 0.5f + 14f * 0.5f).Within(Tolerance));
        }

        [Test]
        public void LoopCut_PastEnd_Throws()
        {
            float[] source = Ramp(10);

            TestDelegate cut = () => AudioBaking.LoopCut(source, 0, 8, 4);

            Assert.Throws<ArgumentOutOfRangeException>(cut);
        }

        [Test]
        public void Mix_SumsWithGains_PadsShorter()
        {
            float[] first = { 1f, 1f, 1f };
            float[] second = { 1f };

            float[] mix = AudioBaking.Mix(first, 0.5f, second, 0.25f);

            AssertSamples(mix, 0.75f, 0.5f, 0.5f);
        }

        [Test]
        public void FadeOut_EndsAtZero_KeepsHead()
        {
            float[] samples = { 1f, 1f, 1f, 1f, 1f };

            AudioBaking.FadeOut(samples, 2);

            AssertSamples(samples, 1f, 1f, 1f, 0.5f, 0f);
        }

        [Test]
        public void NormalizePeak_ScalesToTarget()
        {
            float[] samples = { 0.2f, -0.4f };

            AudioBaking.NormalizePeak(samples, 0.8f);

            AssertSamples(samples, 0.4f, -0.8f);
        }

        [Test]
        public void NormalizePeak_Silence_StaysSilent()
        {
            float[] samples = { 0f, 0f };

            AudioBaking.NormalizePeak(samples, 0.8f);

            AssertSamples(samples, 0f, 0f);
        }

        [Test]
        public void EncodeWav16_HeaderAndClamp()
        {
            float[] samples = { 0f, 2f, -2f };

            byte[] bytes = AudioBaking.EncodeWav16(samples, 48000);

            Assert.That(bytes.Length, Is.EqualTo(50));
            Assert.That(Encoding.ASCII.GetString(bytes, 0, 4), Is.EqualTo("RIFF"));
            Assert.That(Encoding.ASCII.GetString(bytes, 8, 4), Is.EqualTo("WAVE"));
            Assert.That(Encoding.ASCII.GetString(bytes, 36, 4), Is.EqualTo("data"));
            Assert.That(BitConverter.ToInt32(bytes, 24), Is.EqualTo(48000));
            Assert.That(BitConverter.ToInt32(bytes, 40), Is.EqualTo(6));
            Assert.That(BitConverter.ToInt16(bytes, 44), Is.EqualTo(0));
            Assert.That(BitConverter.ToInt16(bytes, 46), Is.EqualTo(32767));
            Assert.That(BitConverter.ToInt16(bytes, 48), Is.EqualTo(-32767));
        }
    }
}
