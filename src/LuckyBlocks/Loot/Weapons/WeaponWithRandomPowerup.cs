using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class WeaponWithRandomPowerup : PowerUppedWeaponBase
{
    public override string Name => $"{_weaponItem} with {_powerupType.Name}";
    public override Item Item => Item.WeaponWithRandomPowerup;

    private static readonly List<WeaponItem> Exceptions = [WeaponItem.FLAMETHROWER];
    private static readonly List<WeaponItem> Inclusions = [WeaponItem.GRENADES];
    
    private static readonly List<WeaponItem> WeaponItems = Enum.GetValues(typeof(WeaponItem))
        .Cast<WeaponItem>()
        .Where(x => x.GetWeaponItemType() is WeaponItemType.Handgun or WeaponItemType.Rifle)
        .Concat(Inclusions)
        .Except(Exceptions)
        .ToList();

    private static readonly List<Type> FirearmPowerups = new()
    {
        typeof(AimBullets), typeof(ExplosiveBullets), typeof(FreezeBullets), typeof(InfiniteRicochetBullets),
        typeof(PushBullets), typeof(TripleRicochetBullets)
    };

    protected override WeaponItem WeaponItem => _weaponItem;
    protected override WeaponItemType WeaponItemType => WeaponItem.GetWeaponItemType();

    private readonly WeaponItem _weaponItem;
    private readonly Type _powerupType;
    private readonly IPowerupFactory _powerupFactory;

    public WeaponWithRandomPowerup(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _weaponItem = WeaponItems.GetRandomElement();
        _powerupType = GetRandomPowerup();
        _powerupFactory = args.PowerupFactory;
    }

    protected override IWeaponPowerup<Weapon> GetPowerup(Weapon weapon)
    {
        return _powerupFactory.CreatePowerup(weapon, _powerupType);
    }

    private Type GetRandomPowerup() => _weaponItem switch
    {
        WeaponItem.GRENADES => typeof(StickyGrenadesLoot),
        _ => FirearmPowerups.GetRandomElement()
    };
}