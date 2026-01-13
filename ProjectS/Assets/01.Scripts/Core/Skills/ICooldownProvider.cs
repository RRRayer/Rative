namespace ProjectS.Core.Skills
{
    public interface ICooldownProvider
    {
        float GetCooldownRemaining(SkillSlot slot);
        float GetCooldownDuration(SkillSlot slot);
    }
}
