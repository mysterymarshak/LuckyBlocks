using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.ThrownItems;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons.Grenades;

internal class StickyGrenadesLoot : PowerUppedWeaponBase
{
    public override Item Item => Item.StickyGrenades;
    public override string Name => "Sticky grenades";

    protected override WeaponItem WeaponItem => WeaponItem.GRENADES;

    private readonly IPowerupFactory _powerupFactory;

    public StickyGrenadesLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, typeof(StickyGrenades));
    }
}