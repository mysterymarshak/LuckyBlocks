using System.Diagnostics;

namespace LuckyBlocks.Features.Time;

internal interface ITimeProvider
{
    Stopwatch Stopwatch { get; }
    float ElapsedGameTime { get; }
    float ElapsedRealTime { get; }
    bool IsTimeStopped { get; }
    float ElapsedFromPreviousUpdate { get; }
    float TimeModifier { get; }
    float GameSlowMoModifier { get; }
    void Initialize();
}