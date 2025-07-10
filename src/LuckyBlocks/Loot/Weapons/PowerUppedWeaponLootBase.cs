using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal abstract class PowerUppedWeaponBase : ILoot
{
    public abstract Item Item { get; }
    public abstract string Name { get; }

    protected abstract WeaponItem WeaponItem { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private const double BoobyTrappedChance = 0.1;

    private readonly Vector2 _spawnPosition;
    private readonly IPowerupFactory _powerupFactory;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IWeaponsDataWatcher _weaponsDataWatcher;
    private readonly IGame _game;

    protected PowerUppedWeaponBase(Vector2 spawnPosition, LootConstructorArgs args)
    {
        _spawnPosition = spawnPosition;
        _powerupFactory = args.PowerupFactory;
        _weaponPowerupsService = args.WeaponsPowerupsService;
        _game = args.Game;
        _weaponsDataWatcher = args.WeaponsDataWatcher;
        var thisScope = args.LifetimeScope.BeginLifetimeScope();
        ExtendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public virtual void Run()
    {
        var weaponItem = WeaponItem;
        var weaponObject = _game.SpawnWeaponItem(weaponItem, _spawnPosition, true, 20_000f);
        var weapon = _weaponsDataWatcher.RegisterWeapon(weaponObject);

        AddPowerups(weapon);
    }

    protected abstract IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon);

    private IEnumerable<IWeaponPowerup<Weapon>> GetPowerupsInternal(Weapon weapon)
    {
        foreach (var powerup in GetPowerups(weapon))
        {
            yield return powerup;
        }

        if (WeaponItem.GetWeaponItemType() is WeaponItemType.Melee or WeaponItemType.Handgun or WeaponItemType.Rifle
                or WeaponItemType.Thrown && SharedRandom.Instance.NextDouble() <= BoobyTrappedChance)
        {
            yield return _powerupFactory.CreatePowerup(weapon, typeof(BoobyTrapped));
        }
    }

    private void AddPowerups(Weapon weapon)
    {
        foreach (var powerup in GetPowerupsInternal(weapon))
        {
            _weaponPowerupsService.AddWeaponPowerup(powerup, weapon);
        }
    }
}