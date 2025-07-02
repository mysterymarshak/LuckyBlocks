using System.Collections.Generic;
using LuckyBlocks.Data;

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
}