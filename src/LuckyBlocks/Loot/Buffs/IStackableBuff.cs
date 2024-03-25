namespace LuckyBlocks.Loot.Buffs;

internal interface IStackableBuff : IBuff
{
    void ApplyAgain(IBuff additionalBuff);
}