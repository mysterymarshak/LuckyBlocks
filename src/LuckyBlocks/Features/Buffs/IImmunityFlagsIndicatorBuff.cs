using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Features.Buffs;

internal interface IImmunityFlagsIndicatorBuff : IBuff
{
    ImmunityFlag ImmunityFlags { get; }
}