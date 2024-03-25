using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.LegendaryWeapon;

internal class LegendaryWeapon : ILoot
{
    public Item Item => Item.LegendaryWeapon;
    public string Name => "Legendary weapon";

    private readonly Vector2 _spawnPosition;
    private readonly IGame _game;
    private readonly WeaponItem _weaponItem;

    public LegendaryWeapon(WeaponItem weaponItem, Vector2 spawnPosition, LootConstructorArgs args)
        => (_spawnPosition, _game, _weaponItem) = (spawnPosition, args.Game, weaponItem);

    public void Run()
    {
        _game.SpawnWeaponItem(_weaponItem, _spawnPosition, true, 20_000f);
    }
}