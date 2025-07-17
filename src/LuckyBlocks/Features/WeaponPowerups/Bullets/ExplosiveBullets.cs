using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Bullets;

internal class ExplosiveBullets : BulletsPowerupBase
{
    public override string Name => "Explosive bullets";

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups =
        [typeof(TripleRicochetBullets), typeof(InfiniteRicochetBullets)];

    private readonly IProjectilesService _projectilesService;
    private readonly PowerupConstructorArgs _args;

    public ExplosiveBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _projectilesService = args.ProjectilesService;
        _args = args;
    }

    public override IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new ExplosiveBullets(firearm, _args) { UsesLeft = UsesLeft };
    }

    protected override void OnFireInternal(IPlayer playerInstance, IProjectile projectile)
    {
        _projectilesService.AddPowerup<ExplosiveProjectile>(projectile, _args);
    }
}