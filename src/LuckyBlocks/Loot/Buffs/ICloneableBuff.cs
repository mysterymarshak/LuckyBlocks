namespace LuckyBlocks.Loot.Buffs;

internal interface ICloneableBuff<out T> : IBuff where T : IBuff
{
    T Clone();
}