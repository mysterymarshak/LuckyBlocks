namespace LuckyBlocks.Loot.Buffs;

internal interface IBuff
{
    string Name { get; }
    void Run();
}