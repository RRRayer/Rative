namespace ProjectS.Core.Skills
{
    public interface ISkillExecutor
    {
        bool TryExecuteSkill(SkillSlot slot);
        float GetCooldownRemaining(SkillSlot slot);
    }
}
