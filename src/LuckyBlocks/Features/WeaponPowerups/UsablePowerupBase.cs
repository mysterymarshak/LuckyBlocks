using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Mediator;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups;

internal abstract class UsablePowerupBase<T> : IUsablePowerup<T> where T : Weapon
{
    public abstract string Name { get; }
    public abstract T Weapon { get; protected set; }
    public abstract int UsesCount { get; }
    public abstract int UsesLeft { get; protected set; }

    protected abstract IEnumerable<Type> IncompatiblePowerups { get; }
    protected IExtendedEvents ExtendedEvents { get; }

    private IPlayer? Player => Weapon.Owner;

    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;
    private readonly ILifetimeScope _lifetimeScope;

    protected UsablePowerupBase(PowerupConstructorArgs args)
    {
        _notificationService = args.NotificationService;
        _mediator = args.Mediator;
        _lifetimeScope = args.LifetimeScope.BeginLifetimeScope();
        ExtendedEvents = _lifetimeScope.Resolve<IExtendedEvents>();
    }

    public void Run()
    {
        if (Weapon is Firearm firearm)
        {
            firearm.Fire += OnUseFirearm;
        }
        else if (Weapon is Throwable throwable)
        {
            throwable.GrenadeThrow += OnUseThrowable;
        }

        Weapon.PickUp += OnPickUp;
        Weapon.Draw += OnDraw;

        ShowUsesLeft(PowerupEvent.Run, ignoreIfDropped: true);
        OnRunInternal();
    }

    public void Dispose()
    {
        if (Weapon is Firearm firearm)
        {
            firearm.Fire -= OnUseFirearm;
        }
        else if (Weapon is Throwable throwable)
        {
            throwable.GrenadeThrow -= OnUseThrowable;
        }

        Weapon.PickUp -= OnPickUp;
        Weapon.Draw -= OnDraw;

        OnDisposeInternal();
    }

    public void Stack(IStackablePowerup<Weapon> powerup)
    {
        var usablePowerup = powerup as IUsablePowerup<T>;
        ArgumentWasNullException.ThrowIfNull(usablePowerup);

        var usesCount = usablePowerup.UsesCount;
        var maxAmmo =
            Weapon switch
            {
                Firearm firearm => firearm.MaxTotalAmmo,
                Throwable throwable => throwable.MaxAmmo,
                _ => throw new InvalidOperationException()
            };

        var oldUsesLeft = UsesLeft;
        UsesLeft = Math.Min(maxAmmo, UsesLeft + usesCount);

        ShowUsesLeft(UsesLeft > oldUsesLeft ? PowerupEvent.AddUses : PowerupEvent.PickUpWhenMaxAmmo,
            ignoreIfDropped: true);
    }

    public void MoveToWeapon(Weapon otherWeapon)
    {
        if (otherWeapon is not T weapon)
        {
            throw new InvalidCastException($"cannot cast weapon to {typeof(T)}");
        }

        Weapon = weapon;
        Run();
    }

    public bool IsCompatibleWith(Type otherPowerupType) => !IncompatiblePowerups.Contains(otherPowerupType);

    public abstract IWeaponPowerup<T> Clone(Weapon copiedWeapon);

    protected virtual void OnRunInternal()
    {
    }

    protected virtual void OnDisposeInternal()
    {
    }

    protected virtual void OnFireInternal(IPlayer player, IEnumerable<IProjectile> projectilesEnumerable)
    {
    }

    protected virtual void OnThrowInternal(IPlayer? playerInstance, IObject? objectThrown, Throwable? throwable)
    {
    }

    private void ShowUsesLeft(PowerupEvent powerupEvent, IPlayer? player = null, bool ignoreIfDropped = false)
    {
        player ??= Player;
        if (player is null && ignoreIfDropped)
            return;

        ArgumentWasNullException.ThrowIfNull(player);

        var message = powerupEvent switch
        {
            PowerupEvent.Run => $"You picked up {UsesLeft} {Name.ToLower()} for {Weapon.WeaponItem}!",
            PowerupEvent.PickUp => $"You picked up {Weapon.WeaponItem} with {UsesLeft} {Name.ToLower()}!",
            PowerupEvent.Draw or PowerupEvent.FirearmUse or PowerupEvent.GrenadeUse =>
                $"{UsesLeft} {Name.ToLower()} left",
            PowerupEvent.AddUses => $"{Name} count was increased to {UsesLeft} for {Weapon.WeaponItem}",
            PowerupEvent.PickUpWhenMaxAmmo =>
                $"You already have maximum {UsesLeft} {Name.ToLower()} for {Weapon.WeaponItem}",
            _ => throw new ArgumentOutOfRangeException(nameof(powerupEvent), powerupEvent, null)
        };

        _notificationService.CreateChatNotification(message, Color.Grey, player.UserIdentifier);
    }

    private void OnDraw(Weapon weapon)
    {
        ShowUsesLeft(PowerupEvent.Draw);
    }

    private void OnPickUp(Weapon weapon, IPlayer playerInstance)
    {
        ShowUsesLeft(PowerupEvent.PickUp, playerInstance);
    }

    private void OnUseThrowable(IPlayer? playerInstance, IObject? objectThrown, Throwable? throwable)
    {
        OnThrowInternal(playerInstance, objectThrown, throwable);

        if (UsesLeft <= 0)
        {
            OnFinish();
        }

        ShowUsesLeft(PowerupEvent.GrenadeUse, playerInstance);
    }

    private void OnUseFirearm(Weapon weapon, IPlayer playerInstance, IEnumerable<IProjectile> projectiles)
    {
        OnFireInternal(playerInstance, projectiles);

        if (UsesLeft <= 0)
        {
            OnFinish();
        }

        ShowUsesLeft(PowerupEvent.FirearmUse, playerInstance);
    }

    private void OnFinish()
    {
        _lifetimeScope.Dispose();

        var notification = new WeaponPowerupFinishedNotification(this, Weapon);
        _mediator.Publish(notification);
    }

    private enum PowerupEvent
    {
        None,

        Run,

        PickUp,

        Draw,

        FirearmUse,

        GrenadeUse,

        AddUses,

        PickUpWhenMaxAmmo
    }
}