using System;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot.Attributes;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups;

internal class WeaponPowerupWrapper : ILoot
{
    public Item Item { get; }
    public string Name => _powerup!.Name;

    private Weapon Weapon => _powerup!.Weapon!;

    private readonly Type _powerupType;
    private readonly IPlayer _player;
    private readonly IWeaponsPowerupsService _weaponsPowerupsService;
    private readonly IPowerupFactory _powerupFactory;

    private IWeaponPowerup<Weapon>? _powerup;

    public WeaponPowerupWrapper(Type powerupType, Item item, IPlayer player, LootConstructorArgs args)
        => (Item, _powerupType, _player, _weaponsPowerupsService, _powerupFactory) =
            (item, powerupType, player, args.WeaponsPowerupsService, args.PowerupFactory);

    public void Run()
    {
        var incompatiblePowerups = EnumUtils.GetAttributesOfType<IncompatibleWithPowerupsAttribute, Item>(Item)
            .SelectMany(x => x.Types);
        
        var getWeaponForPowerupResult =
            _weaponsPowerupsService.TryGetWeaponForPowerup(_player, _powerupType, incompatiblePowerups);
        
        _powerup = CreatePowerup(getWeaponForPowerupResult);
        _weaponsPowerupsService.AddWeaponPowerup(_powerup, Weapon, _player);
    }

    private IWeaponPowerup<Weapon> CreatePowerup(OneOf<Firearm, Throwable, NotFound> weaponForPowerup) =>
        weaponForPowerup switch
        {
            { IsT0: true } => _powerupFactory.CreatePowerup(weaponForPowerup.AsT0, _powerupType),
            { IsT1: true } => _powerupFactory.CreatePowerup(weaponForPowerup.AsT1, _powerupType),
            _ => throw new ArgumentOutOfRangeException(nameof(weaponForPowerup))
        };
}