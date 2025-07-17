using System;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal interface IProjectilePowerup
{
    event Action<IProjectilePowerup, ProjectileHitArgs>? ProjectileRemove;
    event Action<IProjectilePowerup, ProjectileHitArgs>? ProjectileHit;
    IProjectile Projectile { get; }
    bool IsCloned { get; }
    IProjectilePowerup Clone();
    void Run();
    void MoveTo(IProjectile projectile);
    void Dispose();
}