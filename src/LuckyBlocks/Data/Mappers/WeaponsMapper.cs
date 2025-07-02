using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Mappers;

internal interface IWeaponsMapper
{
    T2 Map<T1, T2>(T1 source, IPlayer player) where T1 : struct where T2 : Weapon;
}

internal class WeaponsMapper : IWeaponsMapper
{
    public T2 Map<T1, T2>(T1 source, IPlayer player) where T1 : struct where T2 : Weapon => (T2)(Weapon)(source switch
    {
        MeleeWeaponItem { IsMakeshift: true } meleeMakeshiftWeaponItem => new MeleeTemp(
            meleeMakeshiftWeaponItem.WeaponItem, meleeMakeshiftWeaponItem.WeaponItemType,
            meleeMakeshiftWeaponItem.Durability),
        MeleeWeaponItem meleeWeaponItem => new Melee(meleeWeaponItem.WeaponItem, meleeWeaponItem.WeaponItemType,
            meleeWeaponItem.Durability),
        HandgunWeaponItem handgunWeaponItem => new Firearm(handgunWeaponItem.WeaponItem,
            handgunWeaponItem.WeaponItemType, handgunWeaponItem.CurrentAmmo, handgunWeaponItem.MaxTotalAmmo,
            handgunWeaponItem.MagSize, handgunWeaponItem.SpareMags, handgunWeaponItem.MaxCarriedSpareMags,
            handgunWeaponItem.LazerEquipped,
            ProjectilePowerupData.FromBouncingAndFireRounds(handgunWeaponItem.PowerupBouncingRounds,
                handgunWeaponItem.PowerupFireRounds), handgunWeaponItem.ProjectileItem),
        RifleWeaponItem { WeaponItem: WeaponItem.FLAMETHROWER } flamethrower => new Flamethrower(
            flamethrower.WeaponItem, flamethrower.WeaponItemType, flamethrower.CurrentAmmo, flamethrower.MaxTotalAmmo,
            flamethrower.MagSize, flamethrower.SpareMags, flamethrower.MaxCarriedSpareMags, flamethrower.LazerEquipped,
            ProjectilePowerupData.Empty, ProjectileItem.NONE),
        RifleWeaponItem { WeaponMagCapacity: > 1 } shotgun => new Shotgun(shotgun.WeaponItem, shotgun.WeaponItemType,
            shotgun.CurrentAmmo, shotgun.MaxTotalAmmo, shotgun.MagSize, shotgun.SpareMags, shotgun.MaxCarriedSpareMags,
            shotgun.LazerEquipped,
            ProjectilePowerupData.FromBouncingAndFireRounds(shotgun.PowerupBouncingRounds, shotgun.PowerupFireRounds),
            shotgun.ProjectileItem, shotgun.WeaponMagCapacity),
        RifleWeaponItem rifleWeaponItem => new Firearm(rifleWeaponItem.WeaponItem, rifleWeaponItem.WeaponItemType,
            rifleWeaponItem.CurrentAmmo, rifleWeaponItem.MaxTotalAmmo, rifleWeaponItem.MagSize,
            rifleWeaponItem.SpareMags, rifleWeaponItem.MaxCarriedSpareMags, rifleWeaponItem.LazerEquipped,
            ProjectilePowerupData.FromBouncingAndFireRounds(rifleWeaponItem.PowerupBouncingRounds,
                rifleWeaponItem.PowerupFireRounds), rifleWeaponItem.ProjectileItem),
        PowerupWeaponItem powerupWeaponItem => new PowerupItem(powerupWeaponItem.WeaponItem,
            powerupWeaponItem.WeaponItemType),
        ThrownWeaponItem { WeaponItem: WeaponItem.GRENADES } grenade => new Grenade(grenade.WeaponItem,
            grenade.WeaponItemType, grenade.CurrentAmmo, grenade.MaxCarriedAmmo, player.IsHoldingActiveThrowable,
            player.GetActiveThrowableTimer()),
        ThrownWeaponItem thrownWeaponItem => new Throwable(thrownWeaponItem.WeaponItem, thrownWeaponItem.WeaponItemType,
            thrownWeaponItem.CurrentAmmo, thrownWeaponItem.MaxCarriedAmmo, player.IsHoldingActiveThrowable),
        _ => throw new ArgumentOutOfRangeException()
    });
}