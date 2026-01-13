namespace ProjectS.Core.Skills
{
    public interface IUpgradeProvider
    {
        System.Collections.Generic.List<UpgradeOption> BuildUpgradeOptions(int count);
        void ApplyUpgradeChoice(UpgradeOption option);
    }
}
