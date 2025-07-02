using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Notifications;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal abstract class BulletsPowerupBase : IFirearmPowerup, IUsablePowerup<Firearm>
{
    public abstract string Name { get; }
    public virtual int UsesCount => Math.Min(Math.Max(Weapon.MagSize, 3), Weapon.MaxTotalAmmo / 2);
    public Firearm Weapon { get; private set; }
    public int UsesLeft => _usesLeft ??= Math.Min(UsesCount, Weapon.MaxTotalAmmo);
    
    protected IExtendedEvents ExtendedEvents { get; }

    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;
    private readonly ILifetimeScope _lifetimeScope;

    private int? _usesLeft;

    protected BulletsPowerupBase(Firearm firearm, PowerupConstructorArgs args)
    {
        Weapon = firearm;
        _notificationService = args.NotificationService;
        _mediator = args.Mediator;
        _lifetimeScope = args.LifetimeScope.BeginLifetimeScope();
        ExtendedEvents = _lifetimeScope.Resolve<IExtendedEvents>();
    }

    public void OnRan(IPlayer player)
    {
        ShowBulletsCount(player);
    }

    public void ApplyAgain(IPlayer? player)
    {
        _usesLeft = Math.Min(Weapon.MaxTotalAmmo, UsesLeft + UsesCount);
        ShowBulletsCount(player);
    }

    public void OnFire(IPlayer player, IEnumerable<IProjectile> projectiles)
    {
        InvalidateWeapon(player);
        
        _usesLeft = Math.Min(UsesLeft, Weapon.TotalAmmo + projectiles.Count());
        
        foreach (var projectile in projectiles.Take(UsesLeft))
        {
            OnFire(player, projectile);
            _usesLeft--;
        }

        if (UsesLeft <= 0)
        {
            OnFinish(player);
        }

        ShowBulletsCount(player);
    }

    public void OnWeaponPickedUp(IPlayer player)
    {
        ShowBulletsCount(player);
    }

    public virtual void OnWeaponDropped(IPlayer player, IObjectWeaponItem? objectWeaponItem)
    {
    }

    public void InvalidateWeapon(IPlayer player)
    {
        var weaponsData = player.GetWeaponsData();
        Weapon = (weaponsData.GetWeaponByType(Weapon.WeaponItemType) as Firearm)!;
    }
    
    protected abstract void OnFire(IPlayer player, IProjectile projectile);

    protected virtual void OnFinish()
    {
    }
    
    private void OnFinish(IPlayer player)
    {
        _lifetimeScope.Dispose();

        var notification = new WeaponPowerupFinishedNotification(this, Weapon);
        _mediator.Publish(notification);
    }

    private void ShowBulletsCount(IPlayer? player)
    {
        if (player is null)
            return;

        _notificationService.CreateChatNotification($"{UsesLeft} {Name.ToLower()} left for {Weapon.WeaponItem}",
            Color.Grey, player.UserIdentifier);
    }
}