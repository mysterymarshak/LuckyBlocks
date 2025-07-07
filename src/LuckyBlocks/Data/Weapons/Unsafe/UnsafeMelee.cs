using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

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