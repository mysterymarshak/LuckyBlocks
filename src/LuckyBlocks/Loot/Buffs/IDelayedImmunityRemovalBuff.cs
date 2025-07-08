using System;

namespace LuckyBlocks.Loot.Buffs;

internal interface IDelayedImmunityRemovalBuff : IImmunityFlagsIndicatorBuff
{
    public TimeSpan ImmunityRemovalDelay { get; }
}