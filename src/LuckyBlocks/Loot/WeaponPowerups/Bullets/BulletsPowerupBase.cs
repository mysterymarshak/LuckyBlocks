using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Notifications;
using LuckyBlocks.Reflection;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal abstract class BulletsPowerupBase : IUsablePowerup<Firearm>
{
    public abstract string Name { get; }
    public virtual int UsesCount => Math.Min(Math.Max(Weapon.MagSize, 3), Weapon.MaxTotalAmmo / 2);
    public Firearm Weapon { get; private set; }

    public int UsesLeft
    {
        get => _usesLeft ??= Math.Min(UsesCount, Weapon.MaxTotalAmmo);
        protected init => _usesLeft = value;
    }

    protected abstract IEnumerable<Type> IncompatiblePowerups { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private IPlayer? Player => Weapon.Owner;

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

    public abstract IWeaponPowerup<Firearm> Clone(Weapon weapon);

    public void Run()
    {
        Weapon.Fire += OnFired;
        Weapon.PickUp += ShowBulletsCount;
        Weapon.Draw += ShowBulletsCount;

        ShowBulletsCount(ignoreIfDropped: true);
    }

    public bool IsCompatibleWith(Type otherPowerupType) => !IncompatiblePowerups.Contains(otherPowerupType);

    public void AddUses(int usesCount)
    {
        _usesLeft = Math.Min(Weapon.MaxTotalAmmo, UsesLeft + usesCount);

        ShowBulletsCount(ignoreIfDropped: true);
    }

    public void MoveToWeapon(Weapon otherWeapon)
    {
        if (otherWeapon is not Firearm firearm)
        {
            throw new InvalidCastException("cannot cast weapon to firearm");
        }

        Weapon = firearm;
        Run();
    }

    public void Dispose()
    {
        Weapon.Fire -= OnFired;
        Weapon.PickUp -= ShowBulletsCount;
        Weapon.Draw -= ShowBulletsCount;
    }

    protected abstract void OnFired(IPlayer player, IProjectile projectile);

    protected virtual void OnFinish()
    {
        _lifetimeScope.Dispose();

        var notification = new WeaponPowerupFinishedNotification(this, Weapon);
        _mediator.Publish(notification);
    }

    private void OnFired(IPlayer? player, IEnumerable<IProjectile>? projectiles)
    {
        ArgumentWasNullException.ThrowIfNull(player);
        ArgumentWasNullException.ThrowIfNull(projectiles);

        _usesLeft = Math.Min(UsesLeft, Weapon.TotalAmmo + projectiles.Count());

        foreach (var projectile in projectiles.Take(UsesLeft))
        {
            OnFired(player, projectile);
            _usesLeft--;
        }

        if (UsesLeft <= 0)
        {
            OnFinish();
        }

        ShowBulletsCount(player);
    }

    private void ShowBulletsCount(Weapon weapon)
    {
        ShowBulletsCount();
    }

    private void ShowBulletsCount(IPlayer? player = null, bool ignoreIfDropped = false)
    {
        player ??= Player;
        if (player is null && ignoreIfDropped)
            return;

        ArgumentWasNullException.ThrowIfNull(player);

        var message = Weapon.IsDrawn
            ? $"{UsesLeft} {Name.ToLower()} left"
            : $"You picked up {UsesLeft} {Name.ToLower()} for {Weapon.WeaponItem}!";

        _notificationService.CreateChatNotification(message, Color.Grey, player.UserIdentifier);
    }
}