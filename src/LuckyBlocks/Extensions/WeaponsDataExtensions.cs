using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class WeaponsDataExtensions
{
    public static IEnumerable<Firearm> GetFirearms(this WeaponsData weaponsData)
    {
        var secondaryWeapon = weaponsData.SecondaryWeapon;
        if (secondaryWeapon is { IsInvalid: false })
            yield return secondaryWeapon;

        var primaryWeapon = weaponsData.PrimaryWeapon;
        if (primaryWeapon is { IsInvalid: false } and not Flamethrower)
            yield return primaryWeapon;
    }

    public static bool HasAnyFirearm(this WeaponsData weaponsData)
    {
        var secondaryWeapon = weaponsData.SecondaryWeapon;
        var primaryWeapon = weaponsData.PrimaryWeapon;

        return secondaryWeapon is { IsInvalid: false } || primaryWeapon is { IsInvalid: false } and not Flamethrower;
    }

    public static void UpdateFirearms(this WeaponsData weaponsData)
    {
        var playerInstance = weaponsData.Owner;
        playerInstance.GetUnsafeWeaponsData(out var unsafeWeaponsData);

        weaponsData.UpdateSecondary(unsafeWeaponsData.SecondaryWeapon);
        weaponsData.UpdatePrimary(unsafeWeaponsData.PrimaryWeapon);
    }

    public static IEnumerable<Weapon> GetWeaponsByType(this WeaponsData weaponsData, Type weaponType)
    {
        if (weaponType == typeof(Melee))
        {
            if (!weaponsData.MeleeWeapon.IsInvalid)
            {
                yield return weaponsData.MeleeWeapon;
            }
        }
        else if (weaponType == typeof(MeleeTemp))
        {
            if (!weaponsData.MeleeWeaponTemp.IsInvalid)
            {
                yield return weaponsData.MeleeWeaponTemp;
            }
        }
        else if (weaponType == typeof(Shotgun) || weaponType == typeof(Flamethrower))
        {
            if (weaponsData.PrimaryWeapon is Shotgun { IsInvalid: false } or Flamethrower { IsInvalid: false })
            {
                yield return weaponsData.PrimaryWeapon;
            }
        }
        else if (weaponType == typeof(Firearm))
        {
            if (weaponsData.CurrentWeaponDrawn is
                { IsInvalid: false, WeaponItemType: WeaponItemType.Handgun or WeaponItemType.Rifle })
            {
                yield return weaponsData.CurrentWeaponDrawn;

                if (weaponsData.CurrentWeaponDrawn.WeaponItemType == weaponsData.SecondaryWeapon.WeaponItemType &&
                    !weaponsData.PrimaryWeapon.IsInvalid)
                {
                    yield return weaponsData.PrimaryWeapon;
                }
                
                if (weaponsData.CurrentWeaponDrawn.WeaponItemType == weaponsData.PrimaryWeapon.WeaponItemType &&
                    !weaponsData.SecondaryWeapon.IsInvalid)
                {
                    yield return weaponsData.SecondaryWeapon;
                }
            }
            else
            {
                if (!weaponsData.SecondaryWeapon.IsInvalid)
                {
                    yield return weaponsData.SecondaryWeapon;
                }

                if (!weaponsData.PrimaryWeapon.IsInvalid)
                {
                    yield return weaponsData.PrimaryWeapon;
                }
            }
        }
        else if (weaponType == typeof(Grenade) || weaponType == typeof(Throwable))
        {
            if (!weaponsData.ThrowableItem.IsInvalid)
            {
                yield return weaponsData.ThrowableItem;
            }
        }
        else if (weaponType == typeof(PowerupItem))
        {
            if (!weaponsData.PowerupItem.IsInvalid)
            {
                yield return weaponsData.PowerupItem;
            }
        }
        else
        {
            foreach (var weapon in weaponsData)
            {
                yield return weapon;
            }
        }
    }
}