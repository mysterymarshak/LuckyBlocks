using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class FunWeaponLoot : PowerUppedWeaponBase
{
    public override string Name =>
        $"{_funWeapon.WeaponItem} with [{string.Join(", ", _funWeapon.Powerups.Select(x => x.Name))}]";

    public override Item Item => Item.FunWeapon;

    protected override WeaponItem WeaponItem => _funWeapon.WeaponItem;

    private static readonly List<FunWeapon> FunWeapons =
    [
        new(WeaponItem.BOW, [typeof(PushBullets), typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.BOW, [typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.GRENADE_LAUNCHER, [typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.BAZOOKA, [typeof(InfiniteRicochetBullets)])
    ];

    private readonly IPowerupFactory _powerupFactory;
    private readonly FunWeapon _funWeapon;

    public FunWeaponLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
        _funWeapon = FunWeapons.GetRandomElement();
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        return _funWeapon.Powerups.Select(powerupType => _powerupFactory.CreatePowerup(weapon, powerupType));
    }

    private readonly record struct FunWeapon(WeaponItem WeaponItem, IEnumerable<Type> Powerups);
}