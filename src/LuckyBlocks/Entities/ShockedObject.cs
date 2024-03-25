using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Extensions;
using LuckyBlocks.Notifications;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities;

internal class ShockedObject
{
    public const double MinCharge = 100;

    public double Charge => TimeLeft.TotalMilliseconds;
    public int ObjectId => _object.UniqueId;
    public TimeSpan TimeLeft { get; set; }

    private bool IsShocked { get; set; }

    private readonly IObject _object;
    private readonly TimeSpan _shockDuration;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly IMediator _mediator;
    private readonly IExtendedEvents _extendedEvents;
    private readonly PeriodicTimer<ShockedObject> _periodicTimer;
    private readonly IReadOnlyList<Vector2> _collisionVectors;

    public ShockedObject(IObject @object, TimeSpan shockDuration, IEffectsPlayer effectsPlayer, IGame game,
        IMediator mediator, ILifetimeScope lifetimeScope)
    {
        _object = @object;
        _shockDuration = shockDuration;
        _lifetimeScope = lifetimeScope;
        _effectsPlayer = effectsPlayer;
        _game = game;
        _mediator = mediator;
        _extendedEvents = lifetimeScope.Resolve<IExtendedEvents>();
        _periodicTimer = new(TimeSpan.Zero, TimeBehavior.TimeModifier,
            _ => OnUpdate(), _ => Charge <= MinCharge, _ => Dispose(), this, _extendedEvents);
        _collisionVectors = Enumerable.Range(0, 360)
            .Where(x => x % 45 == 0)
            .Select(x => x * Math.PI / 180)
            .Select(x =>
                new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) *
                (@object.GetAABB().GetDiagonalLength() / 2 + 3))
            .ToList();
    }

    public void Initialize()
    {
        IsShocked = true;
        TimeLeft = _shockDuration;

        _periodicTimer.Start();

        AddEffects();
    }

    private void OnUpdate()
    {
        TimeLeft = TimeSpan.FromMilliseconds(TimeLeft.TotalMilliseconds - _periodicTimer.ElapsedFromPreviousTick);

        var position = _object.GetWorldPosition();
        var raycastResults =
            _collisionVectors.Select(x => _game.RayCast(position, position + x, default));
        var touchedObjects = raycastResults
            .SelectMany(x => x)
            .Where(x => x.Hit)
            .Where(x => x.HitObject.GetBodyType() != BodyType.Static &&
                        x.HitObject.GetPhysicsLayer() == PhysicsLayer.Active)
            .Select(x => x.HitObject)
            .ToList();

        if (!touchedObjects.Any())
            return;

        var notification = new ObjectsTouchedShockObjectNotification(this, touchedObjects);
        _mediator.Publish(notification);
    }

    private void AddEffects()
    {
        var period = TimeSpan.FromMilliseconds(300);
        var timer = new PeriodicTimer<ShockedObject>(period, TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop,
            _ => _effectsPlayer.PlayEffect(EffectName.Electric, _object.GetWorldPosition()),
            shockedObject => !shockedObject.IsShocked, default, this, _extendedEvents);
        timer.Start();
    }

    private void Dispose()
    {
        IsShocked = false;
        _lifetimeScope.Dispose();
        _extendedEvents.Clear();

        var notification = new ShockObjectEndedNotification(_object);
        _mediator.Publish(notification);
    }
}