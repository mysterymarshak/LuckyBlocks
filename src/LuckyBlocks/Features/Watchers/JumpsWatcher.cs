using System;
using LuckyBlocks.Extensions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal class JumpsWatcher
{
    public event Action? Jump;

    private readonly IPlayer _player;
    private readonly IExtendedEvents _extendedEvents;

    private int _lastJumpsCount;
    private IEventSubscription? _updateEventSubscription;

    public JumpsWatcher(IPlayer player, IExtendedEvents extendedEvents)
        => (_player, _extendedEvents) = (player, extendedEvents);

    public void Start()
    {
        _lastJumpsCount = _player.Statistics?.TotalJumps ?? default;
        _updateEventSubscription = _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void Dispose()
    {
        _updateEventSubscription?.Dispose();
    }

    private void OnUpdate(Event<float> @event)
    {
        if (_player.IsDead || !_player.IsValid() || _player.Statistics is null)
        {
            Dispose();
            return;
        }

        var totalJumps = _player.Statistics.TotalJumps;
        if (totalJumps == _lastJumpsCount)
            return;

        _lastJumpsCount = totalJumps;
        Jump?.Invoke();
    }
}