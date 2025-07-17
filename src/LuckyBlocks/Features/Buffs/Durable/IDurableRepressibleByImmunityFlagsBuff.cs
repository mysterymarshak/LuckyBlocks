namespace LuckyBlocks.Features.Buffs.Durable;

internal interface IDurableRepressibleByImmunityFlagsBuff : IDurableBuff, IRepressibleByImmunityFlagsBuff
{
    void Repress(IFinishableBuff buff);
}