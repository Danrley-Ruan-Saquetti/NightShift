using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NightShift
{
    public class RoomLights : MonoBehaviour
    {
        [SerializeField]
        private Room room;
        [SerializeField]
        private Light[] lights;
        [SerializeField, Range(0f, 1f)]
        private float flickerLowIntensity = 0.15f;
        [SerializeField, Range(0f, 1f)]
        private float flickerLowVariation = 0.1f;
        [SerializeField]
        private float minFlickerInterval = 0.05f;
        [SerializeField]
        private float maxFlickerInterval = 0.2f;

        public Room Room => room;
        public bool IsOn => state.IsOn;
        public bool IsFlickering => state.IsFlickering;
        public float Brightness { get; private set; } = 1f;

        private readonly RoomLightState state = new();
        private float[] originalIntensities;
        private bool isFlickerApplied;
        private bool isLow;
        private float nextSwitchIn;

        public event Action<Room, bool> StateChanged;

        private void Awake()
        {
            originalIntensities = new float[lights.Length];

            for (int i = 0; i < lights.Length; i++)
            {
                originalIntensities[i] = lights[i].intensity;
            }
        }

        private void OnEnable()
        {
            state.Changed += OnStateChanged;
        }

        private void Update()
        {
            if (!isFlickerApplied)
            {
                return;
            }

            state.Tick(Time.deltaTime);

            if (!state.IsFlickering)
            {
                RestoreLights();
                return;
            }

            nextSwitchIn -= Time.deltaTime;

            if (nextSwitchIn > 0f)
            {
                return;
            }

            SwitchFlickerLevel();
        }

        private void OnDisable()
        {
            state.Changed -= OnStateChanged;
        }

        public void SetOn(bool on)
        {
            state.SetOn(on);
        }

        public void Toggle()
        {
            state.Toggle();
        }

        public void Flicker(float duration)
        {
            state.StartFlicker(duration);

            if (!state.IsFlickering)
            {
                return;
            }

            isFlickerApplied = true;
            isLow = false;
            nextSwitchIn = 0f;
        }

        public void StopFlicker()
        {
            state.StopFlicker();
            RestoreLights();
        }

        public void ResetLights()
        {
            state.Reset();
            RestoreLights();
        }

        private void SwitchFlickerLevel()
        {
            isLow = !isLow;
            nextSwitchIn = Random.Range(minFlickerInterval, maxFlickerInterval);
            float lowLevel = flickerLowIntensity + Random.Range(-flickerLowVariation, flickerLowVariation);
            Brightness = isLow ? Mathf.Clamp01(lowLevel) : 1f;
            ApplyIntensity(Brightness);
        }

        private void RestoreLights()
        {
            isFlickerApplied = false;
            Brightness = state.IsOn ? 1f : 0f;

            foreach (Light roomLight in lights)
            {
                roomLight.enabled = state.IsOn;
            }

            ApplyIntensity(1f);
        }

        private void ApplyIntensity(float level)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                lights[i].intensity = originalIntensities[i] * level;
            }
        }

        private void OnStateChanged(bool isOn)
        {
            RestoreLights();
            StateChanged?.Invoke(room, isOn);
        }
    }
}
