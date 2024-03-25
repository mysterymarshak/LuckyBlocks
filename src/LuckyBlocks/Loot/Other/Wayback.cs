using LuckyBlocks.Data;
using LuckyBlocks.Wayback;

namespace LuckyBlocks.Loot.Other;

internal class Wayback : ILoot
{
    public Item Item => Item.Wayback;
    public string Name => "Wayback";

    private readonly IWaybackMachine _waybackMachine;

    public Wayback(LootConstructorArgs args)
        => (_waybackMachine) = (args.WaybackMachine);
    
    public void Run()
    {
        _waybackMachine.RestoreFromRandomSnapshot();
    }
}