using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.WeaponPowerups.Grenades;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.ThrownItems;

internal class BananaGrenades : GrenadesPowerupBase
{
    public override Color PaintColor => Color.Yellow;
    public override string Name => "Banana grenades";
    public override int UsesCount => 3;

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(StickyGrenades)];

    private readonly Func<IObjectGrenadeThrown, BananaGrenade> _createGrenadeDelegate;
    private readonly PowerupConstructorArgs _args;

    public BananaGrenades(Grenade grenade, PowerupConstructorArgs args) : base(grenade, args)
    {
        _createGrenadeDelegate = grenadeThrown => (BananaGrenade)CreateGrenade(grenadeThrown);
        _args = args;
    }

    public override IWeaponPowerup<Grenade> Clone(Weapon weapon)
    {
        var grenade = weapon as Grenade;
        ArgumentWasNullException.ThrowIfNull(grenade);
        return new BananaGrenades(grenade, _args) { UsesLeft = UsesLeft };
    }

    protected override GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents, Action<IObject, IExtendedEvents> createPaintDelegate)
    {
        return new BananaGrenade(grenadeThrown, game, extendedEvents, createPaintDelegate, _createGrenadeDelegate);
    }
}