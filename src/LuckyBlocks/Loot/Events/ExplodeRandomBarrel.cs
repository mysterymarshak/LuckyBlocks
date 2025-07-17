using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Events;

internal class ExplodeRandomBarrel : ILoot
{
    public Item Item => Item.ExplodeRandomBarrel;
    public string Name => "Explode random barrel";

    private readonly IGame _game;

    public ExplodeRandomBarrel(LootConstructorArgs args)
        => (_game) = (args.Game);

    public void Run()
    {
        var barrels = _game.GetObjectsByName("BarrelExplosive");
        var barrel = barrels.GetRandomElement();
        barrel.DealDamage(barrel.GetHealth());
    }
}