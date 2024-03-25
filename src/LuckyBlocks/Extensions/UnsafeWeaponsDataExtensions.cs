using System.Collections.Generic;
using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class UnsafeWeaponsDataExtensions
{
    public static bool HasAnyWeapon(this in UnsafeWeaponsData weaponsData)
    {
        return !(weaponsData.MeleeWeapon.IsInvalid && weaponsData.MeleeWeaponTemp.IsInvalid &&
                 weaponsData.SecondaryWeapon.IsInvalid &&
                 weaponsData.PrimaryWeapon.IsInvalid && weaponsData.PowerupItem.IsInvalid &&
                 weaponsData.ThrowableItem.IsInvalid);
    }

    public static bool HasAnyFirearm(this in UnsafeWeaponsData weaponsData)
    {
        var secondaryWeapon = weaponsData.SecondaryWeapon;
        var primaryWeapon = weaponsData.PrimaryWeapon;

        return secondaryWeapon is { IsInvalid: false } || primaryWeapon is
            { IsInvalid: false, WeaponItem: not WeaponItem.FLAMETHROWER };
    }

    public static bool HasThrowableItem(this in UnsafeWeaponsData weaponsData)
    {
        return weaponsData.ThrowableItem is { IsInvalid: false };
    }

    public static bool WeaponsExists(this in UnsafeWeaponsData weaponsData, IEnumerable<WeaponItem> weaponItems)
    {
        foreach (var weaponItem in weaponItems)
        {
            if (!(weaponsData.MeleeWeapon.WeaponItem == weaponItem ||
                  weaponsData.MeleeWeaponTemp.WeaponItem == weaponItem ||
                  weaponsData.SecondaryWeapon.WeaponItem == weaponItem ||
                  weaponsData.PrimaryWeapon.WeaponItem == weaponItem ||
                  weaponsData.PowerupItem.WeaponItem == weaponItem ||
                  weaponsData.ThrowableItem.WeaponItem == weaponItem))
                return false;
        }

        return true;
    }
}