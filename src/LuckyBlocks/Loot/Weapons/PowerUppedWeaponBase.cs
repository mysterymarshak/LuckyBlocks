using Autofac;
using LuckyBlocks.Data;
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
    protected abstract WeaponItemType WeaponItemType { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private readonly Vector2 _spawnPosition;
    private readonly IWeaponsPowerupsService _weaponsPowerupsService;
    private readonly IGame _game;

    private WeaponEventsWatcher? _weaponEventsWatcher;

    protected PowerUppedWeaponBase(Vector2 spawnPosition, LootConstructorArgs args)
        => (_spawnPosition, _weaponsPowerupsService, _game, ExtendedEvents) = (spawnPosition,
            args.WeaponsPowerupsService, args.Game, args.LifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>());

    public virtual void Run()
    {
        var weaponItem = WeaponItem;
        var weapon = _game.SpawnWeaponItem(weaponItem, _spawnPosition, true, 20_000f);
        OnWeaponCreated(weapon);
        
        _weaponEventsWatcher = WeaponEventsWatcher.CreateForWeapon(weapon);
        _weaponEventsWatcher.Pickup += OnWeaponPickedUp;
        _weaponEventsWatcher.Start();
    }

    protected abstract IWeaponPowerup<Weapon> GetPowerup(Weapon weapon);

    protected virtual void OnWeaponCreated(IObjectWeaponItem weaponItem)
    {
    }

    private void OnWeaponPickedUp(IPlayer player)
    {
        var weaponsData = player.GetWeaponsData();
        var weapon = weaponsData.GetWeaponByType(WeaponItemType);

        var powerup = GetPowerup(weapon);
        _weaponsPowerupsService.AddWeaponPowerup(powerup, weapon, player);

        _weaponEventsWatcher!.Dispose();
    }
}