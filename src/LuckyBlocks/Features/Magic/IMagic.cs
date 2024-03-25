using LuckyBlocks.Data;

namespace LuckyBlocks.Features.Magic;

internal interface IMagic
{
    IFinishCondition<IMagic> WhenFinish { get; }
    string Name { get; }
    void ExternalFinish();
}