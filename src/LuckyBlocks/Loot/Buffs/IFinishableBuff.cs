using LuckyBlocks.Data;

namespace LuckyBlocks.Loot.Buffs;

internal interface IFinishableBuff : IBuff
{
    IFinishCondition<IFinishableBuff> WhenFinish { get; }
    void ExternalFinish();
}