using System.Collections.Generic;
using System.Linq;
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

    public static bool HasAnyWeapon(this WeaponsData weaponsData)
    {
        return !(weaponsData.MeleeWeapon.IsInvalid && weaponsData.MeleeWeaponTemp.IsInvalid &&
                 weaponsData.SecondaryWeapon.IsInvalid &&
                 weaponsData.PrimaryWeapon.IsInvalid && weaponsData.PowerupItem.IsInvalid &&
                 weaponsData.ThrowableItem.IsInvalid);
    }

    public static bool WeaponsExists(this WeaponsData weaponsData, IEnumerable<WeaponItem> weaponItems)
    {
        return weaponItems.All(x => weaponsData.MeleeWeapon.WeaponItem == x ||
                                    weaponsData.MeleeWeaponTemp.WeaponItem == x ||
                                    weaponsData.SecondaryWeapon.WeaponItem == x ||
                                    weaponsData.PrimaryWeapon.WeaponItem == x ||
                                    weaponsData.PowerupItem.WeaponItem == x || weaponsData.ThrowableItem.WeaponItem == x);
    }
}