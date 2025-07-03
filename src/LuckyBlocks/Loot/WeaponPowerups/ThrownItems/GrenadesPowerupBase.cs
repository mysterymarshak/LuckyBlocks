using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Notifications;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

internal abstract class GrenadesPowerupBase : IUsablePowerup<Grenade>
{
    public abstract Color PaintColor { get; }
    public abstract string Name { get; }
    public abstract int UsesCount { get; }
    public Grenade Weapon { get; private set; }
    public int UsesLeft  => _usesLeft ??= Math.Min(UsesCount, Weapon.MaxAmmo);
    
    protected abstract IEnumerable<Type> IncompatiblePowerups { get; }

    private IPlayer? Player => Weapon.Owner;

    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;

    private int? _usesLeft;

    protected GrenadesPowerupBase(Grenade grenade, PowerupConstructorArgs args)
    {
        Weapon = grenade;
        _notificationService = args.NotificationService;
        _mediator = args.Mediator;
        _game = args.Game;
        var thisScope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Run()
    {
        Weapon.PickUp += ShowGrenadesCount;
        Weapon.Draw += ShowGrenadesCount;
        Weapon.Throw += OnWeaponDropped;
        Weapon.Drop += OnWeaponDropped;
        Weapon.GrenadeThrow += OnThrown;
        
        ShowGrenadesCount(ignoreIfDropped: true);

        if (Weapon.IsDropped)
        {
            CreatePaint(Weapon.ObjectId, _extendedEvents);
        }
    }
    
    public bool IsCompatibleWith(Type otherPowerupType) => !IncompatiblePowerups.Contains(otherPowerupType);

    public void AddUses(int usesCount)
    {
        _usesLeft = Math.Min(Weapon.MaxAmmo, UsesLeft + usesCount);

        ShowGrenadesCount(ignoreIfDropped: true);
    }

    public void MoveToWeapon(Weapon otherWeapon)
    {
        if (otherWeapon is not Grenade grenade)
        {
            throw new InvalidCastException("cannot cast otherWeapon to grenade");
        }
        
        Weapon = grenade;
        Run();
    }
    
    public void Dispose()
    {
        Weapon.PickUp -= ShowGrenadesCount;
        Weapon.Draw -= ShowGrenadesCount;
        Weapon.Throw -= OnWeaponDropped;
        Weapon.Drop -= OnWeaponDropped;
        Weapon.GrenadeThrow -= OnThrown;
    }
    
    private void ShowGrenadesCount(Weapon weapon)
    {
        ShowGrenadesCount();
    }

    private void OnWeaponDropped(IObjectWeaponItem? objectWeaponItem, Weapon weapon)
    {
        ArgumentWasNullException.ThrowIfNull(objectWeaponItem);
        CreatePaint(objectWeaponItem, _extendedEvents);
    }

    private void OnThrown(IPlayer? player, IObject? thrown, Throwable? throwable)
    {
        var grenadeThrown = thrown as IObjectGrenadeThrown;
        ArgumentWasNullException.ThrowIfNull(grenadeThrown);
        ArgumentWasNullException.ThrowIfNull(player);
        ArgumentWasNullException.ThrowIfNull(throwable);

        var grenade = CreateGrenadeAndPaint(grenadeThrown);
        grenade.Initialize();

        _usesLeft = Math.Min(Weapon.CurrentAmmo, UsesLeft - 1);
        ShowGrenadesCount(player);

        if (UsesLeft > 0)
            return;

        var notification = new WeaponPowerupFinishedNotification(this, Weapon);
        _mediator.Publish(notification);
    }

    protected abstract GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents);

    protected GrenadeBase CreateGrenadeAndPaint(IObjectGrenadeThrown grenadeThrown)
    {
        var grenade = CreateGrenade(grenadeThrown, _game, _extendedEvents);
        CreatePaint(grenadeThrown, _extendedEvents);

        return grenade;
    }

    private void CreatePaint(int objectId, IExtendedEvents extendedEvents) =>
        CreatePaint(_game.GetObject(objectId), extendedEvents);

    private void CreatePaint(IObject @object, IExtendedEvents extendedEvents)
    {
        var grenadeIndicator = new GrenadeIndicator(@object, extendedEvents);
        grenadeIndicator.Paint(PaintColor);
    }

    private void ShowGrenadesCount(IPlayer? player = null, bool ignoreIfDropped = false)
    {
        player ??= Player;
        if (player is null && ignoreIfDropped)
            return;

        ArgumentWasNullException.ThrowIfNull(player);

        _notificationService.CreateChatNotification($"{UsesLeft} {Name.ToLower()} left", Color.Grey,
            player.UserIdentifier);
    }

    protected abstract class GrenadeBase
    {
        private readonly IObjectGrenadeThrown _grenade;
        private readonly IExtendedEvents _extendedEvents;

        private IEventSubscription? _disposeEventSubscription;

        protected GrenadeBase(IObjectGrenadeThrown grenade, IExtendedEvents extendedEvents)
        {
            _grenade = grenade;
            _extendedEvents = extendedEvents;
        }

        public virtual void Initialize()
        {
            _disposeEventSubscription =
                _extendedEvents.HookOnDestroyed(_grenade, _ => Dispose(), EventHookMode.Default);
            _grenade.SetDudChance(0f);
        }

        protected virtual void Dispose()
        {
            _disposeEventSubscription?.Dispose();
        }
    }
}