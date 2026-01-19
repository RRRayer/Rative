namespace ProjectS.Core.Combat
{
    public interface IDeathSyncTarget : ICombatant
    {
        void ApplySyncedDeath();
    }
}
