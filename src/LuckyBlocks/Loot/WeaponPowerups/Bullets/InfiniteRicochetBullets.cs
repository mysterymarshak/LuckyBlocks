using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class InfiniteRicochetBullets : BulletsPowerupBase
{
    public override string Name => "Infinite ricochet bullets";
    public override int UsesCount => Weapon.TotalAmmo;

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(ExplosiveBullets), typeof(TripleRicochetBullets)];
    
    public InfiniteRicochetBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
    }

    protected override void OnFired(IPlayer player, IProjectile projectile)
    {
        var bullet = new Bullet(projectile, ExtendedEvents);
        bullet.Hit += OnBulletHit;
        bullet.Remove += OnBulletRemoved;

        projectile.PowerupBounceActive = true;
    }

    private void OnBulletRemoved(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Hit -= OnBulletHit;
        bullet.Remove -= OnBulletRemoved;
        bullet.Dispose();
    }

    private void OnBulletHit(IBullet bullet, ProjectileHitArgs args)
    {
        if (args.RemoveFlag)
            return;

        var projectile = bullet.Projectile;
        projectile.BounceCount = 0;
    }
}