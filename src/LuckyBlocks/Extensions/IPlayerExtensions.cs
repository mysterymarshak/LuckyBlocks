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

    public static void RemoveAllWeapons(this IPlayer playerInstance)
    {
        playerInstance.RemoveWeaponItemType(WeaponItemType.Melee);
        playerInstance.RemoveWeaponItemType(WeaponItemType.Handgun);
        playerInstance.RemoveWeaponItemType(WeaponItemType.Rifle);
        playerInstance.RemoveWeaponItemType(WeaponItemType.Thrown);
        playerInstance.RemoveWeaponItemType(WeaponItemType.Powerup);
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

    public static void SetAmmo(this IPlayer playerInstance, Firearm firearm, int ammo, int mags)
    {
        switch (firearm.WeaponItemType)
        {
            case WeaponItemType.Rifle:
                playerInstance.SetCurrentPrimaryWeaponAmmo(ammo, mags, firearm.ProjectilePowerupData.ProjectilePowerup);
                break;
            case WeaponItemType.Handgun:
                playerInstance.SetCurrentSecondaryWeaponAmmo(ammo, mags,
                    firearm.ProjectilePowerupData.ProjectilePowerup);
                break;
        }
    }

    public static void SetAmmo(this IPlayer playerInstance, Firearm firearm, int totalAmmo)
    {
        switch (firearm.WeaponItemType)
        {
            case WeaponItemType.Rifle:
                playerInstance.SetCurrentPrimaryWeaponAmmo(totalAmmo);
                break;
            case WeaponItemType.Handgun:
                playerInstance.SetCurrentSecondaryWeaponAmmo(totalAmmo);
                break;
        }
    }

    // triggers WeaponRemoved event on next tick
    public static void SetAmmo(this IPlayer playerInstance, Throwable throwable, int ammo)
    {
        playerInstance.SetCurrentThrownItemAmmo(ammo);
    }

    public static Vector2 GetHandPosition(this IPlayer playerInstance)
    {
        var faceDirection = playerInstance.GetFaceDirection();
        return playerInstance.GetWorldPosition() + playerInstance switch
        {
            { IsMeleeAttacking: true } => new Vector2(0, 9) + faceDirection * new Vector2(12, 0),
            { IsBlocking: true } => new Vector2(0, 9) + faceDirection * new Vector2(8, 0),
            _ => new Vector2(0, 5) + faceDirection * new Vector2(10, 0)
        };
    }
}