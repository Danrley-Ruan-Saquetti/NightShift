using System;

namespace NightShift
{
    public interface IHouseEvent
    {
        bool IsRunning { get; }

        event Action Succeeded;

        void Begin();
        void Cancel();
    }
}
