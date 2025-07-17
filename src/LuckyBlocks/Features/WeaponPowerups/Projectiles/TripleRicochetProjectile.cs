using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Mathematics;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class TripleRicochetProjectile : ProjectilePowerupBase
{
    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public TripleRicochetProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args)
        : base(projectile, extendedEvents, args)
    {
        _game = args.Game;
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new TripleRicochetProjectile(Projectile, ExtendedEvents, _args);
    }

    protected override void OnRunInternal()
    {
        Projectile.PowerupBounceActive = true;
        Projectile.BounceCount = 0;
    }

    protected override void OnHitInternal(ProjectileHitArgs args)
    {
        if (args.IsPlayer)
            return;

        var hitPosition = args.HitPosition;
        var bulletDirection = Projectile.Direction;
        var hitNormal = args.HitNormal;

        var bulletAngle = Math.Abs(MathHelper.PIOver2 - Vector2Helpers.GetAngleBetween(bulletDirection, hitNormal));
        var ricochetDirection = Vector2.Reflect(bulletDirection, hitNormal);
        var rotationAngle = Math.Min(bulletAngle, MathHelper.PIOver2 - bulletAngle) / 2;

        var bullet1Direction = ricochetDirection.Rotate(rotationAngle);
        var bullet2Direction = ricochetDirection.Rotate(-rotationAngle);

        var projectileItem = Projectile.ProjectileItem;
        _game.SpawnProjectile(projectileItem, hitPosition + ricochetDirection, ricochetDirection);
        _game.SpawnProjectile(projectileItem, hitPosition + bullet1Direction, bullet1Direction);
        _game.SpawnProjectile(projectileItem, hitPosition + bullet2Direction, bullet2Direction);

        Dispose();
    }
}