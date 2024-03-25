using LuckyBlocks.Data;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class InfiniteRicochetBullets : BulletsPowerupBase
{
    public override string Name => "Infinite ricochet bullets";
    public override int UsesCount => Weapon.TotalAmmo;

    public InfiniteRicochetBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
    }

    protected override void OnFire(IPlayer player, IProjectile projectile)
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