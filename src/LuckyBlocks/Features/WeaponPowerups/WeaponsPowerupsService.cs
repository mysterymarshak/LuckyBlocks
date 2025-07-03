using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.WeaponPowerups;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups;

internal interface IWeaponPowerupsService
{
    bool CanAddWeaponPowerup(IPlayer player, Type powerupType);
    OneOf<NotFound, IEnumerable<Weapon>> TryGetWeaponsForPowerup(IPlayer player, Type powerupType);
    void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon);
    void RemovePowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon);
    void ConcatPowerups(Weapon existingWeapon, IEnumerable<IWeaponPowerup<Weapon>> powerupsToConcat);
}

internal class WeaponPowerupsService : IWeaponPowerupsService
{
    private readonly IIdentityService _identityService;
    private readonly ILogger _logger;

    public WeaponPowerupsService(IIdentityService identityService, ILogger logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    public bool CanAddWeaponPowerup(IPlayer player, Type powerupType)
    {
        var weapons = TryGetWeaponsForPowerup(player, powerupType);
        return weapons.IsT1;
    }

    public OneOf<NotFound, IEnumerable<Weapon>> TryGetWeaponsForPowerup(IPlayer playerInstance, Type powerupType)
    {
        var player = _identityService.GetPlayerByInstance(playerInstance);
        var weaponsData = player.WeaponsData;

        var powerupWeaponType = powerupType
            .GetInterfaces()
            .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IWeaponPowerup<>))
            .GetGenericArguments()[0];

        var weaponsToPowerup = weaponsData
            .GetWeaponsByType(powerupWeaponType)
            .Where(x => x is not Flamethrower)
            .ToList();

        if (weaponsToPowerup.Count == 0)
        {
            return new NotFound();
        }

        var compatibleWeapons = weaponsToPowerup
            .Where(x => x.Powerups.All(y => y.IsCompatibleWith(powerupType)))
            .ToList();

        if (compatibleWeapons.Count > 0)
        {
            return compatibleWeapons;
        }

        return new NotFound();
    }

    public void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon)
    {
        if (TryAddPowerupAgain(powerup, weapon))
            return;

        weapon.AddPowerup(powerup);
        powerup.Run();

        weapon.PickUp += EnsureWeaponHasEnoughAmmoForPowerups;
        weapon.Dispose += OnWeaponDisposed;

        _logger.Debug("Powerup {PowerupName} added to {WeaponItem} (owner {Player})", powerup.Name, weapon.WeaponItem,
            weapon.Owner?.Name);
    }

    public void RemovePowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon)
    {
        weapon.RemovePowerup(powerup);
        weapon.PickUp -= EnsureWeaponHasEnoughAmmoForPowerups;
        weapon.Dispose -= OnWeaponDisposed;
        powerup.Dispose();

        _logger.Debug("Powerup {PowerupName} removed from {WeaponItem} (owner {Player})", powerup.Name,
            weapon.WeaponItem, weapon.Owner?.Name);
    }

    public void ConcatPowerups(Weapon existingWeapon, IEnumerable<IWeaponPowerup<Weapon>> powerupsToConcat)
    {
        foreach (var powerup in powerupsToConcat)
        {
            if (TryAddPowerupAgain(powerup, existingWeapon))
            {
                var usablePowerup = (IUsablePowerup<Weapon>)powerup;
                RemovePowerup(usablePowerup, usablePowerup.Weapon);
                continue;
            }

            MovePowerup(existingWeapon, powerup);
        }
    }

    private bool TryAddPowerupAgain(IWeaponPowerup<Weapon> powerup, Weapon weapon)
    {
        if (powerup is IUsablePowerup<Weapon> usablePowerup)
        {
            var existingPowerupSameType = weapon.Powerups.FirstOrDefault(x => x.GetType() == powerup.GetType());
            if (existingPowerupSameType is IUsablePowerup<Weapon> existingPowerup)
            {
                existingPowerup.AddUses(usablePowerup.UsesLeft);
                EnsureWeaponHasEnoughAmmoForPowerups(weapon);

                _logger.Debug("Powerup {PowerupName} uses increased for {WeaponItem} (owner {Player})", powerup.Name,
                    weapon.WeaponItem, weapon.Owner?.Name);

                return true;
            }
        }

        return false;
    }

    private void MovePowerup(Weapon targetWeapon, IWeaponPowerup<Weapon> powerup)
    {
        RemovePowerup(powerup, powerup.Weapon);
        targetWeapon.AddPowerup(powerup);
        powerup.MoveToWeapon(targetWeapon);

        _logger.Debug("Powerup {PowerupName} moved to {Weapon} (owner {Player})", powerup.Name, targetWeapon,
            targetWeapon.Owner?.Name);
    }

    private void OnWeaponDisposed(Weapon weapon)
    {
        foreach (var powerup in weapon.Powerups.ToList())
        {
            RemovePowerup(powerup, weapon);
        }
    }

    private void EnsureWeaponHasEnoughAmmoForPowerups(Weapon weapon)
    {
        var minAmmo = weapon.Powerups
            .Where(x => x is IUsablePowerup<Weapon>)
            .Cast<IUsablePowerup<Weapon>>()
            .Select(x => x.UsesLeft)
            .OrderBy(x => x)
            .LastOrDefault();

        if (minAmmo == 0)
            return;

        var owner = weapon.Owner;
        ArgumentWasNullException.ThrowIfNull(owner);

        switch (weapon)
        {
            case Firearm firearm when firearm.TotalAmmo < minAmmo:
                owner.SetAmmo(firearm, minAmmo);
                break;
            case Throwable throwableItem when throwableItem.CurrentAmmo < minAmmo:
                owner.SetAmmo(throwableItem, minAmmo);
                break;
        }
    }
}