﻿using System;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using Serilog;

namespace LuckyBlocks.Utils.Timers;

internal class Timer : TimerBase
{
    protected override Action TickCallback { get; }

    private readonly Action _callback;

    public Timer(TimeSpan interval, TimeBehavior timeBehavior, Action callback, IExtendedEvents extendedEvents) : base(interval, timeBehavior, extendedEvents)
        => (_callback, TickCallback) = (callback, OnTicked);

    public override void Start()
    {
        StartInternal();
    }

    public override void Stop()
    {
        StopInternal();
    }

    public override void Reset()
    {
        ResetInternal();
    }

    private void OnTicked()
    {
        _callback.Invoke();
    }
}

[Inject]
internal abstract class TimerBase
{
    public float ElapsedFromPreviousTick => _elapsed;
    public TimeSpan Interval { get; private set; }

    public TimeSpan TimeLeft =>
        _isFinished ? TimeSpan.Zero : TimeSpan.FromMilliseconds(Interval.TotalMilliseconds - _elapsed);

    protected abstract Action TickCallback { get; }
    
    [InjectTimeProvider]
    protected static ITimeProvider TimeProvider { get; set; }

    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly TimeBehavior _timeBehavior;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Action<Event<float>> _cachedUpdateCallback;

    private IEventSubscription? _updateEventSubscription;
    private bool _isFinished;
    private float _elapsed;

    public TimerBase(TimeSpan interval, TimeBehavior timeBehavior, IExtendedEvents extendedEvents)
        => (Interval, _timeBehavior, _extendedEvents, _cachedUpdateCallback) =
            (interval, timeBehavior, extendedEvents, OnUpdate);

    public abstract void Start();
    public abstract void Stop();
    public abstract void Reset();

    protected void StartInternal()
    {
        if (_isFinished)
        {
            throw new InvalidOperationException("timer is finished");
        }

        _updateEventSubscription = _extendedEvents.HookOnUpdate(_cachedUpdateCallback, EventHookMode.Default);
    }

    protected void StopInternal()
    {
        if (_isFinished)
            return;

        _isFinished = true;
        _updateEventSubscription?.Dispose();
    }

    protected void ResetInternal()
    {
        _isFinished = false;
        _elapsed = default;
    }

    protected void SetInterval(TimeSpan interval)
    {
        if (_isFinished)
        {
            throw new InvalidOperationException("timer is started");
        }

        Interval = interval;
    }

    private void OnUpdate(Event<float> @event)
    {
        if (_isFinished)
            return;

        var elapsed = @event.Args;
        _elapsed += GetElapsed(elapsed);

        if (_elapsed >= Interval.TotalMilliseconds)
        {
            try
            {
                StopInternal();
                TickCallback.Invoke();
            }
            catch (Exception exception)
            {
                Logger.Error(exception, "unexpected exception in TimerBase.OnUpdate");
            }
        }
    }

    private float GetElapsed(float realElapsed)
    {
        if (_timeBehavior.HasFlag<TimeBehavior>(TimeBehavior.RealTime))
        {
            return realElapsed;
        }

        if (_timeBehavior.HasFlag<TimeBehavior>(TimeBehavior.TimeModifier))
        {
            if (_timeBehavior.HasFlag<TimeBehavior>(TimeBehavior.IgnoreTimeStop))
            {
                return TimeProvider.GameSlowMoModifier * realElapsed;
            }
            
            return TimeProvider.TimeModifier * realElapsed;
        }

        throw new ArgumentOutOfRangeException();
    }
}