using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Elevators;
using LuckyBlocks.Features.Entities;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.LuckyBlocks;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Time.TimeRevert.Objects;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Features.WeaponPowerups.Grenades;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using LuckyBlocks.Utils.Watchers;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeRevert;

internal interface ISnapshotCreator
{
    bool IsBusy { get; }
    void Initialize();
    void CreateSnapshot(Action<RealitySnapshot> snapshotCreatedCallback);
}

internal class SnapshotCreator : ISnapshotCreator
{
    public bool IsBusy { get; private set; }

    private static readonly List<Type> AdditionalObjectsToRevertTypes =
    [
        typeof(IObjectElevatorAttachmentJoint), typeof(IObjectRailAttachmentJoint),
        typeof(IObjectTrigger), typeof(IObjectWeldJoint),
        typeof(IObjectTargetObjectJoint), typeof(IObjectDistanceJoint),
        typeof(IObjectDestroyTargets)
    ];

    // performance issue - exclude extra objects
    private static readonly List<Type> AdditionalObjectsExclusions =
    [
        typeof(IObjectOnGameOverTrigger), typeof(IObjectGroupMarker),
        typeof(IObjectScriptTrigger), typeof(IObjectCameraAreaTrigger),
        typeof(IObjectRandomTrigger), typeof(IObjectSetFrameTrigger),
        typeof(IObjectPlaySoundTrigger), typeof(IObjectPathNodeEnableTrigger),
        typeof(IObjectButtonTrigger)
    ];

    private const float ElapsedMillisecondsThreshold = 12f;

    private readonly IObjectsWatcher _objectsWatcher;
    private readonly IGame _game;
    private readonly ITimeProvider _timeProvider;
    private readonly IIdentityService _identityService;
    private readonly IRespawner _respawner;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IElevatorsService _elevatorsService;
    private readonly IProjectilesService _projectilesService;
    private readonly IBuffsService _buffsService;
    private readonly IMagicService _magicService;
    private readonly IEntitiesService _entitiesService;
    private readonly IWeaponsDataWatcher _weaponsDataWatcher;
    private readonly IGrenadesService _grenadesService;
    private readonly ISpawnChanceService _spawnChanceService;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly IExtendedEvents _extendedEvents;
    private readonly List<IObject> _additionalObjectsToRevert;
    private readonly PeriodicTimer _snapshotCreationTimer;

    private int _snapshotId;
    private List<IRevertedObject> _staticObjects = null!;
    private IEnumerator<IRevertedObject> _staticObjectsChunksEnumerator = null!;
    private List<IRevertedObject> _dynamicObjects = null!;
    private IEnumerator<IRevertedObject> _dynamicObjectsChunksEnumerator = null!;
    private List<IRevertedObject> _additionalObjects = null!;
    private IEnumerator<IRevertedObject> _additionalObjectsChunksEnumerator = null!;
    private List<RevertedFireNode> _revertedFireNodes = null!;
    private List<RevertedProjectile> _revertedProjectiles = null!;
    private Iteration _iteration;
    private Action<RealitySnapshot> _snapshotCreationCallback = null!;
    private int _updates;

    public SnapshotCreator(IObjectsWatcher objectsWatcher, IGame game, ITimeProvider timeProvider,
        IIdentityService identityService, IRespawner respawner, IWeaponPowerupsService weaponPowerupsService,
        IElevatorsService elevatorsService, IProjectilesService projectilesService, IBuffsService buffsService,
        IMagicService magicService, IEntitiesService entitiesService, IWeaponsDataWatcher weaponsDataWatcher,
        IGrenadesService grenadesService, ISpawnChanceService spawnChanceService, IMediator mediator, ILogger logger,
        ILifetimeScope lifetimeScope)
    {
        _objectsWatcher = objectsWatcher;
        _game = game;
        _timeProvider = timeProvider;
        _identityService = identityService;
        _respawner = respawner;
        _weaponPowerupsService = weaponPowerupsService;
        _elevatorsService = elevatorsService;
        _projectilesService = projectilesService;
        _buffsService = buffsService;
        _magicService = magicService;
        _entitiesService = entitiesService;
        _weaponsDataWatcher = weaponsDataWatcher;
        _grenadesService = grenadesService;
        _spawnChanceService = spawnChanceService;
        _mediator = mediator;
        _logger = logger;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _additionalObjectsToRevert = GetAdditionalObjectsToRevert();
        _snapshotCreationTimer = new PeriodicTimer(TimeSpan.Zero, TimeBehavior.RealTime, Iterate, null, int.MaxValue,
            _extendedEvents);
    }

    public void Initialize()
    {
        _extendedEvents.HookOnCreated(OnObjectCreated, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectDestroyed, EventHookMode.Default);
    }

    public void CreateSnapshot(Action<RealitySnapshot> snapshotCreatedCallback)
    {
        if (IsBusy)
        {
            throw new InvalidOperationException();
        }

        IsBusy = true;
        _snapshotCreationCallback = snapshotCreatedCallback;
        StartSnapshotTaking();
    }

    private void StartSnapshotTaking()
    {
        var staticObjects = _objectsWatcher.StaticIObjects.ToList();
        var dynamicObjects = _objectsWatcher.DynamicObjects.ToList();

        _staticObjectsChunksEnumerator = staticObjects
            .Select(x => CreateRevertedObject(x.Value))
            .GetEnumerator();
        _additionalObjectsChunksEnumerator = _additionalObjectsToRevert.ToList()
            .Select(CreateAdditionalRevertedObject)
            .GetEnumerator();
        _dynamicObjectsChunksEnumerator = dynamicObjects
            .Where(x => x.Value is not IPlayer playerInstance || playerInstance.IsValidUser())
            .Select(x => CreateRevertedObject(x.Value))
            .GetEnumerator();
        _staticObjects = new List<IRevertedObject>(staticObjects.Count);
        _additionalObjects = new List<IRevertedObject>(_additionalObjectsToRevert.Count);
        _dynamicObjects = new List<IRevertedObject>(dynamicObjects.Count);

        _revertedFireNodes = CreateRevertedFireNodes();
        _revertedProjectiles = CreateRevertedProjectiles();

        _logger.Debug("Snapshot initialization took {InitializationTime}ms",
            _timeProvider.Stopwatch.ElapsedMilliseconds);
        _iteration = Iteration.StaticObjects;

        _snapshotCreationTimer.Reset();
        _snapshotCreationTimer.Start();
    }

    private void Iterate()
    {
        _updates++;

        start:
        var enumerator = _iteration switch
        {
            Iteration.StaticObjects => _staticObjectsChunksEnumerator,
            Iteration.AdditionalObjects => _additionalObjectsChunksEnumerator,
            Iteration.DynamicObjects => _dynamicObjectsChunksEnumerator,
            _ => throw new ArgumentOutOfRangeException(nameof(_iteration))
        };

        var collection = _iteration switch
        {
            Iteration.StaticObjects => _staticObjects,
            Iteration.AdditionalObjects => _additionalObjects,
            Iteration.DynamicObjects => _dynamicObjects,
            _ => throw new ArgumentOutOfRangeException(nameof(_iteration))
        };

        var objectIndex = 0;
        var stopwatch = _timeProvider.Stopwatch;
        const int chunkSize = 10;

        while (enumerator.MoveNext())
        {
            var @object = enumerator.Current;

            collection.Add(@object);

            objectIndex++;
            if (objectIndex % chunkSize == 0 && stopwatch.ElapsedMilliseconds >= ElapsedMillisecondsThreshold)
            {
                return;
            }
        }

        _iteration = _iteration switch
        {
            Iteration.StaticObjects => Iteration.AdditionalObjects,
            Iteration.AdditionalObjects => Iteration.DynamicObjects,
            Iteration.DynamicObjects => Iteration.None,
            _ => throw new ArgumentOutOfRangeException(nameof(_iteration))
        };

        if (_iteration == Iteration.None)
        {
            OnSnapshotTaken();
        }
        else
        {
            goto start;
        }
    }

    private void OnSnapshotTaken()
    {
        _staticObjectsChunksEnumerator.Dispose();
        _dynamicObjectsChunksEnumerator.Dispose();
        _additionalObjectsChunksEnumerator.Dispose();

        var snapshot = new RealitySnapshot(++_snapshotId, _staticObjects, _dynamicObjects, _additionalObjects,
            _revertedFireNodes, _revertedProjectiles, _timeProvider.ElapsedGameTime, _magicService.CloneState(),
            _entitiesService.CloneState(), _spawnChanceService.ChanceId);

        _logger.Debug("Snapshot taken in {UpdatesCount} updates", _updates);

        _snapshotCreationTimer.Stop();
        IsBusy = false;
        _updates = 0;

        _snapshotCreationCallback.Invoke(snapshot);
    }

    private IRevertedObject CreateRevertedObject(IObject @object) => @object switch
    {
        _ when @object.GetBodyType() == BodyType.Static => new RevertedStaticObject(@object),
        IPlayer playerInstance => new RevertedPlayer(_identityService.GetPlayerByInstance(playerInstance), _respawner,
            _weaponPowerupsService, _buffsService),
        IObjectSupplyCrate supplyCrate => new RevertedSupplyCrate(supplyCrate),
        IObjectWeaponItem objectWeaponItem => new RevertedObjectWeaponItem(objectWeaponItem, _weaponsDataWatcher,
            _weaponPowerupsService, _mediator),
        IObjectGrenadeThrown grenadeThrown => new RevertedGrenadeThrown(grenadeThrown, _grenadesService),
        _ => new RevertedDynamicObject(@object)
    };

    private IRevertedObject CreateAdditionalRevertedObject(IObject @object) => @object switch
    {
        IObjectWeldJoint weldJoint => new RevertedWeldJoint(weldJoint),
        IObjectTargetObjectJoint targetObjectJoint => new RevertedTargetObjectJoint(targetObjectJoint),
        IObjectDistanceJoint distanceJoint => new RevertedDistanceJoint(distanceJoint),
        IObjectDestroyTargets destroyTargets => new RevertedDestroyTargets(destroyTargets),
        IObjectElevatorAttachmentJoint elevatorAttachmentJoint => new RevertedElevatorAttachmentJoint(
            elevatorAttachmentJoint, _elevatorsService, _timeProvider), // static
        IObjectRailAttachmentJoint railAttachmentJoint => new RevertedRailAttachmentJoint(
            railAttachmentJoint), // dynamic
        IObjectOnDestroyedTrigger onDestroyedTrigger => new RevertedOnDestroyedTrigger(onDestroyedTrigger),
        IObjectDestroyTrigger destroyTrigger => new RevertedDestroyTrigger(destroyTrigger), // static
        IObjectChangeBodyTypeTrigger changeBodyTypeTrigger => new RevertedChangeBodyTypeTrigger(changeBodyTypeTrigger),
        IObjectTimerTrigger timerTrigger => new RevertedTimerTrigger(timerTrigger), // static
        IObjectTrigger trigger => new RevertedTrigger(trigger), // probably static
        _ => throw new ArgumentException("incorrect method usage")
    };

    private List<RevertedFireNode> CreateRevertedFireNodes()
    {
        var fireNodes = _game.GetFireNodes();
        return fireNodes
            .Select(x => new RevertedFireNode(x))
            .ToList();
    }

    private List<RevertedProjectile> CreateRevertedProjectiles()
    {
        var projectiles = _game.GetProjectiles();
        return projectiles
            .Select(x => new RevertedProjectile(x, _projectilesService))
            .ToList();
    }

    private List<IObject> GetAdditionalObjectsToRevert()
    {
        return _game
            .GetObjects<IObject>()
            .Where(IsCompatibleAdditionalObjectForTracking)
            .ToList();
    }

    private void OnObjectCreated(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (IsCompatibleAdditionalObjectForTracking(@object))
            {
                _additionalObjectsToRevert.Add(@object);
            }
        }
    }

    private void OnObjectDestroyed(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (IsCompatibleAdditionalObjectForTracking(@object))
            {
                _additionalObjectsToRevert.Remove(@object);
            }
        }
    }

    private static bool IsCompatibleAdditionalObjectForTracking(IObject @object) =>
        AdditionalObjectsToRevertTypes.Any(x => x.IsInstanceOfType(@object)) &&
        AdditionalObjectsExclusions.All(x => x.IsInstanceOfType(@object) == false) &&
        !string.IsNullOrWhiteSpace(@object.Name);

    private enum Iteration
    {
        None,

        StaticObjects,

        AdditionalObjects,

        DynamicObjects
    }
}