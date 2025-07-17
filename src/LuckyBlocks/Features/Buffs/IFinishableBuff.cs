using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs;

internal interface IFinishableBuff : IBuff
{
    Color BuffColor { get; }
    Color ChatColor { get; }
    IFinishCondition<IFinishableBuff> WhenFinish { get; }
    void ExternalFinish();
}