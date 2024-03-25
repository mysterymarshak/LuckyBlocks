using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time;

internal class EntitiesTimeStopQueueProvider
{
    public float CurrentChunkRadius => _chunkRadiiByIndex.ContainsKey(_chunkIndex)
        ? _chunkRadiiByIndex[_chunkIndex]
        : _chunkRadiiByIndex.Values.Max();
    
    private readonly IObjectsWatcher _objectsWatcher;
    private readonly IEnumerable<IObject> _exclusions;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Vector2 _relativePosition;
    private readonly int _chunksCount;
    private readonly Dictionary<float, List<IObject>> _chunksByRadius = new();
    private readonly Dictionary<int, float> _chunkRadiiByIndex = new();
    private readonly List<IEntity> _notChunkedEntities = [];

    private int _chunkIndex;
    private IEventSubscription? _objectsCreatedSubscription;
    private IEventSubscription? _projectileCreatedSubscription;

    public EntitiesTimeStopQueueProvider(IObjectsWatcher objectsWatcher, IEnumerable<IObject> exclusions, IGame game,
        IExtendedEvents extendedEvents, Vector2 relativePosition, int chunksCount) =>
        (_objectsWatcher, _extendedEvents, _relativePosition, _chunksCount, _exclusions, _game) = (objectsWatcher,
            extendedEvents, relativePosition, chunksCount, exclusions, game);

    public void Initialize()
    {
        var objects = _objectsWatcher.AllObjects
            .Except(_exclusions)
            .Where(x => x.GetBodyType() != BodyType.Static || x.Name.Contains("Bg") || x.Name.Contains("WaterZone") || x.Name.Contains("AcidZone"))
            .OrderBy(x => Vector2.Distance(x.GetWorldPosition(), _relativePosition))
            .ToList();

        var chunkedObjects = objects.Chunk((int)Math.Ceiling((double)objects.Count / _chunksCount));

        var chunkIndex = 0;
        foreach (var objectsEnumerable in chunkedObjects)
        {
            var chunkObjects = objectsEnumerable.ToList();
            var chunkRadius = Vector2.Distance(chunkObjects.Last().GetWorldPosition(), _relativePosition);

            _chunksByRadius.Add(chunkRadius, chunkObjects);
            _chunkRadiiByIndex.Add(chunkIndex, chunkRadius);

            chunkIndex++;
        }

        _notChunkedEntities.AddRange(_game.GetProjectiles().Select(x => new ProjectileEntity(x)));
        _notChunkedEntities.AddRange(_game.GetFireNodes().Select(x => new FireNodeEntity(x, _game)));

        _objectsCreatedSubscription = _extendedEvents.HookOnCreated(OnObjectsCreated, EventHookMode.Default);
        _projectileCreatedSubscription =
            _extendedEvents.HookOnProjectilesCreated(OnProjectilesCreated, EventHookMode.Default);
    }

    public IEnumerable<IEntity> GetNextChunk()
    {
        if (_chunkRadiiByIndex.TryGetValue(_chunkIndex, out var currentChunkRadius))
        {
            foreach (var entity in GetNotChunkedEntities(currentChunkRadius))
            {
                yield return entity;
            }

            foreach (var entity in GetChunkedEntities(currentChunkRadius))
            {
                yield return entity;
            }
        }
        else
        {
            currentChunkRadius = _chunkRadiiByIndex.Values.Max();
            foreach (var entity in GetNotChunkedEntities(currentChunkRadius))
            {
                yield return entity;
            }
        }

        _chunkIndex++;
    }

    private IEnumerable<IEntity> GetNotChunkedEntities(float currentChunkRadius)
    {
        List<IEntity>? entitiesToRemove = null;

        foreach (var entity in _notChunkedEntities)
        {
            var distanceToRelativePosition = Vector2.Distance(entity.Position, _relativePosition);
            if (currentChunkRadius >= distanceToRelativePosition)
            {
                if (entity.IsValid())
                    yield return entity;

                entitiesToRemove ??= [];
                entitiesToRemove.Add(entity);
            }
        }

        entitiesToRemove?.ForEach(x => _notChunkedEntities.Remove(x));
    }

    private IEnumerable<IEntity> GetChunkedEntities(float currentChunkRadius)
    {
        foreach (var @object in _chunksByRadius[currentChunkRadius])
        {
            if (!@object.IsValid())
                continue;

            var entity = new ObjectEntity(@object);

            if (Vector2.Distance(entity.Position, _relativePosition) > currentChunkRadius)
            {
                _notChunkedEntities.Add(entity);
                continue;
            }

            yield return entity;
        }
    }

    public void Dispose()
    {
        _objectsCreatedSubscription?.Dispose();
        _projectileCreatedSubscription?.Dispose();
    }

    private void OnObjectsCreated(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        _notChunkedEntities.AddRange(objects.Select(x => new ObjectEntity(x)));
    }

    private void OnProjectilesCreated(Event<IProjectile[]> @event)
    {
        var projectiles = @event.Args;
        _notChunkedEntities.AddRange(projectiles.Select(x => new ProjectileEntity(x)));
    }
}