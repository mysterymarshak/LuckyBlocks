using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.LegendaryWeapon;

internal class LegendaryWeaponWrapper : ILoot
{
    public Item Item => _legendaryWeaponLoot.Item;
    public string Name => _legendaryWeaponLoot.Name;

    private const double MinedChance = 0.1;

    private static readonly IReadOnlyList<WeaponWithType> LegendaryWeapons = new List<WeaponWithType>
    {
        new(WeaponItem.BAZOOKA, WeaponItemType.Rifle),
        new(WeaponItem.GRENADE_LAUNCHER, WeaponItemType.Rifle),
        new(WeaponItem.SNIPER, WeaponItemType.Rifle),
        new(WeaponItem.M60, WeaponItemType.Rifle),
        new(WeaponItem.CHAINSAW, WeaponItemType.Melee),
        new(WeaponItem.MAGNUM, WeaponItemType.Handgun),
        new(WeaponItem.FLAREGUN, WeaponItemType.Handgun)
    };
    
    private readonly ILoot _legendaryWeaponLoot;

    public LegendaryWeaponWrapper(Vector2 spawnPosition, LootConstructorArgs lootConstructorArgs)
    {
        var weaponWithType = LegendaryWeapons.GetRandomElement();
        _legendaryWeaponLoot = SharedRandom.Instance.NextDouble() <= MinedChance
            ? new LegendaryWeaponMined(weaponWithType, spawnPosition, lootConstructorArgs)
            : new LegendaryWeapon(weaponWithType.WeaponItem, spawnPosition, lootConstructorArgs);
    }

    public void Run()
    {
        _legendaryWeaponLoot.Run();
    }
}