using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using LuckyBlocks.Data.Weapons.Unsafe;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed class WeaponsData : IEnumerable<Weapon>
{
    public IPlayer Owner { get; set; }
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
        SetThrowableItem();
        SetPowerupItem();
        SetCurrentDrawnWeapon();
    }

    public Weapon this[int index]
    {
        get
        {
            return index switch
            {
                0 => MeleeWeaponTemp,
                1 => MeleeWeapon,
                2 => SecondaryWeapon,
                3 => PrimaryWeapon,
                4 => ThrowableItem,
                5 => PowerupItem,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }
    }

    public void AddWeapon(Weapon weapon)
    {
        switch (weapon.WeaponItemType)
        {
            case WeaponItemType.Melee when weapon is MeleeTemp meleeTemp:
                MeleeWeaponTemp = meleeTemp;
                break;
            case WeaponItemType.Melee when weapon is Melee melee:
                MeleeWeapon = melee;
                break;
            case WeaponItemType.Handgun when weapon is Firearm firearm:
                SecondaryWeapon = firearm;
                break;
            case WeaponItemType.Rifle when weapon is Firearm firearm:
                PrimaryWeapon = firearm;
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

    public void Update(in UnsafeWeaponsData newData, WeaponItemType weaponItemType, bool isMakeshift, bool updateDrawn)
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

        if (updateDrawn && Owner.CurrentWeaponDrawn != CurrentWeaponDrawn.WeaponItemType)
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

    public Weapon GetWeaponByType(WeaponItemType weaponItemType, bool isMakeshift) => weaponItemType switch
    {
        WeaponItemType.Handgun => SecondaryWeapon,
        WeaponItemType.Rifle => PrimaryWeapon,
        WeaponItemType.Melee => isMakeshift ? MeleeWeaponTemp : MeleeWeapon,
        WeaponItemType.Powerup => PowerupItem,
        WeaponItemType.Thrown => ThrowableItem,
        _ => Weapon.Empty
    };

    IEnumerator<Weapon> IEnumerable<Weapon>.GetEnumerator() => GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public WeaponsDataEnumerator GetEnumerator() => GetEnumerator(false);

    public WeaponsDataEnumerator GetEnumerator(bool ignoreInvalid)
    {
        return new WeaponsDataEnumerator(this, ignoreInvalid);
    }

    public override string ToString()
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(Owner.Name);
        stringBuilder.Append(", ");

        foreach (var weapon in this)
        {
            stringBuilder.Append(weapon);
            stringBuilder.Append(" (formatted: ");
            stringBuilder.Append(weapon.GetFormattedName());
            stringBuilder.Append(')');
            stringBuilder.Append(", ");
        }

        stringBuilder.Append("Weapon drawn: ");
        stringBuilder.Append(CurrentWeaponDrawn.WeaponItem);
        stringBuilder.Append(" (");
        stringBuilder.Append(CurrentWeaponDrawn.GetType().Name);
        stringBuilder.Append(")");

        return stringBuilder.ToString();
    }

    public void Dispose()
    {
        foreach (var weapon in this)
        {
            weapon.RaiseEvent(WeaponEvent.Disposed);
        }
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
        CurrentWeaponDrawn = GetWeaponByType(Owner.CurrentWeaponDrawn,
            Owner.CurrentMeleeMakeshiftWeapon.WeaponItem != WeaponItem.NONE);
    }

    public struct WeaponsDataEnumerator : IEnumerator<Weapon>
    {
        public Weapon Current { get; private set; } = null!;

        object? IEnumerator.Current => Current;

        private readonly Range _indexRange = new(0, 5);
        private readonly WeaponsData _weaponsData;
        private readonly bool _ignoreInvalid;

        private int _index;

        public WeaponsDataEnumerator(WeaponsData weaponsData, bool ignoreInvalid = false)
        {
            _weaponsData = weaponsData;
            _ignoreInvalid = ignoreInvalid;
            _index = 0;
        }

        public bool MoveNext()
        {
            if (_index > _indexRange.End.Value)
                return false;

            do
            {
                Current = _weaponsData[_index];
                _index++;

                if (Current is { IsInvalid: false } || _ignoreInvalid)
                    return true;
            } while (_index <= _indexRange.End.Value);

            return false;
        }

        public void Reset()
        {
            _index = 0;
            Current = null;
        }

        public void Dispose()
        {
        }
    }
}