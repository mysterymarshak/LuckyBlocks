using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IWeaponsDataWatcher
{
    void Initialize();
    Weapon RegisterWeapon(IObjectWeaponItem objectWeaponItem);
}

[Inject]
internal class WeaponsDataWatcher : IWeaponsDataWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly IIdentityService _identityService;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly INotificationService _notificationService;
    private readonly IThrowableWeaponsWatcher _throwableWeaponsWatcher;
    private readonly IReloadWeaponsWatcher _reloadWeaponsWatcher;
    private readonly IDrawnWeaponsWatcher _drawnWeaponsWatcher;
    private readonly IChainsawWeaponsWatcher _chainsawWeaponsWatcher;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Dictionary<int, Weapon> _droppedWeapons;

    private readonly List<IObjectAmmoStashTrigger> _ammoTriggers;

    public WeaponsDataWatcher(IIdentityService identityService, IWeaponPowerupsService weaponPowerupsService,
        INotificationService notificationService, IThrowableWeaponsWatcher throwableWeaponsWatcher,
        IReloadWeaponsWatcher reloadWeaponsWatcher, IDrawnWeaponsWatcher drawnWeaponsWatcher,
        IChainsawWeaponsWatcher chainsawWeaponsWatcher, IGame game, ILifetimeScope lifetimeScope)
    {
        _identityService = identityService;
        _weaponPowerupsService = weaponPowerupsService;
        _notificationService = notificationService;
        _throwableWeaponsWatcher = throwableWeaponsWatcher;
        _reloadWeaponsWatcher = reloadWeaponsWatcher;
        _drawnWeaponsWatcher = drawnWeaponsWatcher;
        _chainsawWeaponsWatcher = chainsawWeaponsWatcher;
        _game = game;
        _droppedWeapons = new Dictionary<int, Weapon>();
        _ammoTriggers = [];
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnWeaponAdded(OnWeaponAdded, EventHookMode.Prioritized);
        _extendedEvents.HookOnWeaponRemoved(OnWeaponRemoved, EventHookMode.Prioritized);
        _extendedEvents.HookOnProjectilesCreated(OnProjectilesCreated, EventHookMode.Prioritized);
        _extendedEvents.HookOnPlayerMeleeAction(OnMeleeAction, EventHookMode.Prioritized);
        _extendedEvents.HookOnDestroyed(OnObjectDestroyed, EventHookMode.Default);
        _extendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Prioritized);
        _extendedEvents.HookOnPlayerCreated(OnPlayerCreated, EventHookMode.Default);
        _ammoTriggers.AddRange(_game.GetObjects<IObjectAmmoStashTrigger>());
        _drawnWeaponsWatcher.Initialize();
        _reloadWeaponsWatcher.Initialize();
    }

    public Weapon RegisterWeapon(IObjectWeaponItem objectWeaponItem)
    {
        if (_droppedWeapons.TryGetValue(objectWeaponItem.UniqueId, out var weapon))
        {
            Logger.Warning(
                "Weapon {WeaponItem} with ID {ObjectId} already registered as dropped weapon, returning existing weapon.",
                objectWeaponItem.WeaponItem, objectWeaponItem.UniqueId);
        }
        else
        {
            weapon = objectWeaponItem.ToWeapon();
            _droppedWeapons.Add(objectWeaponItem.UniqueId, weapon);
        }

        return weapon;
    }

    private void OnPlayerCreated(Event<IPlayer[]> @event)
    {
        foreach (var playerInstance in @event.Args)
        {
            var player = _identityService.GetPlayerByInstance(playerInstance);
            player.SetWeaponsData(playerInstance.CreateWeaponsData());

            Logger.Debug("Initialized weapon data for {Player}", player.Name);
        }
    }

    private void OnWeaponAdded(Event<IPlayer, PlayerWeaponAddedArg> @event)
    {
        try
        {
            var (playerInstance, args, _) = @event;
            if (!playerInstance.IsValid())
                return;

            var player = _identityService.GetPlayerByInstance(playerInstance);
            if (args.SourceObjectID != 0 && _droppedWeapons.TryGetValue(args.SourceObjectID, out var droppedWeapon))
            {
                var weaponsData = player.WeaponsData;
                var existingWeapon = weaponsData.GetWeaponByType(args.WeaponItemType, droppedWeapon is MeleeTemp);
                var existingWeaponIsValid = !existingWeapon.IsInvalid;
                var powerupsToConcat = Enumerable.Empty<IWeaponPowerup<Weapon>>();
                if (existingWeaponIsValid &&
                    !IsPickedUpWeaponCompatible(existingWeapon, droppedWeapon, player, out powerupsToConcat))
                    return;

                if ((!existingWeaponIsValid || existingWeapon.WeaponItem != args.WeaponItem) &&
                    args.WeaponItemType != WeaponItemType.InstantPickup)
                {
                    weaponsData.AddWeapon(droppedWeapon);
                    droppedWeapon.SetOwner(playerInstance);

                    droppedWeapon.RaiseEvent(WeaponEvent.PickedUp);
                }

                if (existingWeaponIsValid && existingWeapon.WeaponItem == args.WeaponItem)
                {
                    player.UpdateWeaponData(args.WeaponItemType, existingWeapon is MeleeTemp);
                    _weaponPowerupsService.ConcatPowerups(existingWeapon, powerupsToConcat);
                }

                weaponsData.UpdateDrawn();

                Logger.Debug("Picked up dropped weapon: {WeaponItem}, id: {ObjectId}, owner: {Player}",
                    droppedWeapon.WeaponItem, args.SourceObjectID, player.Name);
            }
            else
            {
                OnPickedUpNewWeapon(player, args, playerInstance);
            }

            if (args.WeaponItemType == WeaponItemType.Thrown)
            {
                _throwableWeaponsWatcher.OnThrowablePickedUp(player);
            }

            if (args.WeaponItem == WeaponItem.CHAINSAW)
            {
                _chainsawWeaponsWatcher.StartTrackingChainsaw(player);
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing weapon added event: {Message}", exception);
        }
    }

    private bool IsPickedUpWeaponCompatible(Weapon existingWeapon, Weapon pickedUpWeapon, Player player,
        out IEnumerable<IWeaponPowerup<Weapon>> powerupsToConcat)
    {
        var powerupsToConcatInternal = new List<IWeaponPowerup<Weapon>>();
        foreach (var pickedUpPowerup in pickedUpWeapon.Powerups)
        {
            foreach (var existingPowerup in existingWeapon.Powerups)
            {
                if (existingPowerup.IsCompatibleWith(pickedUpPowerup.GetType()))
                    continue;

                var playerInstance = player.Instance!;
                playerInstance.SetAmmoFromWeapon(pickedUpWeapon);
                var disarmedWeapon = playerInstance.Disarm(pickedUpWeapon.WeaponItemType);
                OnWeaponDropped(pickedUpWeapon, disarmedWeapon, player, WeaponEvent.Dropped);

                _notificationService.CreateTextNotification($"Incompatible | {pickedUpWeapon.GetFormattedName()}",
                    Color.Grey, TimeSpan.FromSeconds(2), disarmedWeapon.GetWorldPosition());

                playerInstance.GiveWeaponItem(pickedUpWeapon.WeaponItem, false);
                playerInstance.SetAmmoFromWeapon(existingWeapon);

                powerupsToConcat = [];
                return false;
            }

            powerupsToConcatInternal.Add(pickedUpPowerup);
        }

        powerupsToConcat = powerupsToConcatInternal;

        return true;
    }

    private static void OnPickedUpNewWeapon(Player player, PlayerWeaponAddedArg args, IPlayer playerInstance)
    {
        if (args.WeaponItemType != WeaponItemType.InstantPickup)
        {
            var weaponsData = player.WeaponsData;
            var isMakeshift = playerInstance.CurrentMeleeMakeshiftWeapon.WeaponItem == args.WeaponItem;
            player.UpdateWeaponData(args.WeaponItemType, isMakeshift);
            var pickedUpWeapon = weaponsData.GetWeaponByType(args.WeaponItemType, isMakeshift);

            if (pickedUpWeapon.IsDropped)
            {
                pickedUpWeapon.SetOwner(playerInstance);
                pickedUpWeapon.RaiseEvent(WeaponEvent.PickedUp);
            }

            weaponsData.UpdateDrawn();
        }

        Logger.Debug("Picked up weapon: {WeaponItem} {ObjectId}, owner: {Player}", args.WeaponItem,
            args.SourceObjectID, player.Name);
    }

    private void OnWeaponRemoved(Event<IPlayer, PlayerWeaponRemovedArg> @event)
    {
        try
        {
            var (playerInstance, args, _) = @event;
            if (!playerInstance.IsValid())
                return;

            var player = _identityService.GetPlayerByInstance(playerInstance);
            var weaponsData = player.WeaponsData;
            var removedWeapon = weaponsData.GetWeaponByType(args.WeaponItemType,
                weaponsData.MeleeWeaponTemp.WeaponItem == args.WeaponItem);

            if (args.TargetObjectID != 0)
            {
                var @object = _game.GetObject(args.TargetObjectID);
                if (args.Thrown)
                {
                    var isGrenadeThrown = @object is not IObjectWeaponItem;

                    if (isGrenadeThrown && removedWeapon is Grenade grenade && @object is IObjectGrenadeThrown)
                    {
                        OnGrenadeThrown(grenade, args, player, @object);
                    }

                    if (!isGrenadeThrown)
                    {
                        OnWeaponDropped(removedWeapon, @object, player, WeaponEvent.Thrown);
                    }
                }
                else
                {
                    OnWeaponDropped(removedWeapon, @object, player, WeaponEvent.Dropped);
                }
            }
            else
            {
                if (args is not { Thrown: true, WeaponItemType: WeaponItemType.Thrown, TargetObjectID: 0 })
                {
                    DisposeWeapon(removedWeapon);
                }
            }

            player.UpdateWeaponData(args.WeaponItemType, removedWeapon is MeleeTemp, true);

            if (args.WeaponItemType == WeaponItemType.Thrown && player.WeaponsData.ThrowableItem.IsInvalid)
            {
                _throwableWeaponsWatcher.OnThrowableRemoved(player);
            }

            if (args.WeaponItem == WeaponItem.CHAINSAW)
            {
                _chainsawWeaponsWatcher.RemoveChainsawTracking(player, removedWeapon);
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing weapon removed event: {Message}", exception);
        }
    }

    private void OnWeaponDropped(Weapon removedWeapon, IObject @object, Player? player, WeaponEvent weaponEvent)
    {
        var objectId = @object.UniqueId;
        if (_droppedWeapons.ContainsKey(objectId))
            return;

        removedWeapon.SetObject(objectId);
        removedWeapon.RaiseEvent(weaponEvent, @object as IObjectWeaponItem);

        _droppedWeapons.Add(objectId, removedWeapon);

        Logger.Debug("Weapon dropped: {WeaponItem} id {ObjectId}, owner: {Player}",
            removedWeapon.WeaponItem, objectId, player?.Name);
    }

    private void OnGrenadeThrown(Grenade grenade, PlayerWeaponRemovedArg args, Player player, IObject grenadeObject)
    {
        var thrownGrenade = new Grenade(grenade.WeaponItem, grenade.WeaponItemType, 1, grenade.MaxAmmo,
            false, grenade.TimeToExplosion);
        thrownGrenade.SetObject(args.TargetObjectID);
        _throwableWeaponsWatcher.OnGrenadeThrown(thrownGrenade);

        grenade.RaiseEvent(WeaponEvent.GrenadeThrown, player.Instance, grenadeObject, thrownGrenade);

        Logger.Debug("grenade thrown id: {ObjectId}, owner: {Player}, timer: {TimeLeft}", args.TargetObjectID,
            player.Name, grenade.TimeToExplosion);
    }

    private void OnProjectilesCreated(Event<IProjectile[]> @event)
    {
        try
        {
            var handledWeapons = new Dictionary<int, List<ProjectileItem>>();

            foreach (var projectile in @event.Args)
            {
                var playerId = projectile.InitialOwnerPlayerID;
                var playerInstance = _game.GetPlayer(playerId);
                if (playerInstance?.IsValid() != true)
                    continue;

                if (!handledWeapons.TryGetValue(playerId, out var weapons))
                {
                    weapons = [];
                    handledWeapons.Add(playerId, weapons);
                }

                if (weapons.Contains(projectile.ProjectileItem))
                    continue;

                weapons.Add(projectile.ProjectileItem);

                var player = _identityService.GetPlayerByInstance(playerInstance);
                var weaponsData = player.WeaponsData;
                var shotWeapon =
                    weaponsData.GetWeaponByType(((WeaponItem)(int)projectile.ProjectileItem).GetWeaponItemType(),
                        false);

                player.UpdateWeaponData(shotWeapon.WeaponItemType);
                shotWeapon.RaiseEvent(WeaponEvent.Fired, playerInstance, @event.Args);

                Logger.Debug("Shoot with weapon {WeaponItem}, player: {Player}, ammo left: {AmmoLeft}",
                    projectile.ProjectileItem, player.Name, (shotWeapon as Firearm)!.CurrentAmmo);
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing projectiles created event: {Message}", exception);
        }
    }

    private void OnMeleeAction(Event<IPlayer, PlayerMeleeHitArg[]> @event)
    {
        try
        {
            var (playerInstance, args, _) = @event;
            if (playerInstance?.IsValid() != true)
                return;

            foreach (var meleeHit in args)
            {
                if (playerInstance.CurrentWeaponDrawn == WeaponItemType.Melee)
                {
                    var player = _identityService.GetPlayerByInstance(playerInstance);
                    var weaponsData = player.WeaponsData;
                    var currentDrawn = weaponsData.CurrentWeaponDrawn;

                    player.UpdateWeaponData(WeaponItemType.Melee, currentDrawn is MeleeTemp);
                    currentDrawn.RaiseEvent(WeaponEvent.MeleeHit, meleeHit);

                    Logger.Debug("Melee hit with {WeaponItem}, player: {Player}, durability left: {DurabilityLeft}",
                        currentDrawn.WeaponItem, player.Name, ((Melee)currentDrawn).CurrentDurability);
                }
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing melee action event: {Message}", exception);
        }
    }

    private void OnObjectDestroyed(Event<IObject[]> @event)
    {
        try
        {
            foreach (var @object in @event.Args)
            {
                if (_droppedWeapons.ContainsKey(@object.UniqueId))
                {
                    Awaiter.Start(() =>
                    {
                        var weapon = _droppedWeapons[@object.UniqueId];
                        var weaponObject = _game.GetObject(weapon.ObjectId);
                        if (weaponObject?.IsValid() != true && weapon.IsDropped)
                        {
                            DisposeWeapon(weapon);
                        }

                        _droppedWeapons.Remove(@object.UniqueId);

                        Logger.Debug("Removed dropped weapon {WeaponItem} with ID {ObjectId}", weapon.WeaponItem,
                            @object.UniqueId);
                    }, TimeSpan.Zero);
                }
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing object destroyed event: {Message}", exception);
        }
    }

    private void OnKeyInput(Event<IPlayer, VirtualKeyInfo[]> @event)
    {
        try
        {
            var (playerInstance, keyInputs, _) = @event;
            foreach (var keyInput in keyInputs)
            {
                if (keyInput is not
                    {
                        Event: VirtualKeyEvent.Pressed,
                        Key: VirtualKey.ACTIVATE or VirtualKey.ATTACK or VirtualKey.RELOAD
                    })
                    continue;

                var player = _identityService.GetPlayerByInstance(playerInstance);
                var weaponsData = player.WeaponsData;
                var currentDrawn = weaponsData.CurrentWeaponDrawn;

                if (keyInput.Key == VirtualKey.ACTIVATE && weaponsData.HasAnyFirearm() && _ammoTriggers.Count > 0 &&
                    _ammoTriggers.Any(x =>
                    {
                        var aabb = x.GetAABB();
                        aabb.Grow(2f);
                        return aabb.Intersects(playerInstance.GetAABB());
                    }))
                {
                    UpdateFirearms(player);
                }

                if (playerInstance.IsReloading || keyInput.Key == VirtualKey.ATTACK &&
                    currentDrawn is Firearm { CurrentSpareMags: > 0, CurrentAmmo: 0 })
                {
                    _reloadWeaponsWatcher.StartReloadTracking(player, currentDrawn.WeaponItemType);
                }
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing key input event: {Message}", exception);
        }
    }

    private void UpdateFirearms(Player player)
    {
        var weaponsData = player.WeaponsData;
        weaponsData.UpdateFirearms();

#if DEBUG
        Logger.Debug(
            "Updated firearms weapons for player: {Player}, new handgun: {HandgunItem} {HandgunAmmo}, new rifle: {RifleItem} {RifleAmmo}",
            player.Name, weaponsData.SecondaryWeapon.WeaponItem, weaponsData.SecondaryWeapon.CurrentAmmo,
            weaponsData.PrimaryWeapon.WeaponItem, weaponsData.PrimaryWeapon.CurrentAmmo);
#endif
    }

    private static void DisposeWeapon(Weapon weapon)
    {
        weapon.RaiseEvent(WeaponEvent.Disposed);
        Logger.Debug("Disposed weapon {WeaponItem}", weapon.WeaponItem);
    }
}