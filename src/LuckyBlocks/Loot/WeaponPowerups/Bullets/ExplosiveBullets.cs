using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class ExplosiveBullets : BulletsPowerupBase
{
    public override string Name => "Explosive bullets";

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups =
        [typeof(TripleRicochetBullets), typeof(InfiniteRicochetBullets)];

    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public ExplosiveBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _game = args.Game;
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
        var explosiveBullet = new ExplosiveBullet(projectile, ExtendedEvents);
        explosiveBullet.Hit += OnBulletHit;
    }

    private void OnBulletHit(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Hit -= OnBulletHit;
        bullet.Dispose();

        _game.TriggerExplosion(args.HitPosition);
    }

    private class ExplosiveBullet : BulletBase
    {
        protected override float ProjectileSpeedDivider => 3;

        public ExplosiveBullet(IProjectile projectile, IExtendedEvents extendedEvents) : base(projectile,
            extendedEvents)
        {
            projectile.Velocity = GetNewProjectileVelocity();
        }
    }
}