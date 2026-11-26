using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NightShift
{
    public static class EventAudioBaker
    {
        private const string MenuPath = "NightShift/Tools/Bake Event Audio";
        private const string OutputFolder = "Assets/_Project/Audio/";
        private const string NormalizeProperty = "m_Normalize";

        private const string DoorbellPath = "Assets/ThirdParty/BigSoundBank/Doorbell.wav";
        private const float DoorbellSeconds = 2.5f;
        private const float DoorbellFadeSeconds = 0.2f;
        private const float DoorbellPeak = 0.95f;

        private const string FootstepPathFormat = "Assets/ThirdParty/FootstepsEssentials/Footsteps_Wood/Footsteps_Wood_Walk/Footsteps_Wood_Walk_{0:00}.wav";
        private const int FootstepCount = 10;
        private const float FootstepSpacingSeconds = 0.55f;
        private const float FootstepPeak = 0.5f;

        private const string WhisperPath = "Assets/ThirdParty/Freesound/WhispersVoices.wav";
        private const float WhisperStartSeconds = 27.5f;
        private const float WhisperSeconds = 12f;
        private const float WhisperCrossfadeSeconds = 0.5f;
        private const float WhisperPeak = 0.95f;
        private const float WhisperQuality = 0.5f;

        private const string VisitorHitPath = "Assets/ThirdParty/HorrorSFX/hit/hit8.wav";
        private const string VisitorVoicePath = "Assets/ThirdParty/HorrorStingerSoundEffects/Growls/Low Dissonant Growling.wav";
        private const float VisitorVoiceStartSeconds = 2.75f;
        private const float VisitorHitGain = 1f;
        private const float VisitorVoiceGain = 0.6f;

        private const string ShadowVoicePath = "Assets/ThirdParty/UnnervingAmbientSounds/Close Encounter/Close Encounter 1.wav";
        private const float ShadowVoiceStartSeconds = 36.75f;

        private const float ScreamSeconds = 2f;
        private const float ScreamFadeSeconds = 0.3f;
        private const float ScreamPeak = 0.9f;

        private readonly struct Sound
        {
            public Sound(float[] samples, int sampleRate)
            {
                Samples = samples;
                SampleRate = sampleRate;
            }

            public float[] Samples { get; }

            public int SampleRate { get; }

            public int ToSamples(float seconds)
            {
                return Mathf.RoundToInt(seconds * SampleRate);
            }
        }

        [MenuItem(MenuPath)]
        private static void Bake()
        {
            try
            {
                Sound doorbell = BakeDoorbell();
                Sound footsteps = BakeFootsteps();
                Sound whisper = BakeWhisper();
                Sound visitorScream = BakeVisitorScream();
                Sound shadowScream = BakeShadowScream();

                Save("DoorbellRing", doorbell, DecompressedSettings());
                Save("FootstepsWood", footsteps, DecompressedSettings());
                Save("WhisperLoop", whisper, CompressedSettings(WhisperQuality));
                Save("VisitorScream", visitorScream, DecompressedSettings());
                Save("ShadowScream", shadowScream, DecompressedSettings());
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogError(exception.Message);
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool CanBake()
        {
            return !EditorApplication.isPlaying;
        }

        private static Sound BakeDoorbell()
        {
            Sound source = Load(DoorbellPath);

            float[] ring = AudioBaking.Slice(source.Samples, 0, source.ToSamples(DoorbellSeconds));
            AudioBaking.FadeOut(ring, source.ToSamples(DoorbellFadeSeconds));
            AudioBaking.NormalizePeak(ring, DoorbellPeak);

            return new Sound(ring, source.SampleRate);
        }

        private static Sound BakeFootsteps()
        {
            float[][] steps = new float[FootstepCount][];
            int sampleRate = 0;

            for (int i = 0; i < FootstepCount; i++)
            {
                string path = string.Format(FootstepPathFormat, i + 1);
                Sound step = Load(path);

                if (i > 0)
                {
                    RequireSampleRate(step, sampleRate, path);
                }

                sampleRate = step.SampleRate;
                steps[i] = step.Samples;
            }

            float[] walk = AudioBaking.Sequence(steps, Mathf.RoundToInt(FootstepSpacingSeconds * sampleRate));
            AudioBaking.NormalizePeak(walk, FootstepPeak);

            return new Sound(walk, sampleRate);
        }

        private static Sound BakeWhisper()
        {
            Sound source = Load(WhisperPath);

            float[] loop = AudioBaking.LoopCut(
                source.Samples,
                source.ToSamples(WhisperStartSeconds),
                source.ToSamples(WhisperSeconds),
                source.ToSamples(WhisperCrossfadeSeconds));
            AudioBaking.NormalizePeak(loop, WhisperPeak);

            return new Sound(loop, source.SampleRate);
        }

        private static Sound BakeVisitorScream()
        {
            Sound hit = Load(VisitorHitPath);
            Sound voice = Load(VisitorVoicePath);
            RequireSampleRate(voice, hit.SampleRate, VisitorVoicePath);

            int length = hit.ToSamples(ScreamSeconds);
            float[] attack = AudioBaking.Slice(hit.Samples, 0, Math.Min(length, hit.Samples.Length));
            float[] body = AudioBaking.Slice(voice.Samples, voice.ToSamples(VisitorVoiceStartSeconds), length);
            float[] mix = AudioBaking.Mix(attack, VisitorHitGain, body, VisitorVoiceGain);

            return FinishScream(mix, hit.SampleRate);
        }

        private static Sound BakeShadowScream()
        {
            Sound voice = Load(ShadowVoicePath);
            float[] body = AudioBaking.Slice(voice.Samples, voice.ToSamples(ShadowVoiceStartSeconds), voice.ToSamples(ScreamSeconds));

            return FinishScream(body, voice.SampleRate);
        }

        private static Sound FinishScream(float[] samples, int sampleRate)
        {
            Sound scream = new Sound(samples, sampleRate);
            AudioBaking.FadeOut(samples, scream.ToSamples(ScreamFadeSeconds));
            AudioBaking.NormalizePeak(samples, ScreamPeak);

            return scream;
        }

        private static Sound Load(string path)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);

            if (clip == null)
            {
                throw new InvalidOperationException($"Audio source not found: {path}");
            }

            if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
            {
                throw new InvalidOperationException($"Audio source must use Decompress On Load: {path}");
            }

            float[] interleaved = new float[clip.samples * clip.channels];

            if (!clip.LoadAudioData() || !clip.GetData(interleaved, 0))
            {
                throw new InvalidOperationException($"Audio source could not be read: {path}");
            }

            return new Sound(AudioBaking.ToMono(interleaved, clip.channels), clip.frequency);
        }

        private static void RequireSampleRate(Sound sound, int sampleRate, string path)
        {
            if (sound.SampleRate != sampleRate)
            {
                throw new InvalidOperationException($"Audio source is {sound.SampleRate} Hz, expected {sampleRate} Hz: {path}");
            }
        }

        private static AudioImporterSampleSettings DecompressedSettings()
        {
            return new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.DecompressOnLoad,
                compressionFormat = AudioCompressionFormat.PCM,
                quality = 1f,
                preloadAudioData = true
            };
        }

        private static AudioImporterSampleSettings CompressedSettings(float quality)
        {
            return new AudioImporterSampleSettings
            {
                loadType = AudioClipLoadType.CompressedInMemory,
                compressionFormat = AudioCompressionFormat.Vorbis,
                quality = quality,
                preloadAudioData = true
            };
        }

        private static void Save(string name, Sound sound, AudioImporterSampleSettings settings)
        {
            string path = $"{OutputFolder}{name}.wav";
            File.WriteAllBytes(path, AudioBaking.EncodeWav16(sound.Samples, sound.SampleRate));
            AssetDatabase.ImportAsset(path);

            AudioImporter importer = (AudioImporter)AssetImporter.GetAtPath(path);
            SerializedObject serialized = new SerializedObject(importer);
            serialized.FindProperty(NormalizeProperty).boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            importer.forceToMono = false;
            importer.loadInBackground = false;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();
        }
    }
}
