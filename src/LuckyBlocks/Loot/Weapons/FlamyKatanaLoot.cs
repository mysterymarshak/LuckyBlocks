using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.Melees;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class FlamyKatanaLoot : PowerUppedWeaponBase
{
    public override Item Item => Item.FlamyKatana;
    public override string Name => "Flamy katana";

    protected override WeaponItem WeaponItem => WeaponItem.KATANA;
    protected override WeaponItemType WeaponItemType => WeaponItemType.Melee;

    private readonly IPowerupFactory _powerupFactory;

    public FlamyKatanaLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, typeof(FlamyKatana));
    }
}