using System;

namespace LuckyBlocks.Features.Immunity;

internal interface IDelayedRemoveImmunity : IImmunity
{
    public TimeSpan RemovalDelay { get; }
}