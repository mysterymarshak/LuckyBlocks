using System;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Mappers;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Reflection;
using SFDGameScriptInterface;

namespace LuckyBlocks.Extensions;

[Inject]
internal static class IObjectWeaponItemExtensions
{
    [InjectWeaponsMapper]
    private static WeaponsMapper WeaponsMapper { get; set; }

    public static Weapon ToWeapon(this IObjectWeaponItem objectWeaponItem)
    {
        var weapon = (Weapon)(objectWeaponItem switch
        {
            { WeaponItemType: WeaponItemType.Melee }
                => WeaponsMapper.Map<MeleeWeaponItem, Melee>(objectWeaponItem.MeleeWeapon, null),
            { WeaponItemType: WeaponItemType.Handgun }
                => WeaponsMapper.Map<HandgunWeaponItem, Firearm>(objectWeaponItem.HandgunWeapon, null),
            { WeaponItemType: WeaponItemType.Rifle }
                => WeaponsMapper.Map<RifleWeaponItem, Firearm>(objectWeaponItem.RifleWeapon, null),
            { WeaponItemType: WeaponItemType.Thrown }
                => WeaponsMapper.Map<ThrownWeaponItem, Throwable>(objectWeaponItem.ThrownWeapon, null),
            { WeaponItemType: WeaponItemType.Powerup }
                => WeaponsMapper.Map<PowerupWeaponItem, PowerupItem>(objectWeaponItem.PowerupItem, null),
            { WeaponItemType: WeaponItemType.InstantPickup }
                => WeaponsMapper.Map<InstantPickupWeaponItem, InstantPickupItem>(objectWeaponItem.InstantPickupItem,
                    null),
            _ => throw new ArgumentOutOfRangeException(nameof(objectWeaponItem), "Unsupported weapon item type.")
        });

        weapon.SetObject(objectWeaponItem.UniqueId);

        return weapon;
    }
}