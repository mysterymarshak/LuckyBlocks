using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Loot.Buffs;

internal interface IImmunityFlagsIndicatorBuff : IBuff
{
    ImmunityFlag ImmunityFlags { get; }
}