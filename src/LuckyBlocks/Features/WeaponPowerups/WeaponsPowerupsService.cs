using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Utils;
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
    WeaponsData CreateWeaponsDataCopy(Player player);
    void RestoreWeaponsDataFromCopy(Player player, WeaponsData copiedWeaponsData);
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

    public void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon) =>
        AddWeaponPowerup(powerup, weapon, true);

    public void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon, bool run)
    {
        if (TryAddPowerupAgain(powerup, weapon))
            return;

        if (weapon.IsDropped)
        {
            weapon.PickUp += EnsureWeaponHasEnoughAmmoForPowerups;
        }
        else
        {
            EnsureWeaponHasEnoughAmmoForPowerups(weapon);
        }

        weapon.Dispose += OnWeaponDisposed;
        weapon.AddPowerup(powerup);

        if (run)
        {
            powerup.Run();
        }

#if DEBUG
        _logger.Debug("Powerup {PowerupName} added to {WeaponItem} (owner {Player}, copied: {Copied})", powerup.Name,
            weapon.WeaponItem, weapon.Owner?.Name, weapon.Copied);
#endif
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
                RemovePowerup(powerup, powerup.Weapon);
                continue;
            }

            MovePowerup(existingWeapon, powerup);
        }
    }

    public WeaponsData CreateWeaponsDataCopy(Player player)
    {
        var playerInstance = player.Instance!;
        var weaponsData = player.WeaponsData;
        var weaponsDataCopy = playerInstance.CreateWeaponsData();
        weaponsDataCopy.SetCopied();

        foreach (var weapon in weaponsData)
        {
            foreach (var powerup in weapon.Powerups)
            {
                var copiedWeapon = weaponsDataCopy.GetWeaponByType(weapon.WeaponItemType, weapon is MeleeTemp);
                var copiedPowerup = powerup.Clone(copiedWeapon);
                AddWeaponPowerup(copiedPowerup, copiedWeapon, false);
            }
        }

        return weaponsDataCopy;
    }

    public void RestoreWeaponsDataFromCopy(Player player, WeaponsData copiedWeaponsData)
    {
        var playerInstance = player.Instance!;
        var weaponsData = player.WeaponsData;

        weaponsData.Dispose();
        playerInstance.RemoveAllWeapons();

        Awaiter.Start(delegate
        {
            player.SetWeapons(copiedWeaponsData, true);

            foreach (var weapon in copiedWeaponsData)
            {
                weapon.SetOwner(playerInstance);

                foreach (var powerup in weapon.Powerups)
                {
                    powerup.Run();

                    _logger.Debug("Run restored powerup {PowerupName}", powerup.Name);
                }
            }
        }, TimeSpan.Zero);
    }

    private bool TryAddPowerupAgain(IWeaponPowerup<Weapon> powerup, Weapon weapon)
    {
        if (powerup is not IStackablePowerup<Weapon> stackablePowerup)
            return false;

        var existingPowerupSameType = weapon.Powerups.FirstOrDefault(x => x.GetType() == powerup.GetType());
        if (existingPowerupSameType is IStackablePowerup<Weapon> existingPowerup)
        {
            existingPowerup.Stack(stackablePowerup);

            if (existingPowerup is IUsablePowerup<Weapon>)
            {
                EnsureWeaponHasEnoughAmmoForPowerups(weapon);

                _logger.Debug("Powerup {PowerupName} uses increased for {WeaponItem} (owner {Player})", powerup.Name,
                    weapon.WeaponItem, weapon.Owner?.Name);
            }

            _logger.Debug("Powerup {PowerupName} stacked on {WeaponItem} (owner {Player})", powerup.Name,
                weapon.WeaponItem, weapon.Owner?.Name);

            return true;
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
        if (owner is null)
        {
            _logger.Warning(
                "calling EnsureWeaponHasEnoughAmmoForPowerups when weapon's owner is null, skip. Weapon: {Weapon}",
                weapon);
            return;
        }

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