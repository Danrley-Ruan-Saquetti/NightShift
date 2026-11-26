using UnityEngine;

namespace NightShift
{
    public sealed class HeadOcclusion
    {
        public HeadOcclusionState State { get; private set; }
        public Vector3 LastValidPosition { get; private set; }
        public bool IsDark => State != HeadOcclusionState.Clear;

        public HeadOcclusion(Vector3 startPosition)
        {
            Reset(startPosition);
        }

        public void Evaluate(Vector3 headPosition, bool isOverlapping, bool hasLineToLastValid)
        {
            if (isOverlapping)
            {
                State = HeadOcclusionState.Inside;
                return;
            }

            if (!hasLineToLastValid)
            {
                State = HeadOcclusionState.Blind;
                return;
            }

            Reset(headPosition);
        }

        public void Reset(Vector3 headPosition)
        {
            State = HeadOcclusionState.Clear;
            LastValidPosition = headPosition;
        }
    }
}
