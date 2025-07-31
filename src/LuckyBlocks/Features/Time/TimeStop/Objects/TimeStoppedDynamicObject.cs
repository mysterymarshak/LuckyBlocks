using System;
using LuckyBlocks.Extensions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal abstract class TimeStoppedDynamicObjectBase : ITimeStoppedEntity
{
    public Vector2 Position { get; private set; }

    protected IObject Object { get; }
    protected IGame Game { get; }
    protected IEffectsPlayer EffectsPlayer { get; }
    protected IExtendedEvents ExtendedEvents { get; }
    protected Vector2 LinearVelocity { get; set; }
    protected float AngularVelocity { get; set; }
    protected bool IsBurning { get; set; }

    private float _angle;
    private IEventSubscription? _updateEventSubscription;
    private bool _isTimeResumed;

    protected TimeStoppedDynamicObjectBase(IObject @object, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents) => (Object, Game, EffectsPlayer, ExtendedEvents) =
        (@object, game, effectsPlayer, extendedEvents);

    public void Initialize()
    {
        Position = Object.GetWorldPosition();
        LinearVelocity = Object.GetLinearVelocity();
        AngularVelocity = Object.GetAngularVelocity();
        _angle = Object.GetAngle();
        IsBurning = Object.IsBurning;

        InitializeInternal();

        Object.SetLinearVelocity(Vector2.Zero);
        Object.SetAngularVelocity(0f);

        _updateEventSubscription = ExtendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void ResumeTime()
    {
        if (_isTimeResumed)
            return;

        Dispose();

        _isTimeResumed = true;

        if (!Object.IsValid())
            return;

        ResumeTimeInternal();

        Object.SetWorldPosition(Position);
        Object.SetAngle(_angle);
        Object.SetLinearVelocity(LinearVelocity);
        Object.SetAngularVelocity(AngularVelocity);

        if (IsBurning)
        {
            Object.SetMaxFire();
        }
    }

    protected virtual void InitializeInternal()
    {
    }

    protected virtual void ResumeTimeInternal()
    {
    }

    protected virtual void DisposeInternal()
    {
    }

    protected virtual void OnUpdate()
    {
    }

    private void OnUpdate(Event<float> obj)
    {
        // every-tick position reset works normally only on offline server
        // so BodyType.Static > every-tick reset

        // _object.SetWorldPosition(_position);
        // _object.SetLinearVelocity(Vector2.Zero, true);
        // _object.SetAngularVelocity(0f);
        // _object.SetAngle(_angle);

        OnUpdate();
    }

    private void Dispose()
    {
        _updateEventSubscription?.Dispose();
        DisposeInternal();
    }
}

internal class TimeStoppedDynamicObject : TimeStoppedDynamicObjectBase
{
    private IEventSubscription? _damageEventSubscription;
    private float _delayedDamage;
    private RandomPeriodicTimer? _timer;
    private bool _isMissile;

    public TimeStoppedDynamicObject(IObject @object, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents) : base(@object, game, effectsPlayer, extendedEvents)
    {
    }

    protected override void InitializeInternal()
    {
        _isMissile = Object.IsMissile;
        Object.SetBodyType(BodyType.Static);

        _damageEventSubscription = ExtendedEvents.HookOnDamage(Object, OnDamage, EventHookMode.Default);
    }

    protected override void ResumeTimeInternal()
    {
        Object.SetBodyType(BodyType.Dynamic);
        Object.DealDamage(_delayedDamage);
        Object.TrackAsMissile(_isMissile);
    }

    protected override void DisposeInternal()
    {
        _damageEventSubscription?.Dispose();
        _timer?.Stop();
    }

    private void OnDamage(Event<ObjectDamageArgs> @event)
    {
        var args = @event.Args;

        Object.SetHealth(Object.GetHealth() + args.Damage);

        if (args.DamageType == ObjectDamageType.Fire)
        {
            Object.ClearFire();
            IsBurning = true;

            if (_timer is not null)
                return;

            _timer = new RandomPeriodicTimer(TimeSpan.FromMilliseconds(150), TimeSpan.FromMilliseconds(300),
                TimeBehavior.RealTime, () => EffectsPlayer.PlayEffect(EffectName.FireNodeTrailAir, Position),
                ExtendedEvents);

            return;
        }

        _delayedDamage += args.Damage;

        if (args.DamageType == ObjectDamageType.Player)
        {
            var playerInstance = Game.GetPlayer(args.SourceID);

            LinearVelocity += new Vector2(2 * playerInstance.FacingDirection, 3);
            AngularVelocity += 5f;

            if (Game.GetPlayer(args.SourceID) is { IsKicking: true, IsWalking: true })
            {
                Awaiter.Start(ResumeTime, TimeSpan.Zero);
            }
        }
    }
}