using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

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