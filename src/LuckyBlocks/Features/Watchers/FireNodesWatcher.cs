using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal class FireNodesWatcher
{
    private readonly IGame _game;
    private readonly PeriodicTimer<CancellationToken> _timer;
    private readonly Action<IEnumerable<FireNode>> _fireNodesSpawnedCallback;
    private readonly List<FireNode> _fireNodes = [];

    public FireNodesWatcher(TimeSpan updatePeriod, CancellationToken cancellationToken, IGame game,
        IExtendedEvents extendedEvents, Action<IEnumerable<FireNode>> fireNodesSpawnedCallback) =>
        (_game, _fireNodesSpawnedCallback, _timer) = (game, fireNodesSpawnedCallback,
            new PeriodicTimer<CancellationToken>(updatePeriod, TimeBehavior.RealTime, _ => OnTimer(),
                ct => ct.IsCancellationRequested, default, cancellationToken, extendedEvents));

    public void Start()
    {
        _timer.Start();
        _fireNodes.AddRange(_game.GetFireNodes());
    }

    public void Stop()
    {
        _timer.Stop();
    }

    private void OnTimer()
    {
        var currentFireNodes = _game.GetFireNodes();
        var newFireNodes = currentFireNodes
            .Except(_fireNodes)
            .ToList();

        if (newFireNodes.Count > 0)
        {
            _fireNodesSpawnedCallback.Invoke(newFireNodes);
        }

        _fireNodes.RemoveAll(x => currentFireNodes.All(y => y.InstanceID != x.InstanceID));
        _fireNodes.AddRange(newFireNodes);
    }
}