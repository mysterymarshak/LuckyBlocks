using LuckyBlocks.Data;
using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Entities;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

[Inject]
internal static class IPlayerExtensions
{
    [InjectWeaponsMapper]
    private static IWeaponsMapper WeaponsMapper { get; set; }

    public static bool IsValidUser(this IPlayer playerInstance)
    {
        return playerInstance.IsValid() && playerInstance is { IsUser: true, UserIdentifier: > 0 };
    }
    
    public static WeaponsData GetWeaponsData(this IPlayer player)
    {
        return new WeaponsData(player, WeaponsMapper);
    }

    public static void GetUnsafeWeaponsData(this IPlayer player, out UnsafeWeaponsData weaponsData)
    {
        weaponsData = new UnsafeWeaponsData(player);
    }

    public static void SetWeapons(this IPlayer player, in UnsafeWeaponsData weaponsData, bool forceSet = false)
    {
        if (forceSet)
        {
            player.GiveWeaponItem(weaponsData.MeleeWeapon.WeaponItem);
            player.SetCurrentMeleeDurability(weaponsData.MeleeWeapon.CurrentDurability);

            player.GiveWeaponItem(weaponsData.MeleeWeaponTemp.WeaponItem);
            player.SetCurrentMeleeMakeshiftDurability(weaponsData.MeleeWeaponTemp.CurrentDurability);

            player.GiveWeaponItem(weaponsData.PrimaryWeapon.WeaponItem);
            player.SetCurrentPrimaryWeaponAmmo(weaponsData.PrimaryWeapon.CurrentAmmo,
                weaponsData.PrimaryWeapon.CurrentSpareMags,
                weaponsData.PrimaryWeapon.ProjectilePowerupData.ProjectilePowerup);

            player.GiveWeaponItem(weaponsData.SecondaryWeapon.WeaponItem);
            player.SetCurrentSecondaryWeaponAmmo(weaponsData.SecondaryWeapon.CurrentAmmo,
                weaponsData.SecondaryWeapon.CurrentSpareMags,
                weaponsData.SecondaryWeapon.ProjectilePowerupData.ProjectilePowerup);

            player.GiveWeaponItem(weaponsData.PowerupItem.WeaponItem);

            player.GiveWeaponItem(weaponsData.ThrowableItem.WeaponItem);
            player.SetCurrentThrownItemAmmo(weaponsData.ThrowableItem.CurrentAmmo);

            return;
        }

        player.GetUnsafeWeaponsData(out var currentWeaponsData);

        if (currentWeaponsData.MeleeWeapon != weaponsData.MeleeWeapon)
        {
            player.RemoveWeaponItemType(currentWeaponsData.MeleeWeapon.WeaponItemType);
            player.GiveWeaponItem(weaponsData.MeleeWeapon.WeaponItem);
            player.SetCurrentMeleeDurability(weaponsData.MeleeWeapon.CurrentDurability);
        }

        if (currentWeaponsData.MeleeWeaponTemp != weaponsData.MeleeWeaponTemp)
        {
            player.RemoveWeaponItemType(currentWeaponsData.MeleeWeaponTemp.WeaponItemType);
            player.GiveWeaponItem(weaponsData.MeleeWeaponTemp.WeaponItem);
            player.SetCurrentMeleeMakeshiftDurability(weaponsData.MeleeWeaponTemp.CurrentDurability);
        }

        if (currentWeaponsData.PrimaryWeapon != weaponsData.PrimaryWeapon)
        {
            player.RemoveWeaponItemType(currentWeaponsData.PrimaryWeapon.WeaponItemType);
            player.GiveWeaponItem(weaponsData.PrimaryWeapon.WeaponItem);
            player.SetCurrentPrimaryWeaponAmmo(weaponsData.PrimaryWeapon.CurrentAmmo,
                weaponsData.PrimaryWeapon.CurrentSpareMags,
                weaponsData.PrimaryWeapon.ProjectilePowerupData.ProjectilePowerup);
        }

        if (currentWeaponsData.SecondaryWeapon != weaponsData.SecondaryWeapon)
        {
            player.RemoveWeaponItemType(currentWeaponsData.SecondaryWeapon.WeaponItemType);
            player.GiveWeaponItem(weaponsData.SecondaryWeapon.WeaponItem);
            player.SetCurrentSecondaryWeaponAmmo(weaponsData.SecondaryWeapon.CurrentAmmo,
                weaponsData.SecondaryWeapon.CurrentSpareMags,
                weaponsData.SecondaryWeapon.ProjectilePowerupData.ProjectilePowerup);
        }

        if (currentWeaponsData.ThrowableItem != weaponsData.ThrowableItem)
        {
            player.RemoveWeaponItemType(currentWeaponsData.ThrowableItem.WeaponItemType);
            player.GiveWeaponItem(weaponsData.ThrowableItem.WeaponItem);
            player.SetCurrentThrownItemAmmo(weaponsData.ThrowableItem.CurrentAmmo);
        }
        
        if (currentWeaponsData.PowerupItem != weaponsData.PowerupItem)
        {
            player.RemoveWeaponItemType(currentWeaponsData.PowerupItem.WeaponItemType);
            player.GiveWeaponItem(weaponsData.PowerupItem.WeaponItem);
        }
    }

    public static void RemoveAllWeapons(this IPlayer player)
    {
        player.RemoveWeaponItemType(WeaponItemType.Melee);
        player.RemoveWeaponItemType(WeaponItemType.Handgun);
        player.RemoveWeaponItemType(WeaponItemType.Rifle);
        player.RemoveWeaponItemType(WeaponItemType.Thrown);
        player.RemoveWeaponItemType(WeaponItemType.Powerup);
    }

    public static void SetAmmoFromWeapon(this IPlayer playerInstance, Weapon weapon)
    {
        if (weapon is Firearm firearm)
        {
            playerInstance.SetAmmo(firearm, firearm.CurrentAmmo, firearm.CurrentSpareMags);
        }
        else if (weapon is Throwable throwable)
        {
            playerInstance.SetAmmo(throwable, throwable.CurrentAmmo);
        }
    }
    
    public static void SetAmmo(this IPlayer player, Firearm firearm, int ammo, int mags)
    {
        switch (firearm.WeaponItemType)
        {
            case WeaponItemType.Rifle:
                player.SetCurrentPrimaryWeaponAmmo(ammo, mags);
                break;
            case WeaponItemType.Handgun:
                player.SetCurrentSecondaryWeaponAmmo(ammo, mags);
                break;
        }
    }

    public static void SetAmmo(this IPlayer player, Firearm firearm, int totalAmmo)
    {
        switch (firearm.WeaponItemType)
        {
            case WeaponItemType.Rifle:
                player.SetCurrentPrimaryWeaponAmmo(totalAmmo);
                break;
            case WeaponItemType.Handgun:
                player.SetCurrentSecondaryWeaponAmmo(totalAmmo);
                break;
        }
    }

    // triggers WeaponRemoved event on next tick
    public static void SetAmmo(this IPlayer player, Throwable throwable, int ammo)
    {
        player.SetCurrentThrownItemAmmo(ammo);
    }
}