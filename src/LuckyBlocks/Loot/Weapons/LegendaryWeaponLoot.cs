using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot.WeaponPowerups;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class LegendaryWeaponLoot : PowerUppedWeaponBase
{
    public override string Name => "Legendary weapon";
    public override Item Item => Item.LegendaryWeapon;

    protected override WeaponItem WeaponItem { get; }

    private static readonly List<WeaponItem> LegendaryWeapons =
    [
        WeaponItem.BAZOOKA,
        WeaponItem.GRENADE_LAUNCHER,
        WeaponItem.SNIPER,
        WeaponItem.M60,
        WeaponItem.CHAINSAW,
        WeaponItem.MAGNUM,
        WeaponItem.FLAREGUN
    ];
    
    public LegendaryWeaponLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        WeaponItem = LegendaryWeapons.GetRandomElement();
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield break;
    }
}