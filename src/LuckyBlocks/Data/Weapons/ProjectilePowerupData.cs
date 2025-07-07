using LuckyBlocks.Data.Weapons.Unsafe;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal readonly record struct ProjectilePowerupData
{
    public ProjectilePowerup ProjectilePowerup { get; }
    public int Ammo { get; }

    public static readonly ProjectilePowerupData Empty = new(ProjectilePowerup.None, 0);

    private ProjectilePowerupData(ProjectilePowerup projectilePowerup, int ammo)
        => (ProjectilePowerup, Ammo) = (projectilePowerup, ammo);

    public static ProjectilePowerupData FromBouncingAndFireRounds(int powerupBouncingRounds, int powerupFireRounds)
    {
        if (powerupBouncingRounds > 0)
        {
            return new ProjectilePowerupData(ProjectilePowerup.Bouncing, powerupBouncingRounds);
        }

        return powerupFireRounds switch
        {
            > 0 => new ProjectilePowerupData(ProjectilePowerup.Fire, powerupFireRounds),
            _ => Empty
        };
    }

    public static implicit operator ProjectilePowerupData(in UnsafePowerupProjectileData data)
    {
        return new ProjectilePowerupData(data.ProjectilePowerup, data.Ammo);
    }
}