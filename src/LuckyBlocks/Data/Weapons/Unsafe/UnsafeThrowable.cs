using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

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