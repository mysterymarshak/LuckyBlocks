using LuckyBlocks.Data.Args;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class ExplosiveProjectile : ProjectilePowerupBase
{
    protected override float ProjectileSpeedModifier => 1 / 3f;

    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public ExplosiveProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args) :
        base(projectile, extendedEvents, args)
    {
        _game = args.Game;
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new ExplosiveProjectile(Projectile, ExtendedEvents, _args);
    }

    protected override void OnHitInternal(ProjectileHitArgs args)
    {
        _game.TriggerExplosion(args.HitPosition);
        Projectile.FlagForRemoval();
    }
}