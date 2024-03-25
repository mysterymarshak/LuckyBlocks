namespace LuckyBlocks.Loot.Buffs.Wizards;

internal interface IWizard : IFinishableBuff, IStackableBuff, ICloneableBuff<IWizard>
{
    int CastsCount { get; }
    int CastsLeft { get; }
}