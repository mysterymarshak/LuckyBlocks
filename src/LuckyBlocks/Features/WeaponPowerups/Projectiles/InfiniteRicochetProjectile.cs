using LuckyBlocks.Data.Args;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class InfiniteRicochetProjectile : ProjectilePowerupBase
{
    private readonly PowerupConstructorArgs _args;

    public InfiniteRicochetProjectile(IProjectile projectile, IExtendedEvents extendedEvents,
        PowerupConstructorArgs args) : base(projectile, extendedEvents, args)
    {
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new InfiniteRicochetProjectile(Projectile, ExtendedEvents, _args);
    }

    protected override void OnRunInternal()
    {
        Projectile.PowerupBounceActive = true;
    }

    protected override void OnHitInternal(ProjectileHitArgs args)
    {
        if (args.RemoveFlag)
            return;

        Projectile.BounceCount = 0;
    }
}