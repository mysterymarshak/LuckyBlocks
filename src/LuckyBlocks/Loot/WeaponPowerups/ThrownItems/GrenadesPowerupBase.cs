using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Notifications;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

internal abstract class GrenadesPowerupBase : IThrowableItemPowerup<Grenade>, IUsablePowerup<Grenade>
{
    public abstract string Name { get; }
    public abstract int UsesCount { get; }
    public int UsesLeft { get; private set; }
    public Grenade? Weapon { get; private set; }

    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;
    private readonly IGame _game;
    private readonly Color _paintColor;
    private readonly IExtendedEvents _extendedEvents;

    protected GrenadesPowerupBase(Grenade grenade, PowerupConstructorArgs args, Color paintColor)
    {
        Weapon = grenade;
        _notificationService = args.NotificationService;
        _mediator = args.Mediator;
        _game = args.Game;
        _paintColor = paintColor;
        var scope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = scope.Resolve<IExtendedEvents>();
    }

    public void OnRan(IPlayer player)
    {
        UsesLeft = UsesCount;
        ShowGrenadesCount(player);
    }

    public void ApplyAgain(IPlayer? player)
    {
        UsesLeft = Math.Min(Weapon!.MaxAmmo, UsesLeft + UsesCount);
        ShowGrenadesCount(player);
    }

    public void OnWeaponPickedUp(IPlayer player)
    {
        ShowGrenadesCount(player);
    }

    public void OnWeaponDropped(IPlayer player, IObjectWeaponItem? objectWeaponItem)
    {
        if (objectWeaponItem is null)
            return;

        CreatePaint(objectWeaponItem, _extendedEvents);
    }

    public void OnThrow(IPlayer player, IObject thrown)
    {
        var grenadeThrown = (thrown as IObjectGrenadeThrown)!;

        var grenade = CreateGrenadeAndPaint(grenadeThrown);
        grenade.Initialize();

        InvalidateWeapon(player);
        UsesLeft = Math.Min(Weapon.CurrentAmmo, UsesLeft - 1);

        ShowGrenadesCount(player);

        if (UsesLeft > 0)
            return;

        var notification = new WeaponPowerupFinishedNotification(this, Weapon!);
        _mediator.Publish(notification);
    }

    [MemberNotNull(nameof(Weapon))]
    public void InvalidateWeapon(IPlayer player)
    {
        var weaponsData = player.GetWeaponsData();
        var grenade = weaponsData.GetWeaponByType(WeaponItemType.Thrown) as Grenade;
        Weapon = grenade ?? new Grenade(WeaponItem.GRENADES, WeaponItemType.Thrown, default, 5, default, default);
    }

    protected abstract GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents);

    protected GrenadeBase CreateGrenadeAndPaint(IObjectGrenadeThrown grenadeThrown)
    {
        var grenade = CreateGrenade(grenadeThrown, _game, _extendedEvents);
        CreatePaint(grenadeThrown, _extendedEvents);

        return grenade;
    }

    private void CreatePaint(IObject @object, IExtendedEvents extendedEvents)
    {
        var grenadeIndicator = new GrenadeIndicator(@object, extendedEvents);
        grenadeIndicator.Paint(_paintColor);
    }

    private void ShowGrenadesCount(IPlayer? player)
    {
        if (player is null)
            return;

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