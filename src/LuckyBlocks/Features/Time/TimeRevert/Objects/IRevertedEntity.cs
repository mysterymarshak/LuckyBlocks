using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal interface IRevertedEntity
{
    void Restore(IGame game);
}