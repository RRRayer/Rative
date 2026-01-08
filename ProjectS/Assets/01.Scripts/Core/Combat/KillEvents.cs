using System;

namespace ProjectS.Core.Combat
{
    public static class KillEvents
    {
        public static event Action<KillInfo> Killed;

        public static void Raise(KillInfo info)
        {
            Killed?.Invoke(info);
        }
    }
}
