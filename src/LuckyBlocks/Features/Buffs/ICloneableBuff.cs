namespace LuckyBlocks.Features.Buffs;

internal interface ICloneableBuff<out T> : IBuff where T : IBuff
{
    T Clone();
}