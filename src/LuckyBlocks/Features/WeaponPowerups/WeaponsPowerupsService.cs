using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Keyboard;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups;

internal interface IWeaponPowerupsService
{
    void InitializePlayer(Player player);
    bool CanAddWeaponPowerup(IPlayer player, Type powerupType);
    OneOf<NotFound, IEnumerable<Weapon>> TryGetWeaponsForPowerup(IPlayer player, Type powerupType);
    void AddWeaponPowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon);
    void RemovePowerup(IWeaponPowerup<Weapon> powerup, Weapon weapon);
    void ConcatPowerups(Weapon existingWeapon, IEnumerable<IWeaponPowerup<Weapon>> powerupsToConcat);
    WeaponsData CreateWeaponsDataCopy(Player player);
    void RestoreWeaponsDataFromCopy(Player player, WeaponsData copiedWeaponsData, bool restorePowerups = true);
    IEnumerable<IWeaponPowerup<Weapon>> CreateWeaponPowerupsCopy(Weapon weapon);
}

internal class WeaponPowerupsService : IWeaponPowerupsService
{
    private static TimeSpan ShowPowerupsMessageCooldown => TimeSpan.FromSeconds(3);

    private readonly IIdentityService _identityService;
    private readonly IKeyboardService _keyboardService;
    private readonly INotificationService _notificationService;
    private readonly ILogger _logger;
    private readonly Dictionary<Player, IKeyboardEventSubscription> _showPowerupsSubscriptions = new();

