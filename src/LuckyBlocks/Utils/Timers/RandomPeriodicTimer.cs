using System;

namespace LuckyBlocks.Utils.Timers;

internal class RandomPeriodicTimer : TimerBase
{
    protected override Action TickCallback { get; }

    private readonly TimeSpan _minInterval;
    private readonly TimeSpan _maxInterval;
    private readonly Action _callback;

    public RandomPeriodicTimer(TimeSpan minInterval, TimeSpan maxInterval, TimeBehavior timeBehavior, Action callback,
        IExtendedEvents extendedEvents) : base(default, timeBehavior, extendedEvents)
        => (_minInterval, _maxInterval, _callback, TickCallback) = (minInterval, maxInterval, callback, OnTicked);

    public override void Start()
    {
        UpdateInterval();
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

    private void UpdateInterval()
    {
        var interval = TimeSpan.FromMilliseconds(SharedRandom.Instance.Next((int)_minInterval.TotalMilliseconds,
            (int)_maxInterval.TotalMilliseconds));
        SetInterval(interval);
    }

    private void OnTicked()
    {
        _callback.Invoke();

        ResetInternal();

        UpdateInterval();
        StartInternal();
    }
}