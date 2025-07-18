using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.ThrownItems;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.Grenades;

internal class BananaGrenadesLoot : PowerUppedWeaponBase
{
    public override Item Item => Item.BananaGrenades;
    public override string Name => "Banana grenades";

    protected override WeaponItem WeaponItem => WeaponItem.GRENADES;

    private readonly IPowerupFactory _powerupFactory;

    public BananaGrenadesLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, typeof(BananaGrenades));
    }
}