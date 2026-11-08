using System;

namespace NightShift
{
    public class RoomLightState
    {
        public bool IsOn { get; private set; } = true;
        public bool IsFlickering { get; private set; }

        private float flickerRemaining;

        public event Action<bool> Changed;

        public void SetOn(bool on)
        {
            if (on == IsOn)
            {
                return;
            }

            IsOn = on;

            if (!on)
            {
                StopFlicker();
            }

            Changed?.Invoke(on);
        }

        public void Toggle()
        {
            SetOn(!IsOn);
        }

        public void StartFlicker(float duration)
        {
            if (!IsOn || duration <= 0f)
            {
                return;
            }

            IsFlickering = true;
            flickerRemaining = duration;
        }

        public void StopFlicker()
        {
            IsFlickering = false;
            flickerRemaining = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (!IsFlickering)
            {
                return;
            }

            flickerRemaining -= deltaTime;

            if (flickerRemaining <= 0f)
            {
                StopFlicker();
            }
        }

        public void Reset()
        {
            SetOn(true);
            StopFlicker();
        }
    }
}
