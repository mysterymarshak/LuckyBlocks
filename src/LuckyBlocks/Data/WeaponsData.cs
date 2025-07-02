using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Extensions;
using LuckyBlocks.Loot.WeaponPowerups;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data;

internal record Weapon(WeaponItem WeaponItem, WeaponItemType WeaponItemType)
{
    public static readonly Weapon Empty = new(WeaponItem.NONE, WeaponItemType.NONE);

    public bool IsInvalid => WeaponItem == WeaponItem.NONE || WeaponItemType == WeaponItemType.NONE;
    public bool IsDropped => Owner?.IsValid() != true;
    public IPlayer? Owner { get; private set; }
    public int ObjectId { get; private set; }

    public IEnumerable<IWeaponPowerup<Weapon>> Powerups =>
        _powerups ?? Enumerable.Empty<IWeaponPowerup<Weapon>>();

    private readonly List<IWeaponPowerup<Weapon>>? _powerups = null;

    public void SetOwner(IPlayer player)
    {
        Owner = player;
        ObjectId = 0;
    }

    public void SetObject(int objectId)
    {
        Owner = null;
        ObjectId = objectId;
    }
}

internal record Melee(WeaponItem WeaponItem, WeaponItemType WeaponItemType, float CurrentDurability)
    : Weapon(WeaponItem, WeaponItemType)
{
    public float CurrentDurability { get; protected set; } = CurrentDurability;
    public float MaxDurability => 1f;

    public void Update(in UnsafeMelee newData)
    {
        CurrentDurability = newData.CurrentDurability;
    }
}

internal sealed record MeleeTemp(WeaponItem WeaponItem, WeaponItemType WeaponItemType, float CurrentDurability)
    : Melee(WeaponItem, WeaponItemType, CurrentDurability)
{
    public void Update(in UnsafeMeleeTemp newData)
    {
        CurrentDurability = newData.CurrentDurability;
    }
}

internal record Firearm(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxTotalAmmo,
    int MagSize,
    int CurrentSpareMags,
    int MaxSpareMags,
    bool IsLazerEquipped,
    ProjectilePowerupData ProjectilePowerupData,
    ProjectileItem ProjectileItem)
    : Weapon(WeaponItem, WeaponItemType)
{
    public int CurrentAmmo { get; protected set; } = CurrentAmmo;
    public int CurrentSpareMags { get; protected set; } = CurrentSpareMags;
    public bool IsLazerEquipped { get; protected set; } = IsLazerEquipped;
    public ProjectilePowerupData ProjectilePowerupData { get; protected set; } = ProjectilePowerupData;
    public new IEnumerable<IFirearmPowerup> Powerups => base.Powerups.Cast<IFirearmPowerup>();
    public int TotalAmmo => CurrentAmmo + (CurrentSpareMags * MagSize);

    public void Update(in UnsafeFirearm newData)
    {
        CurrentAmmo = newData.CurrentAmmo;
        CurrentSpareMags = newData.CurrentSpareMags;
        IsLazerEquipped = newData.IsLazerEquipped;
        ProjectilePowerupData = newData.ProjectilePowerupData;
    }
}

internal sealed record Shotgun(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxTotalAmmo,
    int MagSize,
    int CurrentSpareMags,
    int MaxSpareMags,
    bool IsLazerEquipped,
    ProjectilePowerupData ProjectilePowerupData,
    ProjectileItem ProjectileItem,
    int BulletsPerShot) : Firearm(WeaponItem, WeaponItemType, CurrentAmmo, MaxTotalAmmo, MagSize, CurrentSpareMags,
    MaxSpareMags, IsLazerEquipped, ProjectilePowerupData, ProjectileItem);

internal sealed record Flamethrower(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxTotalAmmo,
    int MagSize,
    int CurrentSpareMags,
    int MaxSpareMags,
    bool IsLazerEquipped,
    ProjectilePowerupData ProjectilePowerupData,
    ProjectileItem ProjectileItem)
    : Firearm(WeaponItem, WeaponItemType, CurrentAmmo, MaxTotalAmmo, MagSize, CurrentSpareMags, MaxSpareMags,
        IsLazerEquipped, ProjectilePowerupData, ProjectileItem);

internal record Throwable(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxAmmo,
    bool IsActive)
    : Weapon(WeaponItem, WeaponItemType)
{
    public int CurrentAmmo { get; protected set; } = CurrentAmmo;
    public bool IsActive { get; protected set; } = IsActive;

    public void Update(in UnsafeThrowable newData, bool isActive)
    {
        CurrentAmmo = newData.CurrentAmmo;
        IsActive = isActive;
    }
}

internal sealed record Grenade(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxAmmo,
    bool IsActive,
    float TimeToExplosion)
    : Throwable(WeaponItem, WeaponItemType, CurrentAmmo, MaxAmmo, IsActive)
{
    public float TimeToExplosion { get; private set; } = TimeToExplosion;

    public void Update(bool isActive, float timeToExplosion)
    {
        IsActive = isActive;
        TimeToExplosion = timeToExplosion;
    }
}

internal sealed record PowerupItem(WeaponItem WeaponItem, WeaponItemType WeaponItemType)
    : Weapon(WeaponItem, WeaponItemType);

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

internal sealed class WeaponsData : IEnumerable<Weapon>
{
    public IPlayer Owner { get; }
    public Melee MeleeWeapon { get; private set; }
    public MeleeTemp MeleeWeaponTemp { get; private set; }
    public Firearm SecondaryWeapon { get; private set; }
    public Firearm PrimaryWeapon { get; private set; }
    public PowerupItem PowerupItem { get; private set; }
    public Throwable ThrowableItem { get; private set; }
    public Weapon CurrentWeaponDrawn { get; private set; }

    private readonly IWeaponsMapper _weaponsMapper;

    public WeaponsData(IPlayer player, IWeaponsMapper weaponsMapper)
    {
        Owner = player;
        _weaponsMapper = weaponsMapper;

        SetMeleeWeapon();
        SetMeleeTempWeapon();
        SetSecondaryWeapon();
        SetPrimaryWeapon();
        SetPowerupItem();
        SetThrowableItem();
        SetCurrentDrawnWeapon();
    }

    public void AddWeapon(Weapon weapon)
    {
        switch (weapon.WeaponItemType)
        {
            case WeaponItemType.Handgun when weapon is Firearm firearm:
                SecondaryWeapon = firearm;
                break;
            case WeaponItemType.Rifle when weapon is Firearm firearm:
                PrimaryWeapon = firearm;
                break;
            case WeaponItemType.Melee when weapon is Melee melee and not MeleeTemp:
                MeleeWeapon = melee;
                break;
            case WeaponItemType.Melee when weapon is MeleeTemp meleeTemp:
                MeleeWeaponTemp = meleeTemp;
                break;
            case WeaponItemType.Powerup when weapon is PowerupItem powerupItem:
                PowerupItem = powerupItem;
                break;
            case WeaponItemType.Thrown when weapon is Throwable throwable:
                ThrowableItem = throwable;
                break;
            default:
                throw new ArgumentException("Invalid weapon for adding to WeaponsData", nameof(weapon));
        }
    }

    public void Update(in UnsafeWeaponsData newData, WeaponItemType weaponItemType, bool isMakeshift = false)
    {
        switch (weaponItemType)
        {
            case WeaponItemType.Melee when isMakeshift:
                UpdateMeleeTemp(newData.MeleeWeaponTemp);
                break;
            case WeaponItemType.Melee when !isMakeshift:
                UpdateMelee(newData.MeleeWeapon);
                break;
            case WeaponItemType.Handgun:
                UpdateSecondary(newData.SecondaryWeapon);
                break;
            case WeaponItemType.Rifle:
                UpdatePrimary(newData.PrimaryWeapon);
                break;
            case WeaponItemType.Thrown:
                UpdateThrowable(newData.ThrowableItem);
                break;
            case WeaponItemType.Powerup:
                UpdatePowerup(newData.PowerupItem);
                break;
            default:
                throw new ArgumentException("Invalid weapon item type", nameof(weaponItemType));
        }

        if (Owner.CurrentWeaponDrawn != CurrentWeaponDrawn.WeaponItemType)
        {
            UpdateDrawn();
        }
    }

    public void UpdateMelee(in UnsafeMelee newData)
    {
        if (newData.WeaponItem != MeleeWeapon.WeaponItem)
        {
            InvalidateWeapon(WeaponItemType.Melee);
        }
        else if (newData != MeleeWeapon)
        {
            MeleeWeapon.Update(newData);
        }
    }

    public void UpdateMeleeTemp(in UnsafeMeleeTemp newData)
    {
        if (newData.WeaponItem != MeleeWeaponTemp.WeaponItem)
        {
            InvalidateWeapon(WeaponItemType.Melee, true);
        }
        else if (newData != MeleeWeaponTemp)
        {
            MeleeWeaponTemp.Update(newData);
        }
    }

    public void UpdateSecondary(in UnsafeFirearm newData)
    {
        if (newData.WeaponItem != SecondaryWeapon.WeaponItem)
        {
            InvalidateWeapon(WeaponItemType.Handgun);
        }
        else if (newData != SecondaryWeapon)
        {
            SecondaryWeapon.Update(newData);
        }
    }

    public void UpdatePrimary(in UnsafeFirearm newData)
    {
        if (newData.WeaponItem != PrimaryWeapon.WeaponItem)
        {
            InvalidateWeapon(WeaponItemType.Rifle);
        }
        else if (newData != PrimaryWeapon)
        {
            PrimaryWeapon.Update(newData);
        }
    }

    public void UpdateThrowable(in UnsafeThrowable newData)
    {
        var isHoldingThrowable = Owner.IsHoldingActiveThrowable;
        if (newData.WeaponItem != ThrowableItem.WeaponItem)
        {
            InvalidateWeapon(WeaponItemType.Thrown);
        }
        else if (newData != ThrowableItem || isHoldingThrowable != ThrowableItem.IsActive)
        {
            ThrowableItem.Update(newData, isHoldingThrowable);
        }
    }

    public void UpdatePowerup(in UnsafePowerup newData)
    {
        if (newData.WeaponItem != PowerupItem.WeaponItem)
        {
            InvalidateWeapon(WeaponItemType.Powerup);
        }
    }

    public void InvalidateWeapon(WeaponItemType weaponItemType, bool isMakeshift = false)
    {
        switch (weaponItemType)
        {
            case WeaponItemType.Melee when isMakeshift:
                SetMeleeTempWeapon();
                break;
            case WeaponItemType.Melee when !isMakeshift:
                SetMeleeWeapon();
                break;
            case WeaponItemType.Handgun:
                SetSecondaryWeapon();
                break;
            case WeaponItemType.Rifle:
                SetPrimaryWeapon();
                break;
            case WeaponItemType.Thrown:
                SetThrowableItem();
                break;
            case WeaponItemType.Powerup:
                SetPowerupItem();
                break;
            default:
                throw new ArgumentException("Invalid weapon item type", nameof(weaponItemType));
        }
    }

    public void UpdateDrawn()
    {
        SetCurrentDrawnWeapon();
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

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Owner.Name);
        stringBuilder.Append(", ");

        foreach (var weapon in this)
        {
            if (weapon.IsInvalid)
                continue;

            stringBuilder.Append(weapon);
            stringBuilder.Append(", ");
        }

        stringBuilder.Append("Weapon drawn: ");
        stringBuilder.Append(CurrentWeaponDrawn.WeaponItem);

        return stringBuilder.ToString();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [MemberNotNull(nameof(MeleeWeapon))]
    private void SetMeleeWeapon()
    {
        var meleeWeapon = Owner.CurrentMeleeWeapon;
        MeleeWeapon = _weaponsMapper.Map<MeleeWeaponItem, Melee>(meleeWeapon, Owner);
    }

    [MemberNotNull(nameof(MeleeWeaponTemp))]
    private void SetMeleeTempWeapon()
    {
        var meleeTempWeapon = Owner.CurrentMeleeMakeshiftWeapon;
        meleeTempWeapon.IsMakeshift = true;
        MeleeWeaponTemp = _weaponsMapper.Map<MeleeWeaponItem, MeleeTemp>(meleeTempWeapon, Owner);
    }

    [MemberNotNull(nameof(SecondaryWeapon))]
    private void SetSecondaryWeapon()
    {
        var secondaryWeapon = Owner.CurrentSecondaryWeapon;
        SecondaryWeapon = _weaponsMapper.Map<HandgunWeaponItem, Firearm>(secondaryWeapon, Owner);
    }

    [MemberNotNull(nameof(PrimaryWeapon))]
    private void SetPrimaryWeapon()
    {
        var primaryWeapon = Owner.CurrentPrimaryWeapon;
        PrimaryWeapon = _weaponsMapper.Map<RifleWeaponItem, Firearm>(primaryWeapon, Owner);
    }

    [MemberNotNull(nameof(PowerupItem))]
    private void SetPowerupItem()
    {
        var powerupItem = Owner.CurrentPowerupItem;
        PowerupItem = _weaponsMapper.Map<PowerupWeaponItem, PowerupItem>(powerupItem, Owner);
    }

    [MemberNotNull(nameof(ThrowableItem))]
    private void SetThrowableItem()
    {
        var throwableItem = Owner.CurrentThrownItem;
        ThrowableItem = _weaponsMapper.Map<ThrownWeaponItem, Throwable>(throwableItem, Owner);
    }

    [MemberNotNull(nameof(CurrentWeaponDrawn))]
    private void SetCurrentDrawnWeapon()
    {
        CurrentWeaponDrawn = GetWeaponByType(Owner.CurrentWeaponDrawn);
    }
}