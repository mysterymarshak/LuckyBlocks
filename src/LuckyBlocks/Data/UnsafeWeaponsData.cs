using System;
using System.Runtime.InteropServices;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

[StructLayout(LayoutKind.Explicit)]
internal readonly ref struct UnsafeMelee
{
    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;

    [FieldOffset(0)]
    public readonly WeaponItemType WeaponItemType;

    [FieldOffset(sizeof(WeaponItemType))]
    public readonly WeaponItem WeaponItem;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem))]
    public readonly float CurrentDurability;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(float) * 2)]
    public readonly float MaxDurability;

    // parameter 'Value' after 'CurrentDurability' was removed
    // 'MaxValue' have const value 100f
    // i change its name to 'MaxDurability' and overwrite value 100f with 1f
    // parameter 'IsMakeshift' for type UnsafeMelee was removed 

    public static bool operator ==(in UnsafeMelee melee1, Melee melee2)
    {
        return melee1.WeaponItemType == melee2.WeaponItemType && melee1.WeaponItem == melee2.WeaponItem &&
               melee1.CurrentDurability == melee2.CurrentDurability && melee1.MaxDurability == melee2.MaxDurability;
    }

    public static bool operator ==(in UnsafeMelee melee1, in UnsafeMelee melee2)
    {
        return melee1.WeaponItemType == melee2.WeaponItemType && melee1.WeaponItem == melee2.WeaponItem &&
               melee1.CurrentDurability == melee2.CurrentDurability && melee1.MaxDurability == melee2.MaxDurability;
    }

    public static bool operator !=(in UnsafeMelee melee1, in UnsafeMelee melee2)
    {
        return !(melee1 == melee2);
    }

    public static bool operator !=(in UnsafeMelee melee1, Melee melee2)
    {
        return !(melee1 == melee2);
    }
}

[StructLayout(LayoutKind.Explicit)]
internal readonly ref struct UnsafeMeleeTemp
{
    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;

    [FieldOffset(0)]
    public readonly WeaponItemType WeaponItemType;

    [FieldOffset(sizeof(WeaponItemType))]
    public readonly WeaponItem WeaponItem;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem))]
    public readonly float CurrentDurability;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(float) * 2)]
    public readonly float MaxDurability;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(float) * 3)]
    public readonly bool IsMakeshift;

    // parameter 'Value' after 'CurrentDurability' was removed
    // 'MaxValue' have const value 100f
    // i change its name to 'MaxDurability' and overwrite value 100f with 1f

    public static bool operator ==(in UnsafeMeleeTemp meleeTemp1, MeleeTemp meleeTemp2)
    {
        return meleeTemp1.WeaponItemType == meleeTemp2.WeaponItemType &&
               meleeTemp1.WeaponItem == meleeTemp2.WeaponItem &&
               meleeTemp1.CurrentDurability == meleeTemp2.CurrentDurability &&
               meleeTemp1.MaxDurability == meleeTemp2.MaxDurability;
    }

    public static bool operator ==(in UnsafeMeleeTemp meleeTemp1, in UnsafeMeleeTemp meleeTemp2)
    {
        return meleeTemp1.WeaponItemType == meleeTemp2.WeaponItemType &&
               meleeTemp1.WeaponItem == meleeTemp2.WeaponItem &&
               meleeTemp1.CurrentDurability == meleeTemp2.CurrentDurability &&
               meleeTemp1.MaxDurability == meleeTemp2.MaxDurability;
    }

    public static bool operator !=(in UnsafeMeleeTemp meleeTemp1, in UnsafeMeleeTemp meleeTemp2)
    {
        return !(meleeTemp1 == meleeTemp2);
    }

    public static bool operator !=(in UnsafeMeleeTemp meleeTemp1, MeleeTemp meleeTemp2)
    {
        return !(meleeTemp1 == meleeTemp2);
    }
}

[StructLayout(LayoutKind.Explicit)]
internal readonly ref struct UnsafePowerupProjectileData
{
    [FieldOffset(0)]
    public readonly ProjectilePowerup ProjectilePowerup;

    [FieldOffset(sizeof(ProjectilePowerup))]
    public readonly int Ammo;

    public static (ProjectilePowerup, int)
        FromBouncingAndFireRounds(int powerupBouncingRounds, int powerupFireRounds) =>
        (powerupBouncingRounds, powerupFireRounds) switch
        {
            (> 0, _) => (ProjectilePowerup.Bouncing, powerupBouncingRounds),
            (_, > 0) => (ProjectilePowerup.Fire, powerupFireRounds),
            _ => (ProjectilePowerup.None, default)
        };

    public static bool operator ==(in UnsafePowerupProjectileData powerupProjectileData1,
        ProjectilePowerupData powerupProjectileData2)
    {
        return powerupProjectileData1.ProjectilePowerup == powerupProjectileData2.ProjectilePowerup &&
               powerupProjectileData1.Ammo == powerupProjectileData2.Ammo;
    }

    public static bool operator ==(in UnsafePowerupProjectileData powerupProjectileData1,
        in UnsafePowerupProjectileData powerupProjectileData2)
    {
        return powerupProjectileData1.ProjectilePowerup == powerupProjectileData2.ProjectilePowerup &&
               powerupProjectileData1.Ammo == powerupProjectileData2.Ammo;
    }

    public static bool operator !=(in UnsafePowerupProjectileData powerupProjectileData1,
        in UnsafePowerupProjectileData powerupProjectileData2)
    {
        return !(powerupProjectileData1 == powerupProjectileData2);
    }

    public static bool operator !=(in UnsafePowerupProjectileData powerupProjectileData1,
        ProjectilePowerupData powerupProjectileData2)
    {
        return !(powerupProjectileData1 == powerupProjectileData2);
    }
}

[StructLayout(LayoutKind.Explicit)]
internal readonly ref struct UnsafeFirearm
{
    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;

    [FieldOffset(0)]
    public readonly WeaponItemType WeaponItemType;

    [FieldOffset(sizeof(WeaponItemType))]
    public readonly WeaponItem WeaponItem;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem))]
    public readonly int TotalAmmo;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int))]
    public readonly int CurrentAmmo;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 2)]
    public readonly int CurrentSpareMags;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 3)]
    public readonly int MagSize;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 4)]
    public readonly int BulletsPerShot;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 5)]
    public readonly int MaxSpareMags;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 6)]
    public readonly int MaxTotalAmmo;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 7)]
    public readonly UnsafePowerupProjectileData ProjectilePowerupData;

    //  field 'PowerupBouncingRounds' was replaced with enum value ProjectilePowerup
    //  field 'PowerupFireRounds' was replaced with actual ammo with applied powerup

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 9)]
    public readonly bool IsLazerEquipped;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int) * 9 + sizeof(bool) * 4)]
    public readonly ProjectileItem ProjectileItem;

    public static bool operator ==(in UnsafeFirearm firearm1, Firearm firearm2)
    {
        return firearm1.WeaponItemType == firearm2.WeaponItemType && firearm1.WeaponItem == firearm2.WeaponItem &&
               firearm1.TotalAmmo == firearm2.TotalAmmo && firearm1.CurrentAmmo == firearm2.CurrentAmmo &&
               firearm1.CurrentSpareMags == firearm2.CurrentSpareMags && firearm1.MagSize == firearm2.MagSize &&
               firearm1.MaxSpareMags == firearm2.MaxSpareMags && firearm1.MaxTotalAmmo == firearm2.MaxTotalAmmo &&
               firearm1.ProjectilePowerupData == firearm2.ProjectilePowerupData &&
               firearm1.IsLazerEquipped == firearm2.IsLazerEquipped &&
               firearm1.ProjectileItem == firearm2.ProjectileItem;
    }

    public static bool operator ==(in UnsafeFirearm firearm1, in UnsafeFirearm firearm2)
    {
        return firearm1.WeaponItemType == firearm2.WeaponItemType && firearm1.WeaponItem == firearm2.WeaponItem &&
               firearm1.TotalAmmo == firearm2.TotalAmmo && firearm1.CurrentAmmo == firearm2.CurrentAmmo &&
               firearm1.CurrentSpareMags == firearm2.CurrentSpareMags && firearm1.MagSize == firearm2.MagSize &&
               firearm1.MaxSpareMags == firearm2.MaxSpareMags && firearm1.MaxTotalAmmo == firearm2.MaxTotalAmmo &&
               firearm1.ProjectilePowerupData == firearm2.ProjectilePowerupData &&
               firearm1.IsLazerEquipped == firearm2.IsLazerEquipped &&
               firearm1.ProjectileItem == firearm2.ProjectileItem;
    }

    public static bool operator !=(in UnsafeFirearm firearm1, in UnsafeFirearm firearm2)
    {
        return !(firearm1 == firearm2);
    }

    public static bool operator !=(in UnsafeFirearm firearm1, Firearm firearm2)
    {
        return !(firearm1 == firearm2);
    }
}

[StructLayout(LayoutKind.Explicit)]
internal readonly ref struct UnsafePowerup
{
    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;

    [FieldOffset(0)]
    public readonly WeaponItemType WeaponItemType;

    [FieldOffset(sizeof(WeaponItemType))]
    public readonly WeaponItem WeaponItem;

    public static bool operator ==(in UnsafePowerup powerup1, PowerupItem powerup2)
    {
        return powerup1.WeaponItemType == powerup2.WeaponItemType && powerup1.WeaponItem == powerup2.WeaponItem;
    }

    public static bool operator ==(in UnsafePowerup powerup1, in UnsafePowerup powerup2)
    {
        return powerup1.WeaponItemType == powerup2.WeaponItemType && powerup1.WeaponItem == powerup2.WeaponItem;
    }

    public static bool operator !=(in UnsafePowerup powerup1, in UnsafePowerup powerup2)
    {
        return !(powerup1 == powerup2);
    }

    public static bool operator !=(in UnsafePowerup powerup1, PowerupItem powerup2)
    {
        return !(powerup1 == powerup2);
    }
}

[StructLayout(LayoutKind.Explicit)]
internal readonly ref struct UnsafeThrowable
{
    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;

    [FieldOffset(0)]
    public readonly WeaponItemType WeaponItemType;

    [FieldOffset(sizeof(WeaponItemType))]
    public readonly WeaponItem WeaponItem;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem))]
    public readonly int CurrentAmmo;

    [FieldOffset(sizeof(WeaponItemType) + sizeof(WeaponItem) + sizeof(int))]
    public readonly int MaxAmmo;

    public static bool operator ==(in UnsafeThrowable throwable1, Throwable throwable2)
    {
        return throwable1.WeaponItemType == throwable2.WeaponItemType &&
               throwable1.WeaponItem == throwable2.WeaponItem && throwable1.CurrentAmmo == throwable2.CurrentAmmo &&
               throwable1.MaxAmmo == throwable2.MaxAmmo;
    }

    public static bool operator ==(in UnsafeThrowable throwable1, in UnsafeThrowable throwable2)
    {
        return throwable1.WeaponItemType == throwable2.WeaponItemType &&
               throwable1.WeaponItem == throwable2.WeaponItem && throwable1.CurrentAmmo == throwable2.CurrentAmmo &&
               throwable1.MaxAmmo == throwable2.MaxAmmo;
    }

    public static bool operator !=(in UnsafeThrowable throwable1, in UnsafeThrowable throwable2)
    {
        return !(throwable1 == throwable2);
    }

    public static bool operator !=(in UnsafeThrowable throwable1, Throwable throwable2)
    {
        return !(throwable1 == throwable2);
    }
}

internal readonly ref struct UnsafeWeaponsData
{
    public bool IsEmpty => !this.HasAnyWeapon();

    public readonly UnsafeMelee MeleeWeapon;
    public readonly UnsafeMeleeTemp MeleeWeaponTemp;
    public readonly UnsafeFirearm SecondaryWeapon;
    public readonly UnsafeFirearm PrimaryWeapon;
    public readonly UnsafePowerup PowerupItem;
    public readonly UnsafeThrowable ThrowableItem;

    public UnsafeWeaponsData(IPlayer player)
    {
        var meleeWeapon = player.CurrentMeleeWeapon;
        meleeWeapon.MaxValue = 1f;
        new UnsafeCasterMeleeWeaponItemToUnsafeMelee(ref meleeWeapon, ref MeleeWeapon);

        var meleeWeaponTemp = player.CurrentMeleeMakeshiftWeapon;
        meleeWeaponTemp.MaxValue = 1f;
        new UnsafeCasterMeleeWeaponItemToUnsafeMeleeTemp(ref meleeWeaponTemp, ref MeleeWeaponTemp);

        var secondaryWeapon = player.CurrentSecondaryWeapon;
        var (secondaryWeaponProjectilePowerup, secondaryWeaponPowerUppedAmmo) =
            UnsafePowerupProjectileData.FromBouncingAndFireRounds(secondaryWeapon.PowerupBouncingRounds,
                secondaryWeapon.PowerupFireRounds);
        secondaryWeapon.PowerupBouncingRounds = (int)secondaryWeaponProjectilePowerup;
        secondaryWeapon.PowerupFireRounds = secondaryWeaponPowerUppedAmmo;
        new UnsafeCasterHandgunWeaponItemToUnsafeFirearm(ref secondaryWeapon, ref SecondaryWeapon);

        var primaryWeapon = player.CurrentPrimaryWeapon;
        var (primaryWeaponProjectilePowerup, primaryWeaponPowerUppedAmmo) =
            UnsafePowerupProjectileData.FromBouncingAndFireRounds(primaryWeapon.PowerupBouncingRounds,
                primaryWeapon.PowerupFireRounds);
        primaryWeapon.PowerupBouncingRounds = (int)primaryWeaponProjectilePowerup;
        primaryWeapon.PowerupFireRounds = primaryWeaponPowerUppedAmmo;
        new UnsafeCasterRifleWeaponItemToUnsafeFirearm(ref primaryWeapon, ref PrimaryWeapon);

        var powerupItem = player.CurrentPowerupItem;
        new UnsafeCasterPowerupWeaponItemToUnsafePowerup(ref powerupItem, ref PowerupItem);

        var thrownItem = player.CurrentThrownItem;
        new UnsafeCasterThrownWeaponItemToUnsafeThrowable(ref thrownItem, ref ThrowableItem);
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly unsafe ref struct UnsafeCasterMeleeWeaponItemToUnsafeMelee
    {
        [FieldOffset(0)]
        public readonly MeleeWeaponItem Struct1;

        [FieldOffset(0)]
        public readonly UnsafeMelee Struct2;

        public UnsafeCasterMeleeWeaponItemToUnsafeMelee(ref MeleeWeaponItem struct1, ref UnsafeMelee struct2)
        {
            if (sizeof(MeleeWeaponItem) < sizeof(UnsafeMelee))
            {
                throw new InvalidCastException();
            }

            Struct1 = struct1;
            struct2 = Struct2;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly unsafe ref struct UnsafeCasterMeleeWeaponItemToUnsafeMeleeTemp
    {
        [FieldOffset(0)]
        public readonly MeleeWeaponItem Struct1;

        [FieldOffset(0)]
        public readonly UnsafeMeleeTemp Struct2;

        public UnsafeCasterMeleeWeaponItemToUnsafeMeleeTemp(ref MeleeWeaponItem struct1, ref UnsafeMeleeTemp struct2)
        {
            if (sizeof(MeleeWeaponItem) < sizeof(UnsafeMeleeTemp))
            {
                throw new InvalidCastException();
            }

            Struct1 = struct1;
            struct2 = Struct2;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly unsafe ref struct UnsafeCasterHandgunWeaponItemToUnsafeFirearm
    {
        [FieldOffset(0)]
        public readonly HandgunWeaponItem Struct1;

        [FieldOffset(0)]
        public readonly UnsafeFirearm Struct2;

        public UnsafeCasterHandgunWeaponItemToUnsafeFirearm(ref HandgunWeaponItem struct1, ref UnsafeFirearm struct2)
        {
            if (sizeof(HandgunWeaponItem) < sizeof(UnsafeFirearm))
            {
                throw new InvalidCastException();
            }

            Struct1 = struct1;
            struct2 = Struct2;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly unsafe ref struct UnsafeCasterRifleWeaponItemToUnsafeFirearm
    {
        [FieldOffset(0)]
        public readonly RifleWeaponItem Struct1;

        [FieldOffset(0)]
        public readonly UnsafeFirearm Struct2;

        public UnsafeCasterRifleWeaponItemToUnsafeFirearm(ref RifleWeaponItem struct1, ref UnsafeFirearm struct2)
        {
            if (sizeof(RifleWeaponItem) < sizeof(UnsafeFirearm))
            {
                throw new InvalidCastException();
            }

            Struct1 = struct1;
            struct2 = Struct2;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly unsafe ref struct UnsafeCasterPowerupWeaponItemToUnsafePowerup
    {
        [FieldOffset(0)]
        public readonly PowerupWeaponItem Struct1;

        [FieldOffset(0)]
        public readonly UnsafePowerup Struct2;

        public UnsafeCasterPowerupWeaponItemToUnsafePowerup(ref PowerupWeaponItem struct1, ref UnsafePowerup struct2)
        {
            if (sizeof(PowerupWeaponItem) < sizeof(UnsafePowerup))
            {
                throw new InvalidCastException();
            }

            Struct1 = struct1;
            struct2 = Struct2;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly unsafe ref struct UnsafeCasterThrownWeaponItemToUnsafeThrowable
    {
        [FieldOffset(0)]
        public readonly ThrownWeaponItem Struct1;

        [FieldOffset(0)]
        public readonly UnsafeThrowable Struct2;

        public UnsafeCasterThrownWeaponItemToUnsafeThrowable(ref ThrownWeaponItem struct1, ref UnsafeThrowable struct2)
        {
            if (sizeof(ThrownWeaponItem) < sizeof(UnsafeThrowable))
            {
                throw new InvalidCastException();
            }

            Struct1 = struct1;
            struct2 = Struct2;
        }
    }
}