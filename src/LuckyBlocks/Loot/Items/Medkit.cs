using LuckyBlocks.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Items;

internal class Medkit : ILoot
{
    public Item Item => Item.Medkit;
    public string Name => "Medkit";

    private readonly Vector2 _spawnPosition;
    private readonly IGame _game;

    public Medkit(Vector2 spawnPosition, LootConstructorArgs args)
        => (_spawnPosition, _game) = (spawnPosition, args.Game);

    public void Run()
    {
        var className = SharedRandom.Instance.NextDouble() > 0.5 ? "ItemPills" : "ItemMedkit";
        _game.CreateObject(className, _spawnPosition);
    }
}