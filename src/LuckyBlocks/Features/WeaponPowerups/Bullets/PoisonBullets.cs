using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Bullets;

internal class PoisonBullets : BulletsPowerupBase
{
    public static readonly IReadOnlyCollection<Type> IncompatiblePowerups =
        [typeof(PushBullets), typeof(TripleRicochetBullets)];

    public override string Name => "Poison bullets";

    protected override IReadOnlyCollection<Type> IncompatiblePowerupsInternal => IncompatiblePowerups;

    private readonly IProjectilesService _projectilesService;
    private readonly PowerupConstructorArgs _args;

    public PoisonBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _projectilesService = args.ProjectilesService;
        _args = args;
    }

    public override IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new PoisonBullets(firearm, _args) { UsesLeft = UsesLeft };
    }

    protected override void OnFireInternal(IPlayer playerInstance, IProjectile projectile)
    {
        _projectilesService.AddPowerup<PoisonProjectile>(projectile, _args);
    }
}