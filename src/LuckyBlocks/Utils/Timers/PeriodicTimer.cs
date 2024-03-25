using System;
using System.Threading;
using LuckyBlocks.Extensions;

namespace LuckyBlocks.Utils.Timers;

internal class PeriodicTimer<T> : TimerBase
{
    protected override Action TickCallback { get; }

    private readonly Action<T> _callback;
    private readonly Func<T, bool> _finishCondition;
    private readonly Action<T>? _finishCallback;
    private readonly T _arg;

    private bool _isFinished;

    public PeriodicTimer(TimeSpan period, TimeBehavior timeBehavior, Action<T> callback, Func<T, bool> finishCondition,
        Action<T>? finishCallback, T arg, IExtendedEvents extendedEvents) : base(period, timeBehavior, extendedEvents)
        => (_callback, _finishCondition, _finishCallback, _arg, TickCallback) =
            (callback, finishCondition, finishCallback, arg, OnTicked);

    public override void Start()
    {
        if (_isFinished)
        {
            throw new InvalidOperationException("timer is finished");
        }

        StartInternal();
    }

    public override void Stop()
    {
        _isFinished = true;
        StopInternal();
    }

    public override void Reset()
    {
        _isFinished = false;
        ResetInternal();
    }

    private void OnTicked()
    {
        if (_finishCondition(_arg))
        {
            _finishCallback?.Invoke(_arg);
            _isFinished = true;
            return;
        }

        _callback.Invoke(_arg);

        ResetInternal();

        StartInternal();
    }
}

internal class PeriodicTimer : TimerBase
{
    protected override Action TickCallback { get; }

    private readonly Action _callback;
    private readonly Action? _finishCallback;
    private readonly int _repeatsCount;
    private readonly TimeBehavior _timeBehavior;

    private int _iteration;
    private bool _isFinished;
    private CancellationToken _cancellationToken;
    private CancellationTokenRegistration _ctr;

    public PeriodicTimer(TimeSpan period, TimeBehavior timeBehavior, Action callback, Action? finishCallback, int repeatsCount, IExtendedEvents extendedEvents, CancellationToken cancellationToken = default) : base(period, timeBehavior, extendedEvents)
        => (_callback, _finishCallback, _repeatsCount, _timeBehavior, _cancellationToken, TickCallback) = (callback, finishCallback, repeatsCount, timeBehavior, cancellationToken, OnTicked);

    public override void Start()
    {
        if (_isFinished)
        {
            throw new InvalidOperationException("timer is finished");
        }

        _ctr = _cancellationToken.Register(Stop);
        _iteration = 1;
        StartInternal();
    }

    public override void Reset()
    {
        _isFinished = false;
        _iteration = default;
        ResetInternal();
    }

    public override void Stop()
    {
        _isFinished = true;
        _ctr.Dispose();
        StopInternal();
    }

    private void OnTicked()
    {
        if (_isFinished)
            return;

        if (!(TimeProvider.IsTimeStopped &&
              _timeBehavior.HasFlag<TimeBehavior>(TimeBehavior.TicksInTimeStopDoesntAffectToIterationsCount)))
        {
            _iteration++;
        }

        _callback.Invoke();

        if (_iteration > _repeatsCount)
        {
            _isFinished = true;
            _finishCallback?.Invoke();
            return;
        }

        ResetInternal();
        StartInternal();
    }
}