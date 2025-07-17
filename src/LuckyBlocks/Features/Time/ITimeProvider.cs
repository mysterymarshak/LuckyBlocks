using System.Diagnostics;

namespace LuckyBlocks.Features.Time;

internal interface ITimeProvider
{
    Stopwatch Stopwatch { get; }
    float ElapsedGameTime { get; }
    bool IsTimeStopped { get; }
    float ElapsedFromPreviousUpdate { get; }
    float TimeModifier { get; }
    float GameSlowMoModifier { get; }
    void Initialize();
}