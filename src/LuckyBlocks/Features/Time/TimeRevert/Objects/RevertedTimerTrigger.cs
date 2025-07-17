using System.Collections.Generic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert.Objects;

internal class RevertedTimerTrigger : RevertedTrigger
{
    private readonly int _interval;
    private readonly int _repeatsCount;
    private readonly bool _isTimerRunning;

    public RevertedTimerTrigger(IObjectTimerTrigger timerTrigger) : base(timerTrigger)
    {
        _interval = timerTrigger.GetIntervalTime();
        _repeatsCount = timerTrigger.GetRepeatCount();
        _isTimerRunning = timerTrigger.IsTimerRunning;
    }

    protected override void RestoreInternal(IGame game, Dictionary<int, int>? objectsMap)
    {
        base.RestoreInternal(game, objectsMap);

        var timerTrigger = (IObjectTimerTrigger)Object;

        if (_interval != timerTrigger.GetIntervalTime())
        {
            timerTrigger.SetIntervalTime(_interval);
        }

        if (_repeatsCount != timerTrigger.GetRepeatCount())
        {
            timerTrigger.SetRepeatCount(_repeatsCount);
        }

        if (timerTrigger.IsTimerRunning && !_isTimerRunning)
        {
            timerTrigger.StopTimer();
        }
        else if (!timerTrigger.IsTimerRunning && _isTimerRunning)
        {
            timerTrigger.StartTimer();
        }
    }
}