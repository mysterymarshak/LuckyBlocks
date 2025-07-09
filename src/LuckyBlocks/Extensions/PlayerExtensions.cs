using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Data.Weapons.Unsafe;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Loot.Buffs;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

internal static class PlayerExtensions
{
    public static bool IsValid(this Player player)
    {
        return player.Instance?.IsValid() ?? false;
    }

    public static bool HasBuff(this Player player, Type buffType) => HasBuff(player, buffType, out _);

    public static bool HasAnyOfBuffs(this Player player, IEnumerable<Type> buffTypes,
        IEnumerable<Type>? exclusions = null)
        => buffTypes.Any(x =>
            HasBuff(player, x, out var buff) && exclusions?.Any(y => y.IsInstanceOfType(buff)) == false);

    public static ImmunityFlag GetImmunityFlags(this Player player)
    {
        var immunities = player.Immunities.ToList();

        if (immunities.Count == 0)
        {
            return ImmunityFlag.None;
        }

        return immunities
            .Select(x => x.Flag)
            .Aggregate((x, y) => x | y);
    }

    public static bool HasAnyWeapon(this Player player)
    {
        var weaponsData = player.WeaponsData;
        return weaponsData.HasAnyWeapon();
    }

    public static void UpdateWeaponData(this Player player, WeaponItemType weaponItemType, bool isMakeshift = false,
        bool updateDrawn = false)
    {
        var weaponsData = new UnsafeWeaponsData(player.Instance!, weaponItemType);
        player.WeaponsData.Update(weaponsData, weaponItemType, isMakeshift, updateDrawn);
    }

    public static void SetWeapons(this Player player, WeaponsData weaponsData, bool forceSet = false)
    {
        var playerInstance = player.Instance!;

        if (forceSet)
        {
            if (!weaponsData.MeleeWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeapon.WeaponItem);
                playerInstance.SetCurrentMeleeDurability(weaponsData.MeleeWeapon.CurrentDurability);
            }

            if (!weaponsData.MeleeWeaponTemp.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeaponTemp.WeaponItem);
                playerInstance.SetCurrentMeleeMakeshiftDurability(weaponsData.MeleeWeaponTemp.CurrentDurability);
            }

            if (!weaponsData.SecondaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.SecondaryWeapon.WeaponItem);
                playerInstance.SetAmmoFromWeapon(weaponsData.SecondaryWeapon);
            }

            if (!weaponsData.PrimaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PrimaryWeapon.WeaponItem);
                playerInstance.SetAmmoFromWeapon(weaponsData.PrimaryWeapon);
            }

            if (!weaponsData.ThrowableItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.ThrowableItem.WeaponItem);
                playerInstance.SetAmmoFromWeapon(weaponsData.ThrowableItem);
            }

            if (!weaponsData.PowerupItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PowerupItem.WeaponItem);
            }

            player.SetWeaponsData(weaponsData);

            return;
        }

        var currentWeaponsData = player.WeaponsData;

        if (currentWeaponsData.MeleeWeapon != weaponsData.MeleeWeapon)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.MeleeWeapon.WeaponItemType);

            if (!weaponsData.MeleeWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeapon.WeaponItem);
                playerInstance.SetCurrentMeleeDurability(weaponsData.MeleeWeapon.CurrentDurability);
            }
        }

        if (currentWeaponsData.MeleeWeaponTemp != weaponsData.MeleeWeaponTemp)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.MeleeWeaponTemp.WeaponItemType);

            if (!weaponsData.MeleeWeaponTemp.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.MeleeWeaponTemp.WeaponItem);
                playerInstance.SetCurrentMeleeMakeshiftDurability(weaponsData.MeleeWeaponTemp.CurrentDurability);
            }
        }

        if (currentWeaponsData.SecondaryWeapon != weaponsData.SecondaryWeapon)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.SecondaryWeapon.WeaponItemType);

            if (!weaponsData.SecondaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.SecondaryWeapon.WeaponItem);
                playerInstance.SetCurrentSecondaryWeaponAmmo(weaponsData.SecondaryWeapon.CurrentAmmo,
                    weaponsData.SecondaryWeapon.CurrentSpareMags,
                    weaponsData.SecondaryWeapon.ProjectilePowerupData.ProjectilePowerup);
            }
        }

        if (currentWeaponsData.PrimaryWeapon != weaponsData.PrimaryWeapon)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.PrimaryWeapon.WeaponItemType);

            if (!weaponsData.PrimaryWeapon.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PrimaryWeapon.WeaponItem);
                playerInstance.SetCurrentPrimaryWeaponAmmo(weaponsData.PrimaryWeapon.CurrentAmmo,
                    weaponsData.PrimaryWeapon.CurrentSpareMags,
                    weaponsData.PrimaryWeapon.ProjectilePowerupData.ProjectilePowerup);
            }
        }

        if (currentWeaponsData.ThrowableItem != weaponsData.ThrowableItem)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.ThrowableItem.WeaponItemType);

            if (!weaponsData.ThrowableItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.ThrowableItem.WeaponItem);
                playerInstance.SetCurrentThrownItemAmmo(weaponsData.ThrowableItem.CurrentAmmo);
            }
        }

        if (currentWeaponsData.PowerupItem != weaponsData.PowerupItem)
        {
            playerInstance.RemoveWeaponItemType(currentWeaponsData.PowerupItem.WeaponItemType);

            if (!weaponsData.PowerupItem.IsInvalid)
            {
                playerInstance.GiveWeaponItem(weaponsData.PowerupItem.WeaponItem);
            }
        }
    }

    private static bool HasBuff(this Player player, Type buffType, out IFinishableBuff? existingBuff)
    {
        var buffs = player.Buffs;

        foreach (var buff in buffs)
        {
            if (buffType.IsInstanceOfType(buff))
            {
                existingBuff = buff;
                return true;
            }
        }

        existingBuff = null;
        return false;
    }
}