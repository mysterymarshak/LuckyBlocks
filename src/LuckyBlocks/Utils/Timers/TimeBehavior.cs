using System;

namespace LuckyBlocks.Utils.Timers;

[Flags]
internal enum TimeBehavior
{
    None = 0,
    
    RealTime = 1,
    
    TimeModifier = 2,
    
    IgnoreTimeStop = 4,
    
    TicksInTimeStopDoesntAffectToIterationsCount = 8
}