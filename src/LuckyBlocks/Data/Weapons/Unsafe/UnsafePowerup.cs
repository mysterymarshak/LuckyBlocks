using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

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