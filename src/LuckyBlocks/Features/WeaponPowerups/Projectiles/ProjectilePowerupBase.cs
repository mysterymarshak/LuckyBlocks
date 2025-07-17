using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal abstract class ProjectilePowerupBase : IProjectilePowerup
{
    public event Action<IProjectilePowerup, ProjectileHitArgs>? ProjectileRemove;
    public event Action<IProjectilePowerup, ProjectileHitArgs>? ProjectileHit;

    public IProjectile Projectile { get; private set; }
    public bool IsCloned { get; private set; }

    protected virtual float ProjectileSpeedModifier => 1f;
    protected IExtendedEvents ExtendedEvents { get; }

    private IEventSubscription? _hitEventSubscription;

    protected ProjectilePowerupBase(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args)
    {
        Projectile = projectile;
        ExtendedEvents = extendedEvents;
    }

    public IProjectilePowerup Clone()
    {
        var clonedPowerup = CloneInternal();
        clonedPowerup.IsCloned = true;
        return clonedPowerup;
    }

    protected abstract ProjectilePowerupBase CloneInternal();

    public void Run()
    {
        _hitEventSubscription = ExtendedEvents.HookOnProjectileHit(Projectile, OnHit, EventHookMode.Default);

        Projectile.Velocity = GetNewProjectileVelocity();

        OnRunInternal();
    }

    public void MoveTo(IProjectile projectile)
    {
        Projectile = projectile;
        Run();
    }

    public void Dispose()
    {
        Projectile.FlagForRemoval();
        _hitEventSubscription?.Dispose();
        OnDisposedInternal();
    }

    protected virtual void OnRunInternal()
    {
    }

    protected virtual void OnHitInternal(ProjectileHitArgs args)
    {
    }

    protected virtual void OnDisposedInternal()
    {
    }

    private Vector2 GetNewProjectileVelocity() => Projectile switch
    {
        { ProjectileItem: ProjectileItem.BOW or ProjectileItem.GRENADE_LAUNCHER or ProjectileItem.FLAREGUN } =>
            Projectile.Velocity,
        _ => Projectile.Velocity * ProjectileSpeedModifier
    };

    private void OnHit(Event<ProjectileHitArgs> @event)
    {
        var args = @event.Args;

        ProjectileHit?.Invoke(this, args);
        OnHitInternal(args);

        if (args.RemoveFlag)
        {
            ProjectileRemove?.Invoke(this, args);
            Dispose();
        }
    }
}