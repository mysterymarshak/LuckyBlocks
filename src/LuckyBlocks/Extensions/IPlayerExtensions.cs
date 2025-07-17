using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Data.Weapons.Unsafe;
using LuckyBlocks.Reflection;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

[Inject]
internal static class IPlayerExtensions
{
    [InjectWeaponsMapper]
    private static IWeaponsMapper WeaponsMapper { get; set; }

    public static bool IsFake(this IPlayer playerInstance)
    {
        return playerInstance is { UserIdentifier: 0, IsBot: true };
    }

    public static bool IsValidUser(this IPlayer playerInstance)
    {
        return playerInstance.IsValid() && playerInstance is { IsUser: true, UserIdentifier: > 0 };
    }

    public static WeaponsData CreateWeaponsData(this IPlayer playerInstance)
    {
        var weaponsData = new WeaponsData(playerInstance, WeaponsMapper);
        using var enumerator = weaponsData.GetEnumerator(true);
        while (enumerator.MoveNext())
        {
            var weapon = enumerator.Current;
            if (weapon is { WeaponItem: not WeaponItem.NONE, WeaponItemType: not WeaponItemType.NONE, Owner: null })
            {
                weapon.SetOwner(playerInstance);
            }
        }

        return weaponsData;
    }

    public static void GetUnsafeWeaponsData(this IPlayer player, out UnsafeWeaponsData weaponsData)
    {
        weaponsData = new UnsafeWeaponsData(player);
    }

    public static void SetWeapons(this IPlayer playerInstance, in UnsafeWeaponsData weaponsData, bool forceSet = false)
    {
        if (forceSet)
        {
            if (!weaponsData.MeleeWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeapon.WeaponItem);
                playerInstance.SetCurrentMeleeDurability(weaponsData.MeleeWeapon.CurrentDurability);
            }

            if (!weaponsData.MeleeWeaponTemp.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeaponTemp.WeaponItem);
                playerInstance.SetCurrentMeleeMakeshiftDurability(weaponsData.MeleeWeaponTemp.CurrentDurability);
            }

            if (!weaponsData.SecondaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.SecondaryWeapon.WeaponItem);
                playerInstance.SetCurrentSecondaryWeaponAmmo(weaponsData.SecondaryWeapon.CurrentAmmo,
                    weaponsData.SecondaryWeapon.CurrentSpareMags,
                    weaponsData.SecondaryWeapon.ProjectilePowerupData.ProjectilePowerup);
            }

            if (!weaponsData.PrimaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PrimaryWeapon.WeaponItem);
                playerInstance.SetCurrentPrimaryWeaponAmmo(weaponsData.PrimaryWeapon.CurrentAmmo,
                    weaponsData.PrimaryWeapon.CurrentSpareMags,
                    weaponsData.PrimaryWeapon.ProjectilePowerupData.ProjectilePowerup);
            }

            if (!weaponsData.ThrowableItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.ThrowableItem.WeaponItem);
                playerInstance.SetCurrentThrownItemAmmo(weaponsData.ThrowableItem.CurrentAmmo);
            }

            if (!weaponsData.PowerupItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PowerupItem.WeaponItem);
            }

            return;
        }

        playerInstance.GetUnsafeWeaponsData(out var currentWeaponsData);

        if (currentWeaponsData.MeleeWeapon != weaponsData.MeleeWeapon)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.MeleeWeapon.WeaponItemType);

            if (!weaponsData.MeleeWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeapon.WeaponItem);
                playerInstance.SetCurrentMeleeDurability(weaponsData.MeleeWeapon.CurrentDurability);
            }
        }

        if (currentWeaponsData.MeleeWeaponTemp != weaponsData.MeleeWeaponTemp)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.MeleeWeaponTemp.WeaponItemType);

            if (!weaponsData.MeleeWeaponTemp.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeaponTemp.WeaponItem);
                playerInstance.SetCurrentMeleeMakeshiftDurability(weaponsData.MeleeWeaponTemp.CurrentDurability);
            }
        }

        if (currentWeaponsData.SecondaryWeapon != weaponsData.SecondaryWeapon)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.SecondaryWeapon.WeaponItemType);

            if (!weaponsData.SecondaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.SecondaryWeapon.WeaponItem);
                playerInstance.SetCurrentSecondaryWeaponAmmo(weaponsData.SecondaryWeapon.CurrentAmmo,
                    weaponsData.SecondaryWeapon.CurrentSpareMags,
                    weaponsData.SecondaryWeapon.ProjectilePowerupData.ProjectilePowerup);
            }
        }

        if (currentWeaponsData.PrimaryWeapon != weaponsData.PrimaryWeapon)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.PrimaryWeapon.WeaponItemType);

            if (!weaponsData.PrimaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PrimaryWeapon.WeaponItem);
                playerInstance.SetCurrentPrimaryWeaponAmmo(weaponsData.PrimaryWeapon.CurrentAmmo,
                    weaponsData.PrimaryWeapon.CurrentSpareMags,
                    weaponsData.PrimaryWeapon.ProjectilePowerupData.ProjectilePowerup);
            }
        }

        if (currentWeaponsData.ThrowableItem != weaponsData.ThrowableItem)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.ThrowableItem.WeaponItemType);

            if (!weaponsData.ThrowableItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.ThrowableItem.WeaponItem);
                playerInstance.SetCurrentThrownItemAmmo(weaponsData.ThrowableItem.CurrentAmmo);
            }
        }

        if (currentWeaponsData.PowerupItem != weaponsData.PowerupItem)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.PowerupItem.WeaponItemType);

            if (!weaponsData.PowerupItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PowerupItem.WeaponItem);
            }
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
                player.SetCurrentPrimaryWeaponAmmo(ammo, mags, firearm.ProjectilePowerupData.ProjectilePowerup);
                break;
            case WeaponItemType.Handgun:
                player.SetCurrentSecondaryWeaponAmmo(ammo, mags, firearm.ProjectilePowerupData.ProjectilePowerup);
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