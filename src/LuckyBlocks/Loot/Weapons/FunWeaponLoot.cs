using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.Bullets;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class FunWeaponLoot : PowerUppedWeaponBase
{
    public override string Name =>
        $"{_funWeapon.WeaponItem} with [{string.Join(", ", GetPowerups().Select(x => x.Name))}]";

    public override Item Item => Item.FunWeapon;

    protected override WeaponItem WeaponItem => _funWeapon.WeaponItem;

    private const double LostPowerupChance = 0.3;

    private static readonly List<FunWeapon> FunWeapons =
    [
        new(WeaponItem.BOW, [typeof(PushBullets), typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.BOW, [typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.GRENADE_LAUNCHER, [typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.BAZOOKA, [typeof(InfiniteRicochetBullets)]),
        new(WeaponItem.FLAREGUN, [typeof(InfiniteRicochetBullets), typeof(PushBullets)]),
        new(WeaponItem.SAWED_OFF, [typeof(InfiniteRicochetBullets), typeof(LostBullets)])
    ];

    private readonly IPowerupFactory _powerupFactory;
    private readonly FunWeapon _funWeapon;
    private readonly bool _isLostPowerupped;

    public FunWeaponLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
        _funWeapon = FunWeapons.GetRandomElement();
        _isLostPowerupped = SharedRandom.Instance.NextDouble() <= LostPowerupChance;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        return GetPowerups().Select(powerupType => _powerupFactory.CreatePowerup(weapon, powerupType));
    }

    private IEnumerable<Type> GetPowerups()
    {
        var alreadyContainsLostPowerup = false;
        foreach (var powerupType in _funWeapon.Powerups)
        {
            if (powerupType == typeof(LostBullets))
            {
                alreadyContainsLostPowerup = true;
            }

            yield return powerupType;
        }

        if (_isLostPowerupped && !alreadyContainsLostPowerup)
        {
            yield return typeof(LostBullets);
        }
    }

    private readonly record struct FunWeapon(WeaponItem WeaponItem, IEnumerable<Type> Powerups);
}