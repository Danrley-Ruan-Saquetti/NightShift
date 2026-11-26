using System;

namespace NightShift
{
    public static class JumpscareKillers
    {
        public static Killer For(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.Debug:
                case GameOverReason.VisitorNotHidden:
                case GameOverReason.VisitorLeftEarly:
                case GameOverReason.VisitorTouchedEntranceHandle:
                    return Killer.Visitor;
                case GameOverReason.ShadowLightOn:
                case GameOverReason.ShadowWrongRoomSwitch:
                case GameOverReason.ShadowTeleported:
                case GameOverReason.ShadowHidden:
                    return Killer.Shadow;
                default:
                    throw new ArgumentOutOfRangeException(nameof(reason), reason, null);
            }
        }
    }
}
