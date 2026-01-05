using System;

namespace ProjectS.Core.Combat
{
    public static class DamageEvents
    {
        public static event Action<DamageInfo> DamageApplied;

        public static void Raise(DamageInfo info)
        {
            DamageApplied?.Invoke(info);
        }
    }
}
