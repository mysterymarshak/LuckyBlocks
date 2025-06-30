using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Reflection;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

[Inject]
internal class WeaponEventsWatcher
{
    public event Action<IPlayer, IObjectGrenadeThrown>? GrenadeThrow;
    public event Action<IPlayer, IObjectMineThrown>? MineThrow;
    public event Action<IPlayer, IEnumerable<IProjectile>>? Fire;
    public event Action<IPlayer>? Pickup;
    public event Action<IPlayer, IObjectWeaponItem?>? Drop;

    public IPlayer? Owner { get; private set; }

    [InjectGame]
    private static IGame Game { get; set; }

    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly WeaponItem _weaponItem;
    private readonly WeaponItemType _weaponItemType;

    [MemberNotNullWhen(true, nameof(Owner))]
    private bool PickedUp => Owner?.IsValid() == true;

    private IObjectWeaponItem? _weaponObject;
    private Events.PlayerWeaponAddedActionCallback? _playerWeaponPickupCallback;
    private Events.PlayerWeaponRemovedActionCallback? _playerWeaponDropCallback;
    private Events.ProjectileCreatedCallback? _projectileCreatedCallback;
    private Events.ObjectTerminatedCallback? _objectDestroyedCallback;

    private WeaponEventsWatcher(WeaponItem weaponItem, WeaponItemType weaponItemType, IPlayer player)
    {
        _weaponItem = weaponItem;
        _weaponItemType = weaponItemType;
        Owner = player;
    }

    private WeaponEventsWatcher(IObjectWeaponItem weapon)
    {
        _weaponObject = weapon;
        _weaponItem = weapon.WeaponItem;
        _weaponItemType = weapon.WeaponItemType;
    }

    public static WeaponEventsWatcher CreateForWeapon(IObjectWeaponItem weapon)
    {
        return new WeaponEventsWatcher(weapon);
    }

    public static WeaponEventsWatcher CreateForWeapon(WeaponItem weaponItem, WeaponItemType weaponItemType,
        IPlayer player)
    {
        return new WeaponEventsWatcher(weaponItem, weaponItemType, player);
    }

    public void Dispose()
    {
        _playerWeaponDropCallback?.Stop();
        _playerWeaponPickupCallback?.Stop();
        _projectileCreatedCallback?.Stop();
        _objectDestroyedCallback?.Stop();
        GrenadeThrow = null;
        MineThrow = null;
        Fire = null;
        Pickup = null;
        Drop = null;
    }

    public void Start()
    {
        _playerWeaponPickupCallback = Events.PlayerWeaponAddedActionCallback.Start(OnWeaponPickedUp);
        _playerWeaponDropCallback = Events.PlayerWeaponRemovedActionCallback.Start(OnWeaponDropped);
        _projectileCreatedCallback = Events.ProjectileCreatedCallback.Start(OnProjectileCreated);
        _objectDestroyedCallback = Events.ObjectTerminatedCallback.Start(OnObjectDestroyed);
    }

    public void SetOwner(IPlayer player)
    {
        Drop?.Invoke(player, null);

        Owner = player;
        Pickup?.Invoke(player);
    }

    private void OnWeaponPickedUp(IPlayer player, PlayerWeaponAddedArg args)
    {
        try
        {
            if (_weaponObject is not null && args.SourceObjectID == 0)
                return;

            if (_weaponObject is null && args.SourceObjectID != 0)
                return;

            if (args.SourceObjectID != 0 && args.WeaponItem == _weaponItem ||
                args.SourceObjectID == _weaponObject?.UniqueId)
            {
                Owner = player;
                Pickup?.Invoke(player);
            }
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Unexpected exception in WeaponEventsWatcher.OnWeaponPickedUp");
        }
    }

    private void OnWeaponDropped(IPlayer player, PlayerWeaponRemovedArg args)
    {
        try
        {
            if (!PickedUp)
                return;

            Logger.Debug(
                "weapon dropped: {WeaponItem}, Owner was: {Owner}, Player: {Player}, Event: {Event}, ObjID: {ObjectID}",
                args.WeaponItem, Owner.Name, player.Name,
                args.Dropped ? "Dropped" : args.Thrown ? "Thrown" : "Unknown", args.TargetObjectID);
            // truncate grenades ammo -> Event = Thrown with TargetObjectID = 0

            if (Owner != player)
                return;

            if (args.WeaponItem != _weaponItem)
                return;

            if (args.Thrown && _weaponItemType == WeaponItemType.Thrown && args.TargetObjectID == 0)
            {
                Logger.Debug("Thrown ammo was truncated, Owner: {Owner}", Owner.Name);
                // bug bypass
                return;
            }

            var @object = Game.GetObject(args.TargetObjectID);

            switch (@object)
            {
                case IObjectGrenadeThrown grenadeThrown:
                    GrenadeThrow?.Invoke(player, grenadeThrown);
                    return;
                case IObjectMineThrown mineThrown:
                    MineThrow?.Invoke(player, mineThrown);
                    return;
                case IObjectWeaponItem weapon:
                    _weaponObject = weapon;
                    break;
            }

            Owner = null;
            Drop?.Invoke(player, _weaponObject);
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Unexpected exception in WeaponEventsWatcher.OnWeaponDropped");
        }
    }

    private void OnProjectileCreated(IProjectile[] projectiles)
    {
        try
        {
            if (!PickedUp)
                return;

            if (Owner!.CurrentWeaponDrawn != _weaponItemType)
                return;

            var weaponProjectiles = projectiles.Where(x => x.InitialOwnerPlayerID == Owner.UniqueId);
            if (!weaponProjectiles.Any())
                return;

            Fire?.Invoke(Owner, weaponProjectiles);
        }
        catch (Exception exception)
        {
            Logger.Error(exception, "Unexpected exception in WeaponEventsWatcher.OnProjectileCreated");
        }
    }

    private void OnObjectDestroyed(IObject[] objects)
    {
        if (objects.All(x => x.UniqueId != _weaponObject?.UniqueId))
            return;

        Awaiter.Start(DisposeIfNotPickedUp, TimeSpan.Zero);
    }

    private void DisposeIfNotPickedUp()
    {
        if (!PickedUp)
        {
            Dispose();
        }
    }
}