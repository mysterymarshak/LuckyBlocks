using System;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal interface IBullet
{
    event Action<IBullet, ProjectileHitArgs>? Remove;
    event Action<IBullet, ProjectileHitArgs>? Hit;
    IProjectile Projectile { get; }
    void Dispose();
}

internal abstract class BulletBase : IBullet
{
    public event Action<IBullet, ProjectileHitArgs>? Remove;
    public event Action<IBullet, ProjectileHitArgs>? Hit;

    public IProjectile Projectile { get; }
    
    protected abstract float ProjectileSpeedDivider { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private readonly IEventSubscription _hitEventSubscription;

    protected BulletBase(IProjectile projectile, IExtendedEvents extendedEvents)
    {
        Projectile = projectile;
        ExtendedEvents = extendedEvents;
        _hitEventSubscription = ExtendedEvents.HookOnProjectileHit(projectile, OnHit, EventHookMode.Default);
    }

    public void Dispose()
    {
        Projectile.FlagForRemoval();
        _hitEventSubscription.Dispose();
        OnDisposed();
    }

    protected virtual void OnDisposed()
    {
    }

    protected virtual void OnHit(ProjectileHitArgs args)
    {
    }
    
    protected Vector2 GetNewProjectileVelocity() => Projectile switch
    {
        { ProjectileItem: ProjectileItem.BOW or ProjectileItem.GRENADE_LAUNCHER or ProjectileItem.FLAREGUN } =>
            Projectile.Velocity,
        _ => Projectile.Velocity / ProjectileSpeedDivider
    };
        
    private void OnHit(Event<ProjectileHitArgs> @event)
    {
        var args = @event.Args;
        
        OnHit(args);
        Hit?.Invoke(this, args);

        if (args.RemoveFlag)
        {
            Remove?.Invoke(this, args);
        }
    }
}