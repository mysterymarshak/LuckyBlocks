using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Bullets;

internal class InfiniteRicochetBullets : BulletsPowerupBase
{
    public override string Name => "Infinite ricochet bullets";

    public override int UsesCount => Math.Max(base.UsesCount,
        (Weapon is Shotgun ? (Weapon.TotalAmmo / 2) : Weapon.TotalAmmo));

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;
    protected override bool CountShotgunBulletsIndependently => false;

    private static readonly List<Type> _incompatiblePowerups =
        [typeof(ExplosiveBullets), typeof(TripleRicochetBullets)];

    private readonly IProjectilesService _projectilesService;
    private readonly PowerupConstructorArgs _args;

    public InfiniteRicochetBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _projectilesService = args.ProjectilesService;
        _args = args;
    }

    public override IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new InfiniteRicochetBullets(firearm, _args) { UsesLeft = UsesLeft };
    }

    protected override void OnFireInternal(IPlayer playerInstance, IProjectile projectile)
    {
        _projectilesService.AddPowerup<InfiniteRicochetProjectile>(projectile, _args);
    }
}