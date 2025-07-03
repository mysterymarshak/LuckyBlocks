using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Features.WeaponPowerups;
using OneOf;
using OneOf.Types;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal class WeaponPowerupWrapper : ILoot
{
    public Item Item { get; }
    public string Name => _powerup!.Name;

    private Weapon Weapon => _powerup!.Weapon;

    private readonly Type _powerupType;
    private readonly IPlayer _player;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IPowerupFactory _powerupFactory;

    private IWeaponPowerup<Weapon>? _powerup;

    public WeaponPowerupWrapper(Type powerupType, Item item, IPlayer player, LootConstructorArgs args)
        => (Item, _powerupType, _player, _weaponPowerupsService, _powerupFactory) =
            (item, powerupType, player, args.WeaponsPowerupsService, args.PowerupFactory);

    public void Run()
    {
        var getWeaponsForPowerupResult =
            _weaponPowerupsService.TryGetWeaponsForPowerup(_player, _powerupType);

        _powerup = CreatePowerup(getWeaponsForPowerupResult);
        _weaponPowerupsService.AddWeaponPowerup(_powerup, Weapon);
    }

    private IWeaponPowerup<Weapon> CreatePowerup(OneOf<NotFound, IEnumerable<Weapon>> weaponForPowerup) =>
        weaponForPowerup switch
        {
            { IsT1: true } => _powerupFactory.CreatePowerup(weaponForPowerup.AsT1.First(), _powerupType),
            _ => throw new ArgumentOutOfRangeException(nameof(weaponForPowerup))
        };
}