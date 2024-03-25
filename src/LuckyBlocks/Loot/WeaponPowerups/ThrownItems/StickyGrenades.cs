using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Notifications;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

internal class StickyGrenades : IThrowableItemPowerup<Grenade>, IUsablePowerup<Grenade>
{
    public string Name => "Sticky grenades";
    public int UsesCount => 3;
    public int UsesLeft { get; private set; }
    public Grenade? Weapon { get; private set; }

    private static readonly IReadOnlyList<Vector2> GluePattern = new List<Vector2>
    {
        new(0, 3),
        new(0, 2), new(1, 2), new(2, 2)
    };
    
    private readonly INotificationService _notificationService;
    private readonly IMediator _mediator;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;

    public StickyGrenades(Grenade grenade, PowerupConstructorArgs args)
        => (Weapon, _notificationService, _mediator, _game, UsesLeft, _extendedEvents) =
            (grenade, args.NotificationService, args.Mediator, args.Game, UsesCount,
                args.LifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>());

    public void OnRan(IPlayer player)
    {
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
        
        CreateGlue(objectWeaponItem, _extendedEvents);
    }

    public void OnThrow(IPlayer player, IObject thrown)
    {
        var grenadeThrown = (thrown as IObjectGrenadeThrown)!;

        var stickyGrenade = new StickyGrenade(grenadeThrown, _game, _extendedEvents);
        stickyGrenade.Init();
        CreateGlue(grenadeThrown, _extendedEvents);

        var weaponsData = player.GetWeaponsData();
        var grenade = weaponsData.GetWeaponByType(WeaponItemType.Thrown) as Grenade;
        Weapon = grenade ?? new Grenade(WeaponItem.GRENADES, WeaponItemType.Thrown, default, 5, default, default);

        UsesLeft = Math.Min(grenade?.CurrentAmmo ?? 0, UsesLeft - 1);

        ShowGrenadesCount(player);

        if (UsesLeft > 0)
            return;

        var notification = new WeaponPowerupFinishedNotification(this, Weapon!);
        _mediator.Publish(notification);
    }
    
    public static void CreateGlue(IObject grenade, IExtendedEvents extendedEvents)
    {
        var glueDrawer = new TextureDrawer(grenade, autoDisposeOnDestroy: true, extendedEvents);
        glueDrawer.Draw(GluePattern, ExtendedColors.Pink,
            direction => direction == 1 ? new Vector2(-1, 0) : Vector2.Zero);
    }

    private void ShowGrenadesCount(IPlayer? player)
    {
        if (player is null)
            return;

        _notificationService.CreateChatNotification($"{UsesLeft} sticky grenades left", Color.Grey,
            player.UserIdentifier);
    }

    private class StickyGrenade
    {
        private const int COLLISION_VECTOR_LENGTH = 2;

        private static readonly IReadOnlyList<Vector2> CollisionVectors = Enumerable.Range(0, 360)
            .Where(x => x % 10 == 0)
            .Select(x => x * Math.PI / 180)
            .Select(x => new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) * COLLISION_VECTOR_LENGTH)
            .ToList();

        private readonly IObjectGrenadeThrown _grenade;
        private readonly IGame _game;
        private readonly IExtendedEvents _extendedEvents;

        private IObjectWeldJoint? _objectWeldJoint;
        private Events.UpdateCallback? _collisionCheckCallback;
        private Events.PlayerDamageCallback? _playerDamageCallback;
        private IEventSubscription? _updateEventSubscription;

        public StickyGrenade(IObjectGrenadeThrown grenade, IGame game, IExtendedEvents extendedEvents)
            => (_grenade, _game, _extendedEvents) = (grenade, game, extendedEvents);

        public void Init()
        {
            _collisionCheckCallback = Events.UpdateCallback.Start(OnCollisionCheckCallback);
            _playerDamageCallback = Events.PlayerDamageCallback.Start(OnPlayerDamaged);
            _updateEventSubscription = _extendedEvents.HookOnDestroyed(_grenade, Dispose, EventHookMode.Default);
            _grenade.SetDudChance(0f);
        }

        private void OnCollisionCheckCallback(float e)
        {
            var position = _grenade.GetWorldPosition();
            var raycastInput = new RayCastInput
            {
                ClosestHitOnly = true, IncludeOverlap = false, FilterOnMaskBits = true,
                MaskBits = ushort.MaxValue ^ 0b100
            };

            var raycastResults =
                CollisionVectors.Select(x => _game.RayCast(position, position + x, raycastInput).First());
            var result = raycastResults
                .Where(x => x.Hit)
                .OrderBy(x => (x.Position - position).Length())
                .FirstOrDefault();

            if (!result.Hit)
                return;

            var @object = _game.GetObject(result.ObjectID);
            StartFollowing(@object);
        }

        private void OnPlayerDamaged(IPlayer player, PlayerDamageArgs args)
        {
            if (args.DamageType != PlayerDamageEventType.Missile)
                return;

            if (_grenade.UniqueId != args.SourceID)
                return;

            StartFollowing(player);
        }

        private void StartFollowing(IObject @object)
        {
            _objectWeldJoint = (_game.CreateObject("WeldJoint") as IObjectWeldJoint)!;
            _objectWeldJoint.AddTargetObject(@object);
            _objectWeldJoint.AddTargetObject(_grenade);
        }

        private void Dispose(Event @event)
        {
            _playerDamageCallback?.Stop();
            _collisionCheckCallback?.Stop();
            _objectWeldJoint?.RemoveDelayed();
            _extendedEvents.Clear();
            _updateEventSubscription?.Dispose();
        }
    }
}