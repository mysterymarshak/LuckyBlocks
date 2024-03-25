namespace LuckyBlocks.Features.Immunity;

internal interface IApplicableImmunity : IImmunity
{
    void Apply();
    void Remove();
}