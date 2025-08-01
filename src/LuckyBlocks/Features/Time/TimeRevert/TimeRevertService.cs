using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Features.Dialogues;
using LuckyBlocks.Features.Entities;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Features.Time.TimeRevert.Objects;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using LuckyBlocks.Utils.Watchers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert;

internal interface ITimeRevertService
{
    bool TimeCanBeReverted { get; }
    int SnapshotsCount { get; }
    IEnumerable<RealitySnapshot> Snapshots { get; }
    void Initialize();
    void TakeSnapshot();
    void RestoreFromSnapshot(int id);
}

internal class TimeRevertService : ITimeRevertService
{
    public bool TimeCanBeReverted => _snapshots.Count > 0 && !_snapshotCreator.IsBusy && !_game.IsGameOver;
    public int SnapshotsCount => _snapshots.Count;
    public IEnumerable<RealitySnapshot> Snapshots => _snapshots;

    private const int MaxSavedSnapshots = 9;
    private const float SnapshotsFrequency = 5000f;

    private readonly IObjectsWatcher _objectsWatcher;
    private readonly IGame _game;
    private readonly ITimeProvider _timeProvider;
    private readonly ISnapshotCreator _snapshotCreator;
    private readonly IMagicService _magicService;
    private readonly IDialoguesService _dialoguesService;
    private readonly IMappedObjectsService _mappedObjectsService;
    private readonly IEntitiesService _entitiesService;
    private readonly ISpawnChanceService _spawnChanceService;
    private readonly ILogger _logger;
    private readonly PeriodicTimer<IGame> _snapshotsTimer;
    private readonly List<RealitySnapshot> _snapshots = new(MaxSavedSnapshots);
    private readonly IExtendedEvents _extendedEvents;

    public TimeRevertService(IObjectsWatcher objectsWatcher, IGame game, ITimeProvider timeProvider,
        ISnapshotCreator snapshotCreator, IMagicService magicService, IDialoguesService dialoguesService,
        IMappedObjectsService mappedObjectsService, IEntitiesService entitiesService,
        ISpawnChanceService spawnChanceService, ILogger logger, ILifetimeScope lifetimeScope)
    {
        _objectsWatcher = objectsWatcher;
        _game = game;
        _timeProvider = timeProvider;
        _snapshotCreator = snapshotCreator;
        _magicService = magicService;
        _dialoguesService = dialoguesService;
        _mappedObjectsService = mappedObjectsService;
        _entitiesService = entitiesService;
        _spawnChanceService = spawnChanceService;
        _logger = logger;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _snapshotsTimer = new PeriodicTimer<IGame>(TimeSpan.FromMilliseconds(SnapshotsFrequency),
            TimeBehavior.TimeModifier, _ => TakeSnapshot(), x => x.IsGameOver, null, game, _extendedEvents);
    }

    public void Initialize()
    {
        _snapshotsTimer.Start();
    }

    public void TakeSnapshot()
    {
        _snapshotCreator.CreateSnapshot(OnSnapshotCreated);
    }

    public void RestoreFromSnapshot(int id)
    {
        if (!TimeCanBeReverted)
        {
            throw new InvalidOperationException("time cannot be reverted now");
        }

        var snapshot = _snapshots.FirstOrDefault(x => x.Id == id);

        if (snapshot is null)
        {
            throw new ArgumentOutOfRangeException($"snapshot with id '{id}' not found");
        }

        RestoreFromSnapshot(snapshot);
    }

    private void OnSnapshotCreated(RealitySnapshot snapshot)
    {
        if (_snapshots.Count >= MaxSavedSnapshots)
        {
            _snapshots.RemoveAt(0);
        }

        _snapshots.Add(snapshot);

        _logger.Debug("Snapshot created");
    }

    private void RestoreFromSnapshot(RealitySnapshot snapshot)
    {
        var currentDynamicObjects = _objectsWatcher.DynamicObjects.Values;
        RemoveEntitiesCreatedAfterSnapshot(snapshot, currentDynamicObjects);

        _dialoguesService.RemoveAllDialogues();

        RevertObjectsAndEntities(snapshot);

        _entitiesService.RestoreState(snapshot.EntitiesServiceState);

        _spawnChanceService.SetChance(snapshot.SpawnChanceId);

        Awaiter.Start(delegate { _magicService.RestoreState(snapshot.MagicServiceState); }, 3);
        // sad but its for correct working of WeaponPowerupsService
        // see RestoreWeaponsDataFromCopy

        _snapshots.Clear();
        _snapshotsTimer.Reset();

        _logger.Debug(
            $"Snapshot {Math.Round(TimeSpan.FromMilliseconds(_timeProvider.ElapsedGameTime - snapshot.ElapsedGameTime).TotalSeconds)}s behind restored");
    }

    private void RevertObjectsAndEntities(RealitySnapshot snapshot)
    {
        var objectsMap = new Dictionary<int, int>();

        var objectsToRevert = snapshot.StaticObjects
            .Concat(snapshot.DynamicObjects)
            .Concat(snapshot.AdditionalObjects
                .OrderBy(x => x.Object is IObjectTrigger)
                .ThenByDescending(x => x.Object is IObjectTargetObjectJoint)
                .ThenBy(x => x.Object is IObjectDestroyTargets));

        foreach (var @object in objectsToRevert)
        {
            try
            {
                var oldObjectId = @object.OldObjectId;
                var objectId = @object.Restore(_game, objectsMap);
                if (oldObjectId != objectId)
                {
                    objectsMap[oldObjectId] = objectId;
                    _logger.Debug("Restored '{ObjectName}' ({OldObjectId}->{ObjectId})", @object.Name, oldObjectId,
                        objectId);
                }

                var restoredObject = @object.Object;
                if (restoredObject is { Name: "error" })
                {
#if DEBUG
                    _logger.Warning("Error object created: {OldObjectId}->{Id} ({Name})", oldObjectId, objectId,
                        @object.Name);
#endif
                    restoredObject.Remove();
                }
            }
            catch (Exception exception)
            {
                _logger.Warning(exception, "Failed to restore object '{Name}':{Id}", @object.Object?.Name,
                    @object.Object?.UniqueId);
            }
        }

        foreach (var revertedProjectile in snapshot.Projectiles)
        {
            revertedProjectile.Restore(_game);
        }

        foreach (var map in objectsMap)
        {
            if (_mappedObjectsService.IsMapped(map.Key))
            {
                _mappedObjectsService.UpdateActualObject(map.Key, _game.GetObject(map.Value));
            }
        }
    }

    private void RemoveEntitiesCreatedAfterSnapshot(RealitySnapshot snapshot,
        IEnumerable<IObject> currentObjects)
    {
        var revertedDynamicObjects = snapshot.DynamicObjects;
        var objectsToRemove = currentObjects.Where(x => revertedDynamicObjects.All(y => x != y.Object));

        foreach (var objectToRemove in objectsToRemove)
        {
            RemoveObject(objectToRemove);
        }

        FuckFuckingFire(snapshot.FireNodes);

        var revertedProjectiles = snapshot.Projectiles;
        var projectilesToRemove = _game
            .GetProjectiles()
            .Where(x => revertedProjectiles.All(y => x.InstanceID != y.InstanceId));

        foreach (var projectileToRemove in projectilesToRemove)
        {
            projectileToRemove.FlagForRemoval();
        }
    }

    // yes, there's only one way to remove all fire from the map
    // i name this method by 3 'F' rule: Fuck fucking fire
    private void FuckFuckingFire(List<RevertedFireNode> revertedFireNodes)
    {
        var timer = new PeriodicTimer(TimeSpan.Zero, TimeBehavior.RealTime | TimeBehavior.IgnoreTimeStop, delegate
        {
            var fireNodes = _game.GetFireNodes();
            foreach (var fireNode in fireNodes)
            {
                _game.EndFireNode(fireNode.InstanceID);
            }
        }, delegate
        {
            foreach (var revertedFireNode in revertedFireNodes)
            {
                revertedFireNode.Restore(_game);
            }
        }, 5, _extendedEvents);
        timer.Start();
    }

    private void RemoveObject(IObject @object)
    {
        if (@object is IObjectSupplyCrate { CustomId: "LuckyBlock" } supplyCrate)
        {
            supplyCrate.CustomId = "RemovedLuckyBlock";
            supplyCrate.Remove();
            return;
        }

        @object.Remove();
    }
}