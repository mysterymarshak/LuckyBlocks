using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.Melees;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class FlamyKatanaLoot : PowerUppedWeaponBase
{
    public override Item Item => Item.FlamyKatana;
    public override string Name => "Flamy katana";

    protected override WeaponItem WeaponItem => WeaponItem.KATANA;

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