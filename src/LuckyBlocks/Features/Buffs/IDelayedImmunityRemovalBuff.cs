using System;

namespace LuckyBlocks.Features.Buffs;

internal interface IDelayedImmunityRemovalBuff : IImmunityFlagsIndicatorBuff
{
    public TimeSpan ImmunityRemovalDelay { get; }
}