namespace LuckyBlocks.Features.Immunity;

internal interface IImmunity
{
    string Name { get; }
    ImmunityFlag Flag { get; }
}