using LuckyBlocks.Data.Args;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Events;

internal class Explosion : ILoot
{
    public Item Item => Item.Explosion;
    public string Name => "Explosion";

    private readonly Vector2 _explosionPosition;
    private readonly IGame _game;

    public Explosion(Vector2 explosionPosition, LootConstructorArgs args)
        => (_explosionPosition, _game) = (explosionPosition, args.Game);

    public void Run()
    {
        _game.TriggerExplosion(_explosionPosition);
    }
}