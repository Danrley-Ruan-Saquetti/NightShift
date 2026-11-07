using System;
using UnityEngine;

namespace NightShift
{
    public class StainCleaning
    {
        private readonly float requiredScrubTime;
        private readonly bool requiresSpray;

        private float scrubTime;

        public StainCleaning(float requiredScrubTime, bool requiresSpray)
        {
            this.requiredScrubTime = requiredScrubTime;
            this.requiresSpray = requiresSpray;
        }

        public bool IsSprayed { get; private set; }
        public bool IsCleaned { get; private set; }
        public float Progress => scrubTime / requiredScrubTime;

        public event Action Cleaned;

        public bool CanBeScrubbed()
        {
            return !requiresSpray || IsSprayed;
        }

        public void Spray()
        {
            IsSprayed = true;
        }

        public void Scrub(float deltaTime)
        {
            if (IsCleaned || !CanBeScrubbed())
            {
                return;
            }

            scrubTime = Mathf.Min(scrubTime + deltaTime, requiredScrubTime);

            if (scrubTime < requiredScrubTime)
            {
                return;
            }

            IsCleaned = true;
            Cleaned?.Invoke();
        }
    }
}
