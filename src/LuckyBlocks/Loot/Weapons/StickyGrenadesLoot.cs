using LuckyBlocks.Data;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.ThrownItems;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class StickyGrenadesLoot : PowerUppedWeaponBase
{
    public override Item Item => Item.StickyGrenades;
    public override string Name => "Sticky grenades";

    protected override WeaponItem WeaponItem => WeaponItem.GRENADES;
    protected override WeaponItemType WeaponItemType => WeaponItemType.Thrown;

    private readonly IPowerupFactory _powerupFactory;

    public StickyGrenadesLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
        => (_powerupFactory) = (args.PowerupFactory);

    protected override void OnWeaponCreated(IObjectWeaponItem weaponItem)
    {
        StickyGrenades.CreateGlue(weaponItem, ExtendedEvents);
    }

    protected override IWeaponPowerup<Weapon> GetPowerup(Weapon weapon)
    {
        return _powerupFactory.CreatePowerup(weapon, typeof(StickyGrenades));
    }
}