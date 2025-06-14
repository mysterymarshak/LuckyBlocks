using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Mined;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.LegendaryWeapon;

internal class LegendaryWeaponMined : PowerUppedWeaponBase
{
    public override Item Item => Item.LegendaryWeapon;
    public override string Name => "Legendary weapon";

    protected override WeaponItem WeaponItem => _legendaryWeapon.WeaponItem;
    protected override WeaponItemType WeaponItemType => _legendaryWeapon.WeaponItemType;

    private readonly WeaponWithType _legendaryWeapon;
    private readonly IPowerupFactory _powerupFactory;

    public LegendaryWeaponMined(WeaponWithType weaponWithType, Vector2 spawnPosition,
        LootConstructorArgs args) : base(spawnPosition, args)
        => (_legendaryWeapon, _powerupFactory) = (weaponWithType, args.PowerupFactory);

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon,
            _legendaryWeapon.WeaponItemType == WeaponItemType.Melee
                ? typeof(MinedMeleePowerup)
                : typeof(MinedFirearmPowerup));
    }
}