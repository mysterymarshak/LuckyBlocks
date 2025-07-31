using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Features.Time;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Keyboard.States;

internal class AllKeysPressedStrategy : IStateHandleStrategy
{
    public IEnumerable<WatchingVirtualKey> WatchingKeys { get; }

    private readonly TimeSpan _resetCooldown;
    private readonly ITimeProvider _timeProvider;

    private bool _isWaitingForRelease;
    private float _lastTriggerTime;

    public AllKeysPressedStrategy(IEnumerable<WatchingVirtualKey> watchingKeys, TimeSpan resetCooldown,
        ITimeProvider timeProvider)
    {
        WatchingKeys = watchingKeys;
        _resetCooldown = resetCooldown;
        _timeProvider = timeProvider;
    }

    public bool Handle()
    {
        var allPressed = WatchingKeys.All(x => x.State == VirtualKeyEvent.Pressed);

        if (_isWaitingForRelease)
        {
            if (allPressed == false)
            {
                _isWaitingForRelease = false;
            }

            return false;
        }

        var currentTime = _timeProvider.ElapsedRealTime;
        if (TimeSpan.FromMilliseconds(currentTime - _lastTriggerTime) < _resetCooldown)
        {
            return false;
        }

        if (allPressed)
        {
            _lastTriggerTime = currentTime;
            _isWaitingForRelease = true;
            return true;
        }

        return false;
    }

    public void ResetCooldown()
    {
        _lastTriggerTime = 0;
    }
}