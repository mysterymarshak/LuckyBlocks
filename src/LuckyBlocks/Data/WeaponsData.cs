using System.Collections;
using System.Collections.Generic;
using LuckyBlocks.Data.Mappers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record Weapon(WeaponItem WeaponItem, WeaponItemType WeaponItemType)
{
    public static readonly Weapon Empty = new(WeaponItem.NONE, WeaponItemType.NONE);
    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;
}

internal record Melee(WeaponItem WeaponItem, WeaponItemType WeaponItemType, float CurrentDurability)
    : Weapon(WeaponItem, WeaponItemType)
{
    public float MaxDurability => 1f;
}

internal record MeleeTemp(WeaponItem WeaponItem, WeaponItemType WeaponItemType, float CurrentDurability)
    : Melee(WeaponItem, WeaponItemType, CurrentDurability);

internal record Firearm(WeaponItem WeaponItem, WeaponItemType WeaponItemType, int CurrentAmmo, int MaxTotalAmmo,
        int MagSize, int CurrentSpareMags, int MaxSpareMags, bool IsLazerEquipped,
        ProjectilePowerupData ProjectilePowerupData, ProjectileItem ProjectileItem)
    : Weapon(WeaponItem, WeaponItemType)
{
    public int TotalAmmo => CurrentAmmo + (CurrentSpareMags * MagSize);
}

internal record Shotgun(WeaponItem WeaponItem, WeaponItemType WeaponItemType, int CurrentAmmo, int MaxTotalAmmo,
        int MagSize, int CurrentSpareMags,
        int MaxSpareMags, bool IsLazerEquipped, ProjectilePowerupData ProjectilePowerupData,
        ProjectileItem ProjectileItem, int BulletsPerShot)
    : Firearm(WeaponItem, WeaponItemType, CurrentAmmo, MaxTotalAmmo, MagSize, CurrentSpareMags, MaxSpareMags,
        IsLazerEquipped, ProjectilePowerupData, ProjectileItem);

internal record Flamethrower(WeaponItem WeaponItem, WeaponItemType WeaponItemType, int CurrentAmmo, int MaxTotalAmmo,
        int MagSize, int CurrentSpareMags, int MaxSpareMags, bool IsLazerEquipped,
        ProjectilePowerupData ProjectilePowerupData, ProjectileItem ProjectileItem)
    : Firearm(WeaponItem, WeaponItemType, CurrentAmmo, MaxTotalAmmo, MagSize, CurrentSpareMags, MaxSpareMags,
        IsLazerEquipped, ProjectilePowerupData, ProjectileItem);

internal record Throwable(WeaponItem WeaponItem, WeaponItemType WeaponItemType, int CurrentAmmo, int MaxAmmo,
        bool IsActive)
    : Weapon(WeaponItem, WeaponItemType);

internal record Grenade(WeaponItem WeaponItem, WeaponItemType WeaponItemType, int CurrentAmmo, int MaxAmmo,
        bool IsActive, float TimeToExplosion)
    : Throwable(WeaponItem, WeaponItemType, CurrentAmmo, MaxAmmo, IsActive);

internal record Powerup(WeaponItem WeaponItem, WeaponItemType WeaponItemType) : Weapon(WeaponItem, WeaponItemType);

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
}

internal record WeaponsData : IEnumerable<Weapon>
{
    public IPlayer Owner { get; }
    public Melee MeleeWeapon { get; }
    public MeleeTemp MeleeWeaponTemp { get; }
    public Firearm SecondaryWeapon { get; }
    public Firearm PrimaryWeapon { get; }
    public Powerup PowerupItem { get; }
    public Throwable ThrowableItem { get; }
    public Weapon CurrentWeaponDrawn { get; }

    public WeaponsData(IPlayer player, IWeaponsMapper weaponsMapper)
    {
        Owner = player;
        
        var meleeWeapon = player.CurrentMeleeWeapon;
        MeleeWeapon = weaponsMapper.Map<MeleeWeaponItem, Melee>(meleeWeapon, player);
        
        var meleeTempWeapon = player.CurrentMeleeMakeshiftWeapon;
        meleeTempWeapon.IsMakeshift = true;
        MeleeWeaponTemp = weaponsMapper.Map<MeleeWeaponItem, MeleeTemp>(meleeTempWeapon, player);

        var secondaryWeapon = player.CurrentSecondaryWeapon;
        SecondaryWeapon = weaponsMapper.Map<HandgunWeaponItem, Firearm>(secondaryWeapon, player);

        var primaryWeapon = player.CurrentPrimaryWeapon;
        PrimaryWeapon = weaponsMapper.Map<RifleWeaponItem, Firearm>(primaryWeapon, player);

        var powerupItem = player.CurrentPowerupItem;
        PowerupItem = weaponsMapper.Map<PowerupWeaponItem, Powerup>(powerupItem, player);

        var thrownItem = player.CurrentThrownItem;
        ThrowableItem = weaponsMapper.Map<ThrownWeaponItem, Throwable>(thrownItem, player);

        CurrentWeaponDrawn = GetWeaponByType(player.CurrentWeaponDrawn);
    }

    public Weapon GetWeaponByType(WeaponItemType weaponItemType) => weaponItemType switch
    {
        WeaponItemType.Handgun => SecondaryWeapon,
        WeaponItemType.Rifle => PrimaryWeapon,
        WeaponItemType.Melee => MeleeWeapon.IsInvalid ? MeleeWeaponTemp : MeleeWeapon,
        WeaponItemType.Powerup => PowerupItem,
        WeaponItemType.Thrown => ThrowableItem,
        _ => Weapon.Empty
    };

    public IEnumerator<Weapon> GetEnumerator()
    {
        yield return MeleeWeapon;
        yield return MeleeWeaponTemp;
        yield return SecondaryWeapon;
        yield return PrimaryWeapon;
        yield return PowerupItem;
        yield return ThrowableItem;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}