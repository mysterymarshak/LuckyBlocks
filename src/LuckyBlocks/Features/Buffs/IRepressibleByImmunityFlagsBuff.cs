using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Features.Buffs;

internal interface IRepressibleByImmunityFlagsBuff : IBuff
{
    ImmunityFlag ImmunityFlags { get; }
}