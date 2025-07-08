using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using LuckyBlocks.Loot.WeaponPowerups.Melees;
using LuckyBlocks.Loot.WeaponPowerups.ThrownItems;
using OneOf;
using OneOf.Types;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class WeaponWithRandomPowerup : PowerUppedWeaponBase
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

    private static readonly List<(Type, Item)> FirearmPowerups = new()
    {
        (typeof(AimBullets), Item.AimBullets), (typeof(ExplosiveBullets), Item.ExplosiveBullets),
        (typeof(FreezeBullets), Item.FreezeBullets), (typeof(InfiniteRicochetBullets), Item.InfiniteRicochetBullets),
        (typeof(PushBullets), Item.PushBullets), (typeof(TripleRicochetBullets), Item.TripleRicochetBullets),
        (typeof(PoisonBullets), Item.PoisonBullets)
    };

    private static readonly List<(Type, Item)> GrenadePowerups = new()
    {
        (typeof(StickyGrenades), Item.StickyGrenades), (typeof(BananaGrenades), Item.BananaGrenades)
    };

    protected override WeaponItem WeaponItem => _weaponItem;
    protected override WeaponItemType WeaponItemType => WeaponItem.GetWeaponItemType();

    private readonly WeaponItem _weaponItem;
    private readonly Type _powerupType;
    private readonly IPowerupFactory _powerupFactory;
    private readonly IAttributesChecker _attributesChecker;

    public WeaponWithRandomPowerup(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _weaponItem = WeaponItems.GetRandomElement();
        _powerupFactory = args.PowerupFactory;
        _attributesChecker = args.AttributesChecker;
        _powerupType = GetRandomPowerup();
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, _powerupType);
    }

    private Type GetRandomPowerup()
    {
        Type? powerupType = null;
        var item = Item.None;

        while (powerupType is null ||
               !_attributesChecker.Check(item, OneOf<Player, Unknown>.FromT1(new Unknown()), true))
        {
            (powerupType, item) = _weaponItem switch
            {
                WeaponItem.GRENADES => GrenadePowerups.GetRandomElement(),
                WeaponItem.KATANA => (typeof(FlamyKatana), Item.FlamyKatana),
                _ => FirearmPowerups.GetRandomElement()
            };
        }

        return powerupType;
    }
}