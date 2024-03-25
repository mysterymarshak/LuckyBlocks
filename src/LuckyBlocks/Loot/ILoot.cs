namespace LuckyBlocks.Loot;

internal interface ILoot
{
    Item Item { get; }
    string Name { get; }
    void Run();
}