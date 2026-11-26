using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace NightShift
{
    public static class AudioBaking
    {
        private const int WavHeaderBytes = 44;
        private const int FormatChunkBytes = 16;
        private const short PcmFormat = 1;
        private const short MonoChannels = 1;
        private const short BitsPerSample = 16;
        private const int BytesPerSample = BitsPerSample / 8;

        public static float[] ToMono(float[] interleaved, int channels)
        {
            float[] mono = new float[interleaved.Length / channels];

            for (int frame = 0; frame < mono.Length; frame++)
            {
                float sum = 0f;

                for (int channel = 0; channel < channels; channel++)
                {
                    sum += interleaved[frame * channels + channel];
                }

                mono[frame] = sum / channels;
            }

            return mono;
        }

        public static float[] Slice(float[] source, int start, int length)
        {
            if (start < 0 || length < 0 || start + length > source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Slice {start}+{length} exceeds {source.Length} samples");
            }

            float[] slice = new float[length];
            Array.Copy(source, start, slice, 0, length);

            return slice;
        }

        public static float[] Sequence(IReadOnlyList<float[]> clips, int spacing)
        {
            float[] sequence = new float[clips.Count * spacing];

            for (int i = 0; i < clips.Count; i++)
            {
                Array.Copy(clips[i], 0, sequence, i * spacing, Math.Min(clips[i].Length, spacing));
            }

            return sequence;
        }

        public static float[] LoopCut(float[] source, int start, int length, int crossfade)
        {
            if (start < 0 || crossfade < 0 || crossfade > length || start + length + crossfade > source.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(length), $"Loop {start}+{length}+{crossfade} exceeds {source.Length} samples");
            }

            float[] loop = Slice(source, start, length);

            for (int i = 0; i < crossfade; i++)
            {
                float headWeight = (float)i / crossfade;
                loop[i] = loop[i] * headWeight + source[start + length + i] * (1f - headWeight);
            }

            return loop;
        }

        public static float[] Mix(float[] first, float firstGain, float[] second, float secondGain)
        {
            float[] mix = new float[Math.Max(first.Length, second.Length)];

            for (int i = 0; i < first.Length; i++)
            {
                mix[i] = first[i] * firstGain;
            }

            for (int i = 0; i < second.Length; i++)
            {
                mix[i] += second[i] * secondGain;
            }

            return mix;
        }

        public static void FadeOut(float[] samples, int length)
        {
            int faded = Math.Min(length, samples.Length);

            for (int fromEnd = 0; fromEnd < faded; fromEnd++)
            {
                samples[samples.Length - 1 - fromEnd] *= (float)fromEnd / faded;
            }
        }

        public static void NormalizePeak(float[] samples, float peak)
        {
            float loudest = 0f;

            foreach (float sample in samples)
            {
                loudest = Mathf.Max(loudest, Mathf.Abs(sample));
            }

            if (loudest == 0f)
            {
                return;
            }

            float gain = peak / loudest;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= gain;
            }
        }

        public static byte[] EncodeWav16(float[] samples, int sampleRate)
        {
            int dataBytes = samples.Length * BytesPerSample;

            using MemoryStream stream = new MemoryStream(WavHeaderBytes + dataBytes);
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(WavHeaderBytes - 8 + dataBytes);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));

            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(FormatChunkBytes);
            writer.Write(PcmFormat);
            writer.Write(MonoChannels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * BytesPerSample);
            writer.Write((short)BytesPerSample);
            writer.Write(BitsPerSample);

            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataBytes);

            foreach (float sample in samples)
            {
                writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
            }

            writer.Flush();

            return stream.ToArray();
        }
    }
}