    public WeaponPowerupsService(IIdentityService identityService, IKeyboardService keyboardService,
        INotificationService notificationService, ILogger logger)
    {
        _identityService = identityService;
        _keyboardService = keyboardService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public void InitializePlayer(Player player)
    {
        var keyboard = _keyboardService.ResolveForPlayer(player);
        var subscription = keyboard.HookPress([VirtualKey.SPRINT, VirtualKey.WALKING], () => ShowPlayerPowerups(player),
            ShowPowerupsMessageCooldown);
        _showPowerupsSubscriptions.Add(player, subscription);
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

        var isFirstPowerup = !weapon.Powerups.Any();

        if (powerup is IUsablePowerup<Weapon> usablePowerup)
        {
            if (weapon.IsDropped)
            {
                if (isFirstPowerup)
                {
                    weapon.PickUp += EnsureWeaponHasEnoughAmmoForPowerups;
                }
            }
            else
            {
                EnsureWeaponHasEnoughAmmoForPowerup(weapon, usablePowerup, weapon.Owner);
            }
        }

        if (isFirstPowerup)
        {
            weapon.Dispose += OnWeaponDisposed;

            if (weapon is Firearm firearm)
            {
                firearm.Reload += EnsureWeaponHasEnoughAmmoForPowerups;
            }
        }

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
        var isLastPowerup = weapon.Powerups.Count() == 1;

        weapon.RemovePowerup(powerup);

        if (isLastPowerup)
        {
            weapon.PickUp -= EnsureWeaponHasEnoughAmmoForPowerups;
            weapon.Dispose -= OnWeaponDisposed;

            if (weapon is Firearm firearm)
            {
                firearm.Reload -= EnsureWeaponHasEnoughAmmoForPowerups;
            }
        }

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

    public IEnumerable<IWeaponPowerup<Weapon>> CreateWeaponPowerupsCopy(Weapon weapon)
    {
        foreach (var powerup in weapon.Powerups)
        {
            var copiedPowerup = powerup.Clone(weapon);
            yield return copiedPowerup;
        }
    }

    public void RestoreWeaponsDataFromCopy(Player player, WeaponsData copiedWeaponsData, bool restorePowerups = true)
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

                var powerups = restorePowerups ? weapon.Powerups : weapon.Powerups.ToList();
                foreach (var powerup in powerups)
                {
                    if (restorePowerups)
                    {
                        powerup.Run();
                        _logger.Debug("Run restored powerup {PowerupName}", powerup.Name);
                    }
                    else
                    {
                        RemovePowerup(powerup, weapon);
                        _logger.Debug("Remove restored powerup {PowerupName}", powerup.Name);
                    }
                }
            }
        }, 2);

        // in sfd sometimes you cant determinate reason of weapon removal
        // was it because its melee and its broken
        // was it truncating ammo of thrown
        // was it script removal
        // was it because god dont like you
        // so i decided to firstly remove all weapons
        // wait when all events fired and then set new weapons
        // so only i can restore weapons data copy only in 2 ticks
        // => only in third tick i can guarantee that there's correct weapons data
        // hope i wont shoot in my leg
        // but i already delay magic restoring for 3 ticks
        // funny isnt it? delay need for StealWizard
    }

    private void ShowPlayerPowerups(Player player)
    {
        var poweruppedWeapons = player.WeaponsData
            .Where(x => x.Powerups.Count(y => !y.IsHidden) > 0)
            .ToList();

        if (poweruppedWeapons.Count == 0)
        {
            _notificationService.CreateChatNotification("You have no powerupped weapons", Color.White,
                player.UserIdentifier);
            return;
        }

        _notificationService.CreateChatNotification("Your powerupped weapons:", Color.White, player.UserIdentifier);
        foreach (var poweruppedWeapon in poweruppedWeapons)
        {
            var message = $"{poweruppedWeapon.WeaponItem}: [{string.Join(", ", poweruppedWeapon.Powerups
                .Where(x => !x.IsHidden)
                .Select(y => y switch
                {
                    IUsablePowerup<Weapon> usablePowerup => $"{usablePowerup.Name} ({usablePowerup.UsesLeft})",
                    _ => y.Name
                }))}]";
            _notificationService.CreateChatNotification(message, Color.White, player.UserIdentifier);
        }

        // var message = poweruppedWeapons.Count switch
        // {
        //     0 => "You have no powerupped weapons",
        //     _ =>
        //         $"Your powerupped weapons: [{string.Join(", ", poweruppedWeapons.Select(x => $"{x.WeaponItem}: [{string.Join(", ", x.Powerups
        //             .Where(y => !y.IsHidden)
        //             .Select(z => z switch
        //             {
        //                 IUsablePowerup<Weapon> usablePowerup => $"{usablePowerup.Name} ({usablePowerup.UsesLeft})",
        //                 _ => z.Name
        //             }))}]"))}]"
        // };
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
                var playerInstance = weapon.Owner;
                if (playerInstance?.IsValid() == true)
                {
                    EnsureWeaponHasEnoughAmmoForPowerups(weapon, playerInstance);
                }

                _logger.Debug("Powerup {PowerupName} uses increased for {WeaponItem} (owner {Player})", powerup.Name,
                    weapon.WeaponItem, playerInstance?.Name);
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

    private void EnsureWeaponHasEnoughAmmoForPowerup(Weapon weapon, IUsablePowerup<Weapon> powerup,
        IPlayer playerInstance)
    {
        var minAmmo = powerup.UsesLeft;
        EnsureWeaponHasEnoughAmmo(weapon, playerInstance, minAmmo);
    }

    private void EnsureWeaponHasEnoughAmmoForPowerups(Weapon weapon, IPlayer playerInstance)
    {
        var minAmmo = weapon.Powerups
            .Where(x => x is IUsablePowerup<Weapon>)
            .Cast<IUsablePowerup<Weapon>>()
            .Select(x => x.UsesLeft)
            .OrderBy(x => x)
            .LastOrDefault();

        if (minAmmo == 0)
            return;

        EnsureWeaponHasEnoughAmmo(weapon, playerInstance, minAmmo);
    }

    private void EnsureWeaponHasEnoughAmmo(Weapon weapon, IPlayer playerInstance, int minAmmo)
    {
        switch (weapon)
        {
            case Firearm firearm when firearm.TotalAmmo < minAmmo:
                playerInstance.SetAmmo(firearm, minAmmo);
                break;
            case Throwable throwableItem when throwableItem.CurrentAmmo < minAmmo:
                playerInstance.SetAmmo(throwableItem, minAmmo);
                break;
        }
    }
}