using LuckyBlocks.Data;

namespace LuckyBlocks.Features.Buffs;

internal interface IFinishableBuff : IBuff
{
    IFinishCondition<IFinishableBuff> WhenFinish { get; }
    void ExternalFinish();
}