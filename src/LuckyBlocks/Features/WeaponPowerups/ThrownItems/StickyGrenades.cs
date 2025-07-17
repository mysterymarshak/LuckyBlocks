using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.WeaponPowerups.Grenades;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.ThrownItems;

internal class StickyGrenades : GrenadesPowerupBase
{
    public override Color PaintColor => ExtendedColors.Pink;
    public override string Name => "Sticky grenades";
    public override int UsesCount => 3;

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(BananaGrenades)];

    private readonly PowerupConstructorArgs _args;

    public StickyGrenades(Grenade grenade, PowerupConstructorArgs args) : base(grenade, args)
    {
        _args = args;
    }

    public override IWeaponPowerup<Grenade> Clone(Weapon weapon)
    {
        var grenade = weapon as Grenade;
        ArgumentWasNullException.ThrowIfNull(grenade);
        return new StickyGrenades(grenade, _args) { UsesLeft = UsesLeft };
    }

    protected override GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents, Action<IObject, IExtendedEvents> createPaintDelegate)
    {
        return new StickyGrenade(grenadeThrown, game, extendedEvents, createPaintDelegate);
    }
}