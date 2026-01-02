using UnityEngine;

namespace ProjectS.Core.Combat
{
    public interface ICombatant
    {
        float Health { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }
        Vector3 Position { get; }

        void ApplyDamage(DamageInfo info);
    }
}
