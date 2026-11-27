using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace NightShift
{
    public class FoveationLevel : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float level = 0.5f;

        private void Start()
        {
            List<XRDisplaySubsystem> displays = new();
            SubsystemManager.GetSubsystems(displays);

            foreach (XRDisplaySubsystem display in displays)
            {
                if (!display.running)
                {
                    continue;
                }

                display.foveatedRenderingLevel = level;
                display.foveatedRenderingFlags = XRDisplaySubsystem.FoveatedRenderingFlags.None;
            }
        }
    }
}
