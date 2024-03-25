using LuckyBlocks.Features.Immunity;

namespace LuckyBlocks.Loot.Buffs;

internal interface IRepressibleByImmunityFlagsBuff : IBuff
{
    ImmunityFlag ImmunityFlags { get; }
}