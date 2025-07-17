using LuckyBlocks.Features.Magic;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal interface IWizard : IFinishableBuff, IStackableBuff, ICloneableBuff<IWizard>
{
    int CastsCount { get; }
    int CastsLeft { get; }
    void BindMagic(IMagic magic);
}