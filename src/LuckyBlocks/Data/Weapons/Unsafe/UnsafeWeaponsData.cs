using System;
using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

internal readonly ref struct UnsafeWeaponsData
{
    public bool IsEmpty => !HasAnyWeapon();

    public readonly UnsafeMelee MeleeWeapon;
    public readonly UnsafeMeleeTemp MeleeWeaponTemp;
    public readonly UnsafeFirearm SecondaryWeapon;
    public readonly UnsafeFirearm PrimaryWeapon;
    public readonly UnsafePowerup PowerupItem;
    public readonly UnsafeThrowable ThrowableItem;

    public UnsafeWeaponsData(IPlayer playerInstance)
    {
        var meleeWeapon = playerInstance.CurrentMeleeWeapon;
        meleeWeapon.MaxValue = 1f;
        new UnsafeCasterMeleeWeaponItemToUnsafeMelee(ref meleeWeapon, ref MeleeWeapon);

        var meleeWeaponTemp = playerInstance.CurrentMeleeMakeshiftWeapon;
        meleeWeaponTemp.MaxValue = 1f;
        new UnsafeCasterMeleeWeaponItemToUnsafeMeleeTemp(ref meleeWeaponTemp, ref MeleeWeaponTemp);

        var secondaryWeapon = playerInstance.CurrentSecondaryWeapon;
        var (secondaryWeaponProjectilePowerup, secondaryWeaponPowerUppedAmmo) =
            UnsafePowerupProjectileData.FromBouncingAndFireRounds(secondaryWeapon.PowerupBouncingRounds,
                secondaryWeapon.PowerupFireRounds);
        secondaryWeapon.PowerupBouncingRounds = (int)secondaryWeaponProjectilePowerup;
        secondaryWeapon.PowerupFireRounds = secondaryWeaponPowerUppedAmmo;
        new UnsafeCasterHandgunWeaponItemToUnsafeFirearm(ref secondaryWeapon, ref SecondaryWeapon);

        var primaryWeapon = playerInstance.CurrentPrimaryWeapon;
        var (primaryWeaponProjectilePowerup, primaryWeaponPowerUppedAmmo) =
            UnsafePowerupProjectileData.FromBouncingAndFireRounds(primaryWeapon.PowerupBouncingRounds,
                primaryWeapon.PowerupFireRounds);
        primaryWeapon.PowerupBouncingRounds = (int)primaryWeaponProjectilePowerup;
        primaryWeapon.PowerupFireRounds = primaryWeaponPowerUppedAmmo;
        new UnsafeCasterRifleWeaponItemToUnsafeFirearm(ref primaryWeapon, ref PrimaryWeapon);

        var thrownItem = playerInstance.CurrentThrownItem;
        new UnsafeCasterThrownWeaponItemToUnsafeThrowable(ref thrownItem, ref ThrowableItem);

        var powerupItem = playerInstance.CurrentPowerupItem;
        new UnsafeCasterPowerupWeaponItemToUnsafePowerup(ref powerupItem, ref PowerupItem);
    }

    public UnsafeWeaponsData(IPlayer playerInstance, WeaponItemType weaponItemType)
    {
        switch (weaponItemType)
        {
            case WeaponItemType.Melee:
                var meleeWeapon = playerInstance.CurrentMeleeWeapon;
                meleeWeapon.MaxValue = 1f;
                new UnsafeCasterMeleeWeaponItemToUnsafeMelee(ref meleeWeapon, ref MeleeWeapon);

                var meleeWeaponTemp = playerInstance.CurrentMeleeMakeshiftWeapon;
                meleeWeaponTemp.MaxValue = 1f;
                new UnsafeCasterMeleeWeaponItemToUnsafeMeleeTemp(ref meleeWeaponTemp, ref MeleeWeaponTemp);

                break;
            case WeaponItemType.Handgun:
                var secondaryWeapon = playerInstance.CurrentSecondaryWeapon;
                var (secondaryWeaponProjectilePowerup, secondaryWeaponPowerUppedAmmo) =
                    UnsafePowerupProjectileData.FromBouncingAndFireRounds(secondaryWeapon.PowerupBouncingRounds,
                        secondaryWeapon.PowerupFireRounds);
                secondaryWeapon.PowerupBouncingRounds = (int)secondaryWeaponProjectilePowerup;
                secondaryWeapon.PowerupFireRounds = secondaryWeaponPowerUppedAmmo;
                new UnsafeCasterHandgunWeaponItemToUnsafeFirearm(ref secondaryWeapon, ref SecondaryWeapon);

                break;
            case WeaponItemType.Rifle:
                var primaryWeapon = playerInstance.CurrentPrimaryWeapon;
                var (primaryWeaponProjectilePowerup, primaryWeaponPowerUppedAmmo) =
                    UnsafePowerupProjectileData.FromBouncingAndFireRounds(primaryWeapon.PowerupBouncingRounds,
                        primaryWeapon.PowerupFireRounds);
                primaryWeapon.PowerupBouncingRounds = (int)primaryWeaponProjectilePowerup;
                primaryWeapon.PowerupFireRounds = primaryWeaponPowerUppedAmmo;
                new UnsafeCasterRifleWeaponItemToUnsafeFirearm(ref primaryWeapon, ref PrimaryWeapon);

                break;
            case WeaponItemType.Thrown:
                var thrownItem = playerInstance.CurrentThrownItem;
                new UnsafeCasterThrownWeaponItemToUnsafeThrowable(ref thrownItem, ref ThrowableItem);

                break;
            case WeaponItemType.Powerup:
                var powerupItem = playerInstance.CurrentPowerupItem;
                new UnsafeCasterPowerupWeaponItemToUnsafePowerup(ref powerupItem, ref PowerupItem);

                break;
            default:
                throw new ArgumentException(nameof(weaponItemType));
        }
    }

    private bool HasAnyWeapon()
    {
        return !(MeleeWeapon.IsInvalid && MeleeWeaponTemp.IsInvalid && SecondaryWeapon.IsInvalid &&
                 PrimaryWeapon.IsInvalid && PowerupItem.IsInvalid && ThrowableItem.IsInvalid);
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
}