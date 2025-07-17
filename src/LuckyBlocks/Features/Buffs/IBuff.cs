namespace LuckyBlocks.Features.Buffs;

internal interface IBuff
{
    string Name { get; }
    void Run();
}