namespace PS.Core.Combat
{
    public interface IDeathSyncTarget : ICombatant
    {
        void ApplySyncedDeath();
    }
}
