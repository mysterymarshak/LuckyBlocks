using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.InfiniteBouncing;

internal abstract class WeaponWithInfiniteBouncingBase : PowerUppedWeaponBase
{
    private readonly IPowerupFactory _powerupFactory;

    protected WeaponWithInfiniteBouncingBase(Vector2 spawnPosition, LootConstructorArgs lootConstructorArgs) : base(
        spawnPosition, lootConstructorArgs)
        => (_powerupFactory) = (lootConstructorArgs.PowerupFactory);

    public override void Run()
    {
        if (WeaponItemType is not (WeaponItemType.Rifle or WeaponItemType.Handgun))
            throw new ArgumentOutOfRangeException(nameof(WeaponItemType));

        base.Run();
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, typeof(InfiniteRicochetBullets));
    }
}