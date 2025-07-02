using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IWeaponsDataWatcher
{
    void Initialize();
}

[Inject]
internal class WeaponsDataWatcher : IWeaponsDataWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly IIdentityService _identityService;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly List<Grenade> _thrownGrenades;
    private readonly List<Player> _playersWithThrowable;
    private readonly Dictionary<int, Weapon> _droppedWeapons;
    private readonly Dictionary<int, TimerBase> _drawnWeaponTimers;
    private readonly Dictionary<int, List<ReloadAwaitingData>> _reloadWeaponTimers;

    private IEventSubscription? _watchingForThrowablesSubscription;

    public WeaponsDataWatcher(IIdentityService identityService, IGame game, ILifetimeScope lifetimeScope)
    {
        _identityService = identityService;
        _game = game;
        _thrownGrenades = [];
        _playersWithThrowable = [];
        _droppedWeapons = new Dictionary<int, Weapon>();
        _drawnWeaponTimers = new Dictionary<int, TimerBase>();
        _reloadWeaponTimers = new Dictionary<int, List<ReloadAwaitingData>>();
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnWeaponAdded(OnWeaponAdded, EventHookMode.Default);
        _extendedEvents.HookOnWeaponRemoved(OnWeaponRemoved, EventHookMode.Default);
        _extendedEvents.HookOnProjectilesCreated(OnProjectilesCreated, EventHookMode.Default);
        _extendedEvents.HookOnPlayerMeleeAction(OnMeleeAction, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectDestroyed, EventHookMode.Default);
        _extendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Default);
    }

    private void OnWeaponAdded(Event<IPlayer, PlayerWeaponAddedArg> @event)
    {
        var (playerInstance, args, _) = @event;
        if (!playerInstance.IsValidUser())
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);
        if (args.SourceObjectID != 0 && _droppedWeapons.TryGetValue(args.SourceObjectID, out var droppedWeapon))
        {
            player.WeaponsData.AddWeapon(droppedWeapon);

            droppedWeapon.SetOwner(playerInstance);
            droppedWeapon.RaiseEvent(WeaponEvent.PickedUp);

            Logger.Debug("Picked up dropped weapon: {WeaponItem}, id: {ObjectId}, owner: {Player}",
                droppedWeapon.WeaponItem, args.SourceObjectID, player.Name);
        }
        else
        {
            UpdateWeaponData(player, args.WeaponItemType);
            var pickedUpWeapon = player.WeaponsData.GetWeaponByType(args.WeaponItemType);

            pickedUpWeapon.SetOwner(playerInstance);
            pickedUpWeapon.RaiseEvent(WeaponEvent.PickedUp);

            Logger.Debug("Picked up weapon: {WeaponItem} {ObjectId}, owner: {Player}", args.WeaponItem,
                args.SourceObjectID, player.Name);
        }

        OnThrowablePickedUp(args, player);
    }

    private void OnThrowablePickedUp(PlayerWeaponAddedArg args, Player player)
    {
        if (args.WeaponItemType == WeaponItemType.Thrown)
        {
            if (!_playersWithThrowable.Contains(player))
            {
                _playersWithThrowable.Add(player);
            }

            _watchingForThrowablesSubscription ??= _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
        }
    }

    private void OnWeaponRemoved(Event<IPlayer, PlayerWeaponRemovedArg> @event)
    {
        var (playerInstance, args, _) = @event;
        if (!playerInstance.IsValidUser())
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);
        var weaponsData = player.WeaponsData;
        var removedWeapon = weaponsData.GetWeaponByType(args.WeaponItemType);

        if (args.TargetObjectID != 0)
        {
            var @object = _game.GetObject(args.TargetObjectID);
            if (args.Thrown)
            {
                var isGrenadeThrown = @object is not IObjectWeaponItem;

                if (isGrenadeThrown && removedWeapon is Grenade grenade && @object is IObjectGrenadeThrown)
                {
                    OnGrenadeThrown(grenade, args, player);
                }

                if (!isGrenadeThrown)
                {
                    OnWeaponDropped(removedWeapon, args, @object, player, WeaponEvent.Thrown);
                }
            }
            else if (args.Dropped)
            {
                OnWeaponDropped(removedWeapon, args, @object, player, WeaponEvent.Dropped);
            }
        }

        UpdateWeaponData(player, args.WeaponItemType);

        if (player.WeaponsData.ThrowableItem.IsInvalid)
        {
            _playersWithThrowable.Remove(player);
        }
    }

    private void OnWeaponDropped(Weapon removedWeapon, PlayerWeaponRemovedArg args, IObject @object,
        Player player, WeaponEvent weaponEvent)
    {
        removedWeapon.SetObject(args.TargetObjectID);
        removedWeapon.RaiseEvent(weaponEvent);

        _droppedWeapons.Add(args.TargetObjectID, removedWeapon);

        Logger.Debug("Weapon dropped: {WeaponItem} id {ObjectId} missile {IsMissile}, owner: {Player}",
            removedWeapon.WeaponItem, args.TargetObjectID, @object.IsMissile, player.Name);
    }

    private void OnGrenadeThrown(Grenade grenade, PlayerWeaponRemovedArg args, Player player)
    {
        var thrownGrenade = new Grenade(grenade.WeaponItem, grenade.WeaponItemType, 1, grenade.MaxAmmo,
            false, grenade.TimeToExplosion);
        thrownGrenade.SetObject(args.TargetObjectID);
        thrownGrenade.RaiseEvent(WeaponEvent.GrenadeThrown);
        _thrownGrenades.Add(thrownGrenade);

        Logger.Debug("grenade thrown id: {ObjectId}, owner: {Player}, timer: {TimeLeft}", args.TargetObjectID,
            player.Name, grenade.TimeToExplosion);
    }

    private void OnProjectilesCreated(Event<IProjectile[]> @event)
    {
        var handledWeapons = new Dictionary<int, List<ProjectileItem>>();

        foreach (var projectile in @event.Args)
        {
            var playerId = projectile.InitialOwnerPlayerID;
            var playerInstance = _game.GetPlayer(playerId);
            if (playerInstance?.IsValidUser() != true)
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
            var currentDrawn = weaponsData.CurrentWeaponDrawn;

            UpdateWeaponData(player, currentDrawn.WeaponItemType);
            currentDrawn.RaiseEvent(WeaponEvent.Fired);

            Logger.Debug("Shoot with weapon {WeaponItem}, player: {Player}, ammo left: {AmmoLeft}",
                projectile.ProjectileItem, player.Name, (currentDrawn as Firearm)!.CurrentAmmo);
        }
    }

    private void OnMeleeAction(Event<IPlayer, PlayerMeleeHitArg[]> @event)
    {
        var (playerInstance, args, _) = @event;
        if (playerInstance?.IsValidUser() != true)
            return;

        foreach (var meleeHit in args)
        {
            if (playerInstance.CurrentWeaponDrawn == WeaponItemType.Melee)
            {
                var player = _identityService.GetPlayerByInstance(playerInstance);
                var weaponsData = player.WeaponsData;
                var currentDrawn = weaponsData.CurrentWeaponDrawn;

                UpdateWeaponData(player, WeaponItemType.Melee, currentDrawn is MeleeTemp);
                currentDrawn.RaiseEvent(WeaponEvent.MeleeHit);

                Logger.Debug("Melee hit with {WeaponItem}, player: {Player}, durability left: {DurabilityLeft}",
                    currentDrawn.WeaponItem, player.Name, ((Melee)currentDrawn).CurrentDurability);
            }
        }
    }

    private void OnUpdate(Event<float> @event)
    {
        UpdatePlayersThrowable();
        UpdateThrownGrenades();

        if (_thrownGrenades.Count == 0 && _playersWithThrowable.Count == 0)
        {
            _watchingForThrowablesSubscription!.Dispose();
            _watchingForThrowablesSubscription = null;

            Logger.Debug("Dispose throwables subscription");
        }
    }

    private void UpdateThrownGrenades()
    {
        for (var i = _thrownGrenades.Count - 1; i >= 0; i--)
        {
            var grenade = _thrownGrenades[i];
            var grenadeObject = (IObjectGrenadeThrown)_game.GetObject(grenade.ObjectId);
            if (!grenadeObject.IsValid())
            {
                _thrownGrenades.RemoveAt(i);
                continue;
            }

            grenade.Update(false, grenadeObject.GetExplosionTimer());

            Logger.Debug("Grenade update: {WeaponItem}, id: {ObjectId}, timer: {TimeLeft}", grenade.WeaponItem,
                grenadeObject.UniqueId, grenade.TimeToExplosion);
        }
    }

    private void UpdatePlayersThrowable()
    {
        var playersWithHoldingGrenades = _playersWithThrowable.Where(x =>
            x is { Instance.IsHoldingActiveThrowable: true } or
                { Instance.IsHoldingActiveThrowable: false, WeaponsData.ThrowableItem.IsActive: true });

        foreach (var player in playersWithHoldingGrenades)
        {
            var throwable = player.WeaponsData.ThrowableItem;
            var playerInstance = player.Instance!;

            if (throwable is Grenade grenade)
            {
                grenade.Update(playerInstance.IsHoldingActiveThrowable, playerInstance.GetActiveThrowableTimer());
            }
            else if (throwable.IsActive != playerInstance.IsHoldingActiveThrowable)
            {
                UpdateWeaponData(player, WeaponItemType.Thrown);
            }

            Logger.Debug("Throwable update: {WeaponItem}, player: {Player}, isHolding: {IsHolding}",
                throwable.WeaponItem, player.Name, playerInstance.IsHoldingActiveThrowable);
        }
    }

    private void OnObjectDestroyed(Event<IObject[]> @event)
    {
        foreach (var @object in @event.Args)
        {
            if (_droppedWeapons.ContainsKey(@object.UniqueId))
            {
                Awaiter.Start(() => _droppedWeapons.Remove(@object.UniqueId), TimeSpan.Zero);
            }
        }
    }

    private void OnKeyInput(Event<IPlayer, VirtualKeyInfo[]> @event)
    {
        var (playerInstance, keyInputs, _) = @event;
        foreach (var keyInput in keyInputs)
        {
            if (keyInput is not
                {
                    Event: VirtualKeyEvent.Pressed,
                    Key: VirtualKey.DRAW_MELEE or VirtualKey.DRAW_HANDGUN or VirtualKey.DRAW_RIFLE
                    or VirtualKey.DRAW_GRENADE or VirtualKey.DRAW_SPECIAL or VirtualKey.SHEATHE
                    or VirtualKey.ACTIVATE or VirtualKey.ATTACK
                })
                continue;

            var player = _identityService.GetPlayerByInstance(playerInstance);
            var weaponsData = player.WeaponsData;
            var needUpdateDrawn = (keyInput.Key == VirtualKey.DRAW_MELEE) ||
                                  (keyInput.Key == VirtualKey.DRAW_HANDGUN && !weaponsData.SecondaryWeapon.IsInvalid) ||
                                  (keyInput.Key == VirtualKey.DRAW_RIFLE && !weaponsData.PrimaryWeapon.IsInvalid) ||
                                  (keyInput.Key == VirtualKey.DRAW_GRENADE && !weaponsData.ThrowableItem.IsInvalid) ||
                                  (keyInput.Key == VirtualKey.DRAW_SPECIAL && !weaponsData.PowerupItem.IsInvalid) ||
                                  keyInput.Key == VirtualKey.SHEATHE;

            var weaponItemType = keyInput.Key switch
            {
                VirtualKey.DRAW_MELEE when weaponsData is
                    { MeleeWeapon.IsInvalid: true, MeleeWeaponTemp.IsInvalid: true } => WeaponItemType.NONE,
                VirtualKey.DRAW_MELEE => WeaponItemType.Melee,
                VirtualKey.DRAW_HANDGUN => WeaponItemType.Handgun,
                VirtualKey.DRAW_RIFLE => WeaponItemType.Rifle,
                VirtualKey.DRAW_GRENADE => WeaponItemType.Thrown,
                VirtualKey.DRAW_SPECIAL => WeaponItemType.Powerup,
                VirtualKey.SHEATHE => WeaponItemType.NONE,
                _ => WeaponItemType.NONE
            };

            if (needUpdateDrawn)
            {
                UpdateDrawnWhileNotChanged(player, weaponItemType);
            }

            if (keyInput.Key == VirtualKey.ACTIVATE && weaponsData.HasAnyFirearm())
            {
                // pickup ammo (Helipad for example)
                UpdateFirearms(player);
            }

            if (playerInstance.IsReloading || keyInput.Key == VirtualKey.ATTACK &&
                weaponsData.CurrentWeaponDrawn is Firearm { CurrentSpareMags: > 0, CurrentAmmo: 0 })
            {
                UpdateWhileReloadNotCompletedOrInterrupted(player, weaponsData.CurrentWeaponDrawn.WeaponItemType);
            }
        }
    }

    private void UpdateFirearms(Player player)
    {
        var weaponsData = player.WeaponsData;
        weaponsData.UpdateFirearms();

        Logger.Debug(
            "Updated firearms weapons for player: {Player}, new handgun: {HandgunItem} {HandgunAmmo}, new rifle: {RifleItem} {RifleAmmo}",
            player.Name, weaponsData.SecondaryWeapon.WeaponItem, weaponsData.SecondaryWeapon.CurrentAmmo,
            weaponsData.PrimaryWeapon.WeaponItem, weaponsData.PrimaryWeapon.CurrentAmmo);
    }

    private void UpdateDrawnWhileNotChanged(Player player, WeaponItemType weaponItemType)
    {
        var playerInstance = player.Instance!;

        if (_drawnWeaponTimers.TryGetValue(playerInstance.UniqueId, out var existingTimer))
        {
            existingTimer.Stop();
            _drawnWeaponTimers.Remove(playerInstance.UniqueId);
        }

        var weaponsData = player.WeaponsData;
        if (weaponsData.CurrentWeaponDrawn.WeaponItemType == weaponItemType)
            return;

        var args = new AwaitingDrawnChangeTimerArgs(player, weaponItemType);
        var timer = new PeriodicTimer<AwaitingDrawnChangeTimerArgs>(TimeSpan.Zero, TimeBehavior.TimeModifier, Callback,
            FinishCondition, FinishCallback, args, _extendedEvents);
        timer.Start();

        _drawnWeaponTimers.Add(playerInstance.UniqueId, timer);

        static void Callback(AwaitingDrawnChangeTimerArgs args)
        {
            var player = args.Player;
            var playerInstance = player.Instance!;
            var weaponsData = player.WeaponsData;

            weaponsData.UpdateDrawn();

            Logger.Debug(
                "Updated drawn weapon for player: {Player}, new drawn: {WeaponItem}, drawn: {DrawnItemType}, wainting for {WeaponItemType}",
                player.Name, weaponsData.CurrentWeaponDrawn.WeaponItem, playerInstance.CurrentWeaponDrawn,
                args.WeaponItemType);
        }

        static bool FinishCondition(AwaitingDrawnChangeTimerArgs args)
        {
            var playerInstance = args.Player.Instance;
            var weaponsData = args.Player.WeaponsData;

            return playerInstance?.IsValid() != true || playerInstance.CurrentWeaponDrawn == args.WeaponItemType &&
                weaponsData.CurrentWeaponDrawn.WeaponItemType == args.WeaponItemType;
        }

        static void FinishCallback(AwaitingDrawnChangeTimerArgs args)
        {
            if (args.WeaponItemType == WeaponItemType.NONE)
                return;

            var player = args.Player;
            var playerInstance = player.Instance;
            if (playerInstance?.IsValid() != true)
                return;

            var weaponsData = player.WeaponsData;
            var currentDrawn = weaponsData.CurrentWeaponDrawn;
            currentDrawn.RaiseEvent(WeaponEvent.Drawn);
        }
    }

    private void UpdateWhileReloadNotCompletedOrInterrupted(Player player, WeaponItemType weaponItemType)
    {
        var playerInstance = player.Instance!;
        var weaponsData = player.WeaponsData;
        var savedWeapon = weaponsData.CurrentWeaponDrawn with { } as Firearm;

        if (_reloadWeaponTimers.TryGetValue(playerInstance.UniqueId, out var awaitingReloads))
        {
            if (awaitingReloads.Any(x => x.WeaponItemType == weaponItemType))
                return;
        }
        else
        {
            awaitingReloads = [];
            _reloadWeaponTimers.Add(playerInstance.UniqueId, awaitingReloads);
        }

        Logger.Debug("Start watching reloading for player: {Player}, waiting for changing {WeaponItemType}",
            player.Name, weaponItemType);

        var args = new AwaitingWeaponChangeTimerArgs(player, weaponItemType, savedWeapon!, awaitingReloads);
        var timer = new PeriodicTimer<AwaitingWeaponChangeTimerArgs>(TimeSpan.Zero, TimeBehavior.TimeModifier, Callback,
            FinishCondition, FinishCallback, args, _extendedEvents);
        timer.Start();

        awaitingReloads.Add(new ReloadAwaitingData(timer, weaponItemType));

        static void Callback(AwaitingWeaponChangeTimerArgs args)
        {
            var player = args.Player;
            UpdateWeaponData(player, args.WeaponItemType);

            Logger.Debug("Updated weapon for player: {Player}, waiting for changing {WeaponItemType}", player.Name,
                args.WeaponItemType);
        }

        static bool FinishCondition(AwaitingWeaponChangeTimerArgs args)
        {
            var player = args.Player;
            var playerInstance = player.Instance;
            var weaponsData = player.WeaponsData;

            return playerInstance?.IsValid() != true ||
                   weaponsData.CurrentWeaponDrawn.WeaponItemType != args.WeaponItemType ||
                   (weaponsData.GetWeaponByType(args.WeaponItemType) as Firearm)!.CurrentSpareMags !=
                   args.SavedWeapon.CurrentSpareMags;
        }

        static void FinishCallback(AwaitingWeaponChangeTimerArgs args)
        {
            var reloadsData = args.ReloadsData;
            reloadsData.RemoveAll(x => x.WeaponItemType == args.WeaponItemType);
        }
    }

    private static void UpdateWeaponData(Player player, WeaponItemType weaponItemType, bool isMakeshift = false)
    {
        var weaponsData = new UnsafeWeaponsData(player.Instance!);
        player.WeaponsData.Update(weaponsData, weaponItemType, isMakeshift);
    }

    private record AwaitingDrawnChangeTimerArgs(Player Player, WeaponItemType WeaponItemType);

    private record AwaitingWeaponChangeTimerArgs(
        Player Player,
        WeaponItemType WeaponItemType,
        Firearm SavedWeapon,
        List<ReloadAwaitingData> ReloadsData);

    private record ReloadAwaitingData(TimerBase Timer, WeaponItemType WeaponItemType);
}