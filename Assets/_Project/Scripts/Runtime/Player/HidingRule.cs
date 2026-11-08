using System;
using System.Collections.Generic;
using UnityEngine;

namespace NightShift
{
    public static class HidingRule
    {
        public static int FindHidingIndex(Vector3 head, IReadOnlyList<Transform> volumes, IReadOnlyList<bool> doorsClosed)
        {
            if (volumes.Count != doorsClosed.Count)
            {
                throw new ArgumentException("Each hiding volume needs exactly one door state");
            }

            for (int i = 0; i < volumes.Count; i++)
            {
                if (doorsClosed[i] && UnitBox.Contains(volumes[i], head))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
