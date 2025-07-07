using System.Runtime.InteropServices;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons.Unsafe;

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