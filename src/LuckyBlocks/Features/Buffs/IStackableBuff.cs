namespace LuckyBlocks.Features.Buffs;

internal interface IStackableBuff : IBuff
{
    void ApplyAgain(IBuff additionalBuff);
}