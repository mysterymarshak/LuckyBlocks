using LuckyBlocks.Data;
using LuckyBlocks.Features.Identity;

namespace LuckyBlocks.Features.Magic;

internal interface IMagic
{
    bool IsCloned { get; }
    bool IsFinished { get; }
    bool ShouldCastOnRestore { get; }
    Player Wizard { get; }
    string Name { get; }
    IFinishCondition<IMagic> WhenFinish { get; }
    IMagic Clone();
    void OnRestored();
    void Cast();
    void ExternalFinish();
}