using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Bullets;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups;

internal interface IWeaponsPowerupsService
{
    bool CanAddWeaponPowerup(IPlayer player, Type powerupType, IEnumerable<Type> incompatiblePowerups);

    OneOf<Firearm, Throwable, NotFound> TryGetWeaponForPowerup(IPlayer player, Type powerupType,
        IEnumerable<Type> incompatiblePowerupTypes);

    void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon, IPlayer player);
    void RemovePowerup(IWeaponPowerup<Weapon> powerup, Weapon? weapon);
    void SetOwner(Weapon weapon, IPlayer newOwner, IPlayer previousOwner);
}

internal class WeaponsPowerupsService : IWeaponsPowerupsService
{
    private readonly ILogger _logger;
    private readonly List<PowerupWrapper> _powerups;

    public WeaponsPowerupsService(ILogger logger)
        => (_logger, _powerups) = (logger, new());

    public void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon, IPlayer player)
    {
        InvalidateWeapons(player);

        var appliedAgain = ApplyPowerupAgainIfExists(powerup, weapon, player);
        if (appliedAgain)
            return;
        
        if (powerup is ILimitedAmmoPowerup<Weapon> limitedAmmoPowerup)
        {
            TruncateAmmo(weapon, player, limitedAmmoPowerup.MaxAmmo);
        }

        var watcher = CreateWatcherAndHookEventsForPowerup(powerup, weapon, player);
        var weaponPowerup = new PowerupWrapper(powerup, watcher);

        watcher.Start();
        powerup.OnRan(player);
        _powerups.Add(weaponPowerup);

        _logger.Debug("Powerup {PowerupName} added to {WeaponItem} (owner {PlayerName})", powerup.Name,
            weapon.WeaponItem, player.Name);
    }

    public bool CanAddWeaponPowerup(IPlayer player, Type powerupType, IEnumerable<Type> incompatiblePowerupTypes)
    {
        var weapon = TryGetWeaponForPowerup(player, powerupType, incompatiblePowerupTypes);
        return weapon.IsT0 || weapon.IsT1;
    }

    public void RemovePowerup(IWeaponPowerup<Weapon> powerup, Weapon? weapon)
    {
        var getPowerupResult = GetAppliedPowerup(powerup, weapon);
        if (!getPowerupResult.TryPickT0(out var weaponPowerup, out _))
            return;

        var weaponEventsWatcher = weaponPowerup.WeaponEventsWatcher;
        weaponEventsWatcher.Dispose();

        _powerups.Remove(weaponPowerup);

        _logger.Debug("Powerup {PowerupName} removed from {WeaponItem} (owner {OwnerName})", powerup.Name,
            weapon?.WeaponItem ?? WeaponItem.NONE, weaponEventsWatcher.Owner?.Name);
    }

    public OneOf<Firearm, Throwable, NotFound> TryGetWeaponForPowerup(IPlayer player, Type powerupType,
        IEnumerable<Type> incompatiblePowerupTypes)
    {
        player.GetUnsafeWeaponsData(out var weaponsData);

        if (typeof(IFirearmPowerup).IsAssignableFrom(powerupType))
        {
            if (!weaponsData.HasAnyFirearm())
                return new NotFound();

            var getFirearmForPowerupResult = TryGetFirearmForPowerup(player, incompatiblePowerupTypes);
            if (getFirearmForPowerupResult.TryPickT0(out var firearm, out _))
                return firearm;
        }

        if (typeof(IThrowableItemPowerup<Throwable>).IsAssignableFrom(powerupType))
        {
            if (!weaponsData.HasThrowableItem())
                return new NotFound();

            var getThrowableForPowerupResult = TryGetThrowableForPowerup(player, incompatiblePowerupTypes);
            if (getThrowableForPowerupResult.TryPickT0(out var throwable, out _))
                return throwable;
        }

        return new NotFound();
    }

    public void SetOwner(Weapon weapon, IPlayer newOwner, IPlayer previousOwner)
    {
        if (weapon is not (Firearm or Throwable))
            return;

        var powerup =
            _powerups.FirstOrDefault(x => x.Powerup.Weapon == weapon && x.WeaponEventsWatcher.Owner == previousOwner);

        if (powerup is null)
            return;

        var weaponEventsWatcher = powerup.WeaponEventsWatcher;
        weaponEventsWatcher.SetOwner(newOwner);
    }

    public void InvalidateWeapons(IPlayer player)
    {
        foreach (var powerupItem in _powerups)
        {
            var powerup = powerupItem.Powerup;
            var owner = powerupItem.WeaponEventsWatcher.Owner;
            
            if (owner == player && powerup is IUsablePowerup<Weapon> usablePowerup)
            {
                usablePowerup.InvalidateWeapon(player);
            }
        }
    }

    private OneOf<Firearm, NotFound> TryGetFirearmForPowerup(IPlayer player, IEnumerable<Type> incompatiblePowerupTypes)
    {
        var firearms = player.GetWeaponsData().GetFirearms();

        var powerUppedFirearms = _powerups.Where(x => firearms.Any(y => y == x.Powerup.Weapon));

        var incompatibleFirearms = powerUppedFirearms
            .Where(x => incompatiblePowerupTypes.Contains(x.Powerup.GetType()))
            .Select(x => x.Powerup.Weapon);

        var compatibleFirearms = firearms
            .Where(x => !incompatibleFirearms.Contains(x))
            .ToList();

        if (compatibleFirearms.Count == 0)
            return new NotFound();

        return compatibleFirearms.GetRandomElement();
    }

    private OneOf<Throwable, NotFound> TryGetThrowableForPowerup(IPlayer player,
        IEnumerable<Type> incompatiblePowerupTypes)
    {
        var weaponsData = player.GetWeaponsData();

        var throwableItem = weaponsData.ThrowableItem;
        var powerUppedThrowableItem = _powerups.FirstOrDefault(x => x.Powerup.Weapon == throwableItem);
        if (powerUppedThrowableItem is null)
            return throwableItem;

        if (incompatiblePowerupTypes.Contains(powerUppedThrowableItem.Powerup.GetType()))
            return new NotFound();

        return throwableItem;
    }

    private bool ApplyPowerupAgainIfExists(IWeaponPowerup<Weapon> powerup, Weapon weapon, IPlayer player)
    {
        if (powerup is not IUsablePowerup<Weapon> usablePowerup)
            return false;

        _logger.Debug("Powerup is usable, can be applied again: {PowerupName}", powerup.Name);

        var usesLeft = usablePowerup.UsesLeft;

        var getExistingWeaponPowerupResult = GetAppliedPowerup(powerup, weapon);
        if (getExistingWeaponPowerupResult.TryPickT0(out var existingWeaponPowerup, out _))
        {
            var existingPowerup = (existingWeaponPowerup.Powerup as IUsablePowerup<Weapon>)!;
            var currentOwner = existingWeaponPowerup.WeaponEventsWatcher.Owner;

            existingPowerup.ApplyAgain(currentOwner);
            usesLeft = existingPowerup.UsesLeft;

            _logger.Debug("Powerup {PowerupName} applied again for {PlayerName}", powerup.Name, player.Name);

            if (existingPowerup is ILimitedAmmoPowerup<Weapon> limitedAmmoPowerup)
            {
                TruncateAmmo(weapon, player, limitedAmmoPowerup.MaxAmmo);
            }
        }

        EnsureThatWeaponHasEnoughAmmoAndGiveIfNot(weapon, player, usesLeft);

        return existingWeaponPowerup is not null;
    }

    private void EnsureThatWeaponHasEnoughAmmoAndGiveIfNot(Weapon weapon, IPlayer player, int minAmmo)
    {
        switch (weapon)
        {
            case Firearm firearm when firearm.TotalAmmo < minAmmo:
                player.SetAmmo(firearm, minAmmo);
                break;
            case Throwable throwableItem when throwableItem.CurrentAmmo < minAmmo:
                player.SetAmmo(throwableItem, minAmmo);
                break;
        }
    }

    // triggers WeaponRemoved event on next tick for grenades
    private void TruncateAmmo(Weapon weapon, IPlayer player, int maxAmmo)
    {
        switch (weapon)
        {
            case Firearm firearm when firearm.TotalAmmo > maxAmmo:
                player.SetAmmo(firearm, maxAmmo);
                break;
            case Throwable throwableItem when throwableItem.CurrentAmmo > maxAmmo:
                player.SetAmmo(throwableItem, maxAmmo);
                break;
        }
    }

    private WeaponEventsWatcher CreateWatcherAndHookEventsForPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon,
        IPlayer player)
    {
        var watcher = WeaponEventsWatcher.CreateForWeapon(weapon.WeaponItem, weapon.WeaponItemType, player);
        watcher.Pickup += powerup.OnWeaponPickedUp;
        watcher.Drop += powerup.OnWeaponDropped;

        switch (powerup)
        {
            case IFirearmPowerup firearmPowerup:
                watcher.Fire += firearmPowerup.OnFire;
                break;
            case IThrowableItemPowerup<Throwable> throwableItemPowerup:
                watcher.GrenadeThrow += throwableItemPowerup.OnThrow;
                watcher.MineThrow += throwableItemPowerup.OnThrow;
                break;
        }

        return watcher;
    }

    private OneOf<PowerupWrapper, NotFound> GetAppliedPowerup(IWeaponPowerup<Weapon> powerup, Weapon? weapon)
    {
        var existingPowerup = _powerups.FirstOrDefault(x =>
            x.Powerup.Weapon == weapon && x.Powerup.GetType() == powerup.GetType());

        if (existingPowerup is null)
            return new NotFound();

        return existingPowerup;
    }

    private record PowerupWrapper(IWeaponPowerup<Weapon> Powerup, WeaponEventsWatcher WeaponEventsWatcher);
}