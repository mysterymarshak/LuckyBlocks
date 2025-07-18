using System;
using System.Threading;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop;

internal interface ITimeStopService
{
    TimeSpan TimeStopDelay { get; }
    bool IsTimeStopped { get; }
    void StopTime(TimeSpan duration, IObject relativeObject);
    void ForceResumeTime(Action? timeResumedCallback = null);
}

internal class TimeStopService : ITimeStopService
{
    public TimeSpan TimeStopDelay => _timeStopper.SmoothEntitiesStopDuration;
    public bool IsTimeStopped { get; private set; }

    private readonly IGame _game;
    private readonly ITimeStopper _timeStopper;

    private CancellationTokenSource? _timeStopCts;
    private Action? _timeResumedCallback;

    public TimeStopService(IGame game, ITimeStopper timeStopper) => (_game, _timeStopper) = (game, timeStopper);

    public void StopTime(TimeSpan duration, IObject relativeObject)
    {
        IsTimeStopped = true;
        _game.AutoSpawnSupplyCratesEnabled = false;
        _timeStopCts = _timeStopper.StopTime(duration, relativeObject, timeResumedCallback: OnTimeResumed);
    }

    public void ForceResumeTime(Action? timeResumedCallback = null)
    {
        if (!IsTimeStopped)
            return;

        _timeStopCts?.Cancel();
        _timeResumedCallback = timeResumedCallback;
    }

    private void OnTimeResumed()
    {
        IsTimeStopped = false;
        _game.AutoSpawnSupplyCratesEnabled = true;
        _timeResumedCallback?.Invoke();
    }
}