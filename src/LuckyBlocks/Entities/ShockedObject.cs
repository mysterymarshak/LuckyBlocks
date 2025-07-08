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
    public const double ELEMENTARY_CHARGE = 50;

    public double Charge => TimeLeft.TotalMilliseconds;
    public int ObjectId => _object.UniqueId;
    public TimeSpan TimeLeft { get; set; }

    private bool IsShocked { get; set; }

    private readonly IObject _object;
    private readonly TimeSpan _shockDuration;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly IMediator _mediator;
    private readonly PeriodicTimer<ShockedObject> _periodicTimer;
    private readonly PeriodicTimer<ShockedObject> _effectsTimer;
    private readonly IReadOnlyList<Vector2> _collisionVectors;

    public ShockedObject(IObject @object, TimeSpan shockDuration, IEffectsPlayer effectsPlayer, IGame game,
        IMediator mediator, IExtendedEvents extendedEvents)
    {
        _object = @object;
        _shockDuration = shockDuration;
        _effectsPlayer = effectsPlayer;
        _game = game;
        _mediator = mediator;
        _periodicTimer = new PeriodicTimer<ShockedObject>(TimeSpan.Zero, TimeBehavior.TimeModifier, x => x.OnUpdate(),
            x => x.Charge <= ELEMENTARY_CHARGE, x => x.Dispose(), this, extendedEvents);
        _effectsTimer = new PeriodicTimer<ShockedObject>(TimeSpan.FromMilliseconds(300),
            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop,
            x => x._effectsPlayer.PlayEffect(EffectName.Electric, _object.GetWorldPosition()),
            shockedObject => !shockedObject.IsShocked, null, this, extendedEvents);
        _collisionVectors = Enumerable.Range(0, 8)
            .Select(x => x * 45)
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
        _effectsTimer.Start();
    }

    private void OnUpdate()
    {
        TimeLeft = TimeSpan.FromMilliseconds(Math.Max(0,
            TimeLeft.TotalMilliseconds - _periodicTimer.ElapsedFromPreviousTick));

        var position = _object.GetWorldPosition();
        var touchedObjects = _collisionVectors
            .Select(x => _game.RayCast(position, position + x, default))
            .SelectMany(x => x)
            .Where(x => x.Hit && x.HitObject.GetBodyType() != BodyType.Static &&
                        x.HitObject.GetPhysicsLayer() == PhysicsLayer.Active)
            .Select(x => x.HitObject)
            .ToList();

        foreach (var collisionVector in _collisionVectors)
        {
            _game.DrawLine(position, position + collisionVector, Color.Red);
        }

        if (touchedObjects.Count == 0)
            return;

        var notification = new ObjectsTouchedShockObjectNotification(this, touchedObjects);
        _mediator.Publish(notification);
    }

    private void Dispose()
    {
        IsShocked = false;

        _periodicTimer.Stop();
        _effectsTimer.Stop();

        var notification = new ShockObjectEndedNotification(_object);
        _mediator.Publish(notification);
    }
}