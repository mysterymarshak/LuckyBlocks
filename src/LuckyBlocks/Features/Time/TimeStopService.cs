using System;
using System.Threading;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time;

internal interface ITimeStopService
{
    TimeSpan TimeStopDuration { get; }
    TimeSpan TimeStopDelay { get; }
    bool IsTimeStopped { get; }
    void StopTime(IObject relativeObject);
    void ForceResumeTime();
}

internal class TimeStopService : ITimeStopService
{
    public TimeSpan TimeStopDuration => TimeSpan.FromSeconds(7);
    public TimeSpan TimeStopDelay => _timeStopper.SmoothEntitiesStopDuration;
    public bool IsTimeStopped { get; private set; }

    private readonly IGame _game;
    private readonly ITimeStopper _timeStopper;

    private CancellationTokenSource? _timeStopCts;

    public TimeStopService(IGame game, ITimeStopper timeStopper) => (_game, _timeStopper) = (game, timeStopper);

    public void StopTime(IObject relativeObject)
    {
        IsTimeStopped = true;
        _game.AutoSpawnSupplyCratesEnabled = false;
        _timeStopCts = _timeStopper.StopTime(TimeStopDuration, relativeObject, timeResumedCallback: OnTimeResumed);
    }

    public void ForceResumeTime()
    {
        if (!IsTimeStopped)
            return;

        _timeStopCts?.Cancel();
    }

    private void OnTimeResumed()
    {
        IsTimeStopped = false;
        _game.AutoSpawnSupplyCratesEnabled = true;
    }
}