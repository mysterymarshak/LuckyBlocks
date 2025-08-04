using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.Bullets;
using LuckyBlocks.Features.WeaponPowerups.Melees;
using LuckyBlocks.Features.WeaponPowerups.ThrownItems;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class WeaponWithRandomPowerupLoot : PowerUppedWeaponBase
{
    public override string Name => $"{_weaponItem} with {_powerupType.Name}";
    public override Item Item => Item.WeaponWithRandomPowerup;

    private static readonly List<WeaponItem> Exceptions = [WeaponItem.FLAMETHROWER];
    private static readonly List<WeaponItem> Inclusions = [WeaponItem.GRENADES, WeaponItem.KATANA];

    private static readonly List<WeaponItem> WeaponItems = Enum.GetValues(typeof(WeaponItem))
        .Cast<WeaponItem>()
        .Where(x => x.GetWeaponItemType() is WeaponItemType.Handgun or WeaponItemType.Rifle)
        .Concat(Inclusions)
        .Except(Exceptions)
        .ToList();

    private static readonly List<Type> FirearmPowerups = new()
    {
        typeof(AimBullets), typeof(ExplosiveBullets),
        typeof(FreezeBullets), typeof(InfiniteRicochetBullets),
        typeof(PushBullets), typeof(TripleRicochetBullets),
        typeof(PoisonBullets)
    };

    private static readonly List<Type> GrenadePowerups = new()
    {
        typeof(StickyGrenades), typeof(BananaGrenades)
    };

    protected override WeaponItem WeaponItem => _weaponItem;

    private readonly WeaponItem _weaponItem;
    private readonly Type _powerupType;
    private readonly IPowerupFactory _powerupFactory;

    public WeaponWithRandomPowerupLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _weaponItem = WeaponItems.GetRandomElement();
        _powerupFactory = args.PowerupFactory;
        _powerupType = GetRandomPowerup();
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, _powerupType);
    }

    private Type GetRandomPowerup()
    {
        var powerupType = _weaponItem switch
        {
            WeaponItem.GRENADES => GrenadePowerups.GetRandomElement(),
            WeaponItem.KATANA => typeof(FlamyKatana),
            _ => FirearmPowerups.GetRandomElement()
        };

        return powerupType;
    }
}