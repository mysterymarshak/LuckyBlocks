using System;

namespace LuckyBlocks.Features.Buffs.Durable;

internal interface IDurableBuff : IFinishableBuff, IStackableBuff, ICloneableBuff<IDurableBuff>
{
    TimeSpan Duration { get; }
    TimeSpan TimeLeft { get; }
}