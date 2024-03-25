using System;
using LuckyBlocks.Extensions;

namespace LuckyBlocks.Utils.Timers;

internal readonly record struct DynamicPeriodicTimerTickArgs(int StepIndex, int StepsCount);

internal class DynamicPeriodicTimer : TimerBase
{
    public bool IsFinished { get; private set; }

    protected override Action TickCallback { get; }

    private readonly TimeSpan _start;
    private readonly int _stepsCount;
    private readonly double _step;
    private readonly Action? _callback;
    private readonly Action<DynamicPeriodicTimerTickArgs>? _callbackWithArgs;
    private readonly Action? _finishCallback;

    private int _currentStepIndex;

    private DynamicPeriodicTimer(TimeSpan startPeriod, TimeSpan endPeriod, TimeSpan total, Action? finishCallback,
        TimeBehavior timeBehavior, IExtendedEvents extendedEvents) : base(startPeriod, timeBehavior, extendedEvents)
    {
        var differenceRaw = endPeriod.TotalMilliseconds - startPeriod.TotalMilliseconds;
        var differenceTimeSpan = TimeSpan.FromMilliseconds(Math.Abs(differenceRaw));
        _start = startPeriod;
        _stepsCount = Convert.ToInt32(Math.Round(total.Divide(differenceTimeSpan)));
        _step = differenceTimeSpan.Divide(_stepsCount).TotalMilliseconds * Math.Sign(differenceRaw);
        _finishCallback = finishCallback;
        TickCallback = OnTicked;
    }

    public DynamicPeriodicTimer(TimeSpan startPeriod, TimeSpan endPeriod, TimeSpan total, Action callback,
        Action? finishCallback, TimeBehavior timeBehavior, IExtendedEvents extendedEvents) : this(startPeriod, endPeriod, total,
        finishCallback, timeBehavior, extendedEvents)
    {
        _callback = callback;
    }

    public DynamicPeriodicTimer(TimeSpan startPeriod, TimeSpan endPeriod, TimeSpan total,
        Action<DynamicPeriodicTimerTickArgs> callback, Action? finishCallback,
        TimeBehavior timeBehavior, IExtendedEvents extendedEvents) : this(startPeriod, endPeriod, total, finishCallback, timeBehavior, extendedEvents)
    {
        _callbackWithArgs = callback;
    }

    public override void Start()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException("timer is finished");
        }

        _currentStepIndex = 1;
        StartInternal();
    }

    public override void Stop()
    {
        IsFinished = true;
        StopInternal();
    }

    public override void Reset()
    {
        _currentStepIndex = default;
        IsFinished = false;
    }

    private void OnTicked()
    {
        _currentStepIndex++;

        _callback?.Invoke();
        _callbackWithArgs?.Invoke(new DynamicPeriodicTimerTickArgs(_currentStepIndex - 1, _stepsCount));

        if (_currentStepIndex > _stepsCount)
        {
            IsFinished = true;
            _finishCallback?.Invoke();
            return;
        }

        ResetInternal();
        
        SetInterval(TimeSpan.FromMilliseconds(_start.TotalMilliseconds + _step * _currentStepIndex));
        StartInternal();
    }
}