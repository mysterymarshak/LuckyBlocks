using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups.ThrownItems;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Weapons;

internal class StickyGrenadesLoot : PowerUppedWeaponBase
{
    public override Item Item => Item.StickyGrenades;
    public override string Name => "Sticky grenades";

    protected override WeaponItem WeaponItem => WeaponItem.GRENADES;
    protected override WeaponItemType WeaponItemType => WeaponItemType.Thrown;

    private readonly IPowerupFactory _powerupFactory;
    private readonly IExtendedEvents _extendedEvents;

    public StickyGrenadesLoot(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _powerupFactory = args.PowerupFactory;
        var scope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = scope.Resolve<IExtendedEvents>();
    }

    protected override void OnWeaponCreated(IObjectWeaponItem objectWeaponItem)
    {
        var grenadeIndicator = new GrenadeIndicator(objectWeaponItem, _extendedEvents);
        grenadeIndicator.Paint(StickyGrenades.PaintColor);
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield return _powerupFactory.CreatePowerup(weapon, typeof(StickyGrenades));
    }
}