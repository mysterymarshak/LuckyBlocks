using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

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