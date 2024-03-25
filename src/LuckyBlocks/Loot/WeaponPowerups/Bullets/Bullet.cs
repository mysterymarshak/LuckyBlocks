using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class Bullet : BulletBase
{
    protected override float ProjectileSpeedDivider => 1;
    
    public Bullet(IProjectile projectile, IExtendedEvents extendedEvents) : base(projectile, extendedEvents)
    {
    }
}