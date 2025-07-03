using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Mathematics;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class TripleRicochetBullets : BulletsPowerupBase
{
    public override string Name => "Triple ricochet bullets";

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(ExplosiveBullets), typeof(FreezeBullets), typeof(InfiniteRicochetBullets), typeof(AimBullets), typeof(PushBullets)];
    
    private readonly IGame _game;

    public TripleRicochetBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
        => (_game) = (args.Game);

    protected override void OnFired(IPlayer player, IProjectile projectile)
    {
        var explosiveBullet = new Bullet(projectile, ExtendedEvents);
        explosiveBullet.Hit += OnBulletHit;
        
        projectile.PowerupBounceActive = true;
        projectile.BounceCount = 0;
    }

    private void OnBulletHit(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Hit -= OnBulletHit;
        bullet.Dispose();

        if (args.IsPlayer)
            return;

        var projectile = bullet.Projectile;
        var hitPosition = args.HitPosition;
        var bulletDirection = projectile.Direction;
        var hitNormal = args.HitNormal;

        var bulletAngle = Math.Abs(MathHelper.PIOver2 - Vector2Helpers.GetAngleBetween(bulletDirection, hitNormal));
        var ricochetDirection = Vector2.Reflect(bulletDirection, hitNormal);
        var rotationAngle = Math.Min(bulletAngle, MathHelper.PIOver2 - bulletAngle) / 2;

        var bullet1Direction = ricochetDirection.Rotate(rotationAngle);
        var bullet2Direction = ricochetDirection.Rotate(-rotationAngle);

        var projectileItem = projectile.ProjectileItem;
        _game.SpawnProjectile(projectileItem, hitPosition + ricochetDirection, ricochetDirection);
        _game.SpawnProjectile(projectileItem, hitPosition + bullet1Direction, bullet1Direction);
        _game.SpawnProjectile(projectileItem, hitPosition + bullet2Direction, bullet2Direction);
    }
}