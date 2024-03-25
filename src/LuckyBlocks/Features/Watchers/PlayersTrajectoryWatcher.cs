using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal readonly record struct TrajectoryNode(int Id, int PlayerId, Vector2 Position, float ElapsedTimeFromPrevious,
    float CreationTime)
{
    public bool IsInvalid => Id == default || PlayerId == default || Position == default ||
                             ElapsedTimeFromPrevious == default || CreationTime == default;
}

internal interface IPlayersTrajectoryWatcher
{
    void Start();
    OneOf<TrajectoryNode, NotFound> GetTrajectoryNodeFromPosition(Vector2 position, int excludedPlayerId);
    OneOf<TrajectoryNode, NotFound, PlayerIsDeadResult> GetNextNodeForPlayer(int playerId, int previousNodeId);
}

internal class PlayersTrajectoryWatcher : IPlayersTrajectoryWatcher
{
    private readonly IGame _game;
    private readonly IIdentityService _identityService;
    private readonly ILogger _logger;
    private readonly Dictionary<int, Watcher> _watchers;
    private readonly IExtendedEvents _extendedEvents;

    public PlayersTrajectoryWatcher(IGame game, IIdentityService identityService, ILogger logger, IExtendedEvents extendedEvents)
        => (_game, _identityService, _logger, _extendedEvents, _watchers) =
            (game, identityService, logger, extendedEvents, new());

    public void Start()
    {
        var players = _identityService.GetAlivePlayers();

        foreach (var player in players)
        {
            var playerInstance = player.Instance!;
            var playerId = playerInstance.UniqueId;
            _watchers.Add(playerId, new(playerInstance));
        }

        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public OneOf<TrajectoryNode, NotFound> GetTrajectoryNodeFromPosition(Vector2 position, int excludedPlayerId)
    {
        foreach (var watcherPair in _watchers)
        {
            var watcher = watcherPair.Value;
            var getTrajectoryNodeResult = watcher.GetTrajectoryNodeFromPosition(position, excludedPlayerId);

            if (!getTrajectoryNodeResult.TryPickT0(out var trajectoryNode, out _))
                continue;

            return trajectoryNode;
        }

        return new NotFound();
    }

    public OneOf<TrajectoryNode, NotFound, PlayerIsDeadResult> GetNextNodeForPlayer(int playerId, int previousNodeId)
    {
        if (!_watchers.TryGetValue(playerId, out var watcher))
            return new NotFound();

        var getNextNodeResult = watcher.GetNextNode(previousNodeId);
        return getNextNodeResult.Match<OneOf<TrajectoryNode, NotFound, PlayerIsDeadResult>>(x => x, x => x);
    }

    private void OnUpdate(Event<float> @event)
    {
        var elapsed = @event.Args;
        
        try
        {
            List<int>? disposedWatchers = default;

            foreach (var watcherPair in _watchers)
            {
                var playerId = watcherPair.Key;
                var watcher = watcherPair.Value;
                var updateResult = watcher.OnUpdate(elapsed, _game.TotalElapsedGameTime);

                if (!updateResult.IsT1)
                    continue;

                disposedWatchers ??= new List<int>(_watchers.Count);
                disposedWatchers.Add(playerId);
                watcher.Dispose();
            }

            disposedWatchers?.ForEach(x => _watchers.Remove(x));
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Unexpected exception in PlayersTrajectoryWatcher.OnUpdate");
        }
    }

    private class Watcher
    {
        private readonly IPlayer _player;
        private readonly List<TrajectoryNode> _trajectoryNodes;

        private int _nodeId;

        public Watcher(IPlayer player)
            => (_player, _trajectoryNodes) = (player, new());

        public OneOf<Success, PlayerIsDeadResult> OnUpdate(float elapsedFromPreviousUpdate, float gameTime)
        {
            if (_player.IsDead)
                return new PlayerIsDeadResult();

            var previousNode = _trajectoryNodes.LastOrDefault();
            var offset = new Vector2(0, 5);
            var playerPosition = _player.GetWorldPosition() + offset;
            var distanceDifference = Vector2.Distance(previousNode.Position, playerPosition);

            if (distanceDifference <= 0.3f)
                return new Success();

            _nodeId++;

            var trajectoryNode = new TrajectoryNode(_nodeId, _player.UniqueId, playerPosition,
                elapsedFromPreviousUpdate, gameTime);
            _trajectoryNodes.Add(trajectoryNode);

            RemoveTooOldNodes(gameTime);

            return new Success();
        }

        public OneOf<TrajectoryNode, NotFound> GetTrajectoryNodeFromPosition(Vector2 position, int excludedPlayerId)
        {
            var trajectoryNode = _trajectoryNodes
                .Where(x => x.PlayerId != excludedPlayerId)
                .LastOrDefault(x => Vector2.Distance(x.Position, position) <= 2f);
            return trajectoryNode.IsInvalid ? new NotFound() : trajectoryNode;
        }

        public OneOf<TrajectoryNode, NotFound> GetNextNode(int previousNodeId)
        {
            var previousTrajectoryNode = _trajectoryNodes.FirstOrDefault(x => x.Id == previousNodeId);

            if (previousTrajectoryNode.IsInvalid)
                return new NotFound();

            var previousTrajectoryNodeIndex = _trajectoryNodes.IndexOf(previousTrajectoryNode);
            if (previousTrajectoryNodeIndex + 1 < _trajectoryNodes.Count)
                return _trajectoryNodes[previousTrajectoryNodeIndex + 1];

            return new NotFound();
        }

        public void Dispose()
        {
            _trajectoryNodes.Clear();
        }

        private void RemoveTooOldNodes(float gameTime)
        {
            _trajectoryNodes.RemoveAll(x => gameTime - x.CreationTime >= 7_000f);
        }
    }
}