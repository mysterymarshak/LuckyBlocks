using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.Time.TimeStop.Objects;
using LuckyBlocks.Features.Time.TimeStop.Slowers;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using LuckyBlocks.Utils.Watchers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop;

internal interface ITimeStopper
{
    TimeSpan SmoothEntitiesStopDuration { get; }

    CancellationTokenSource StopTime(TimeSpan duration, IObject relativeObject, Action? timeStoppedCallback = default,
        Action? timeResumedCallback = default);
}

internal class TimeStopper : ITimeStopper
{
    public TimeSpan SmoothEntitiesStopDuration => TimeSpan.FromMilliseconds(1500);

    private TimeSpan SmoothObjectResumingDuration => TimeSpan.FromMilliseconds(1500);
    private TimeSpan ProjectileSloMoDuration => TimeSpan.FromMilliseconds(150);
    private TimeSpan MissileSloMoDuration => TimeSpan.FromMilliseconds(400);
    private TimeSpan FireNodesWatcherUpdatePeriod => TimeSpan.FromMilliseconds(200);

    private readonly IExtendedEvents _extendedEvents;
    private readonly IIdentityService _identityService;
    private readonly IObjectsWatcher _objectsWatcher;
    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly IMappedObjectsService _mappedObjectsService;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly Dictionary<string, ITimeStoppedEntity> _timeStoppedEntities = new();

    private TimeSpan _timeStopDuration;
    private IObject? _relativeObject;
    private CancellationTokenSource? _cts;
    private CancellationTokenRegistration? _ctr;
    private IEventSubscription? _objectsCreatedSubscription;
    private IEventSubscription? _projectilesCreatedSubscription;
    private Action? _timeResumedCallback;
    private FireNodesWatcher? _fireNodesWatcher;

    public TimeStopper(ILifetimeScope lifetimeScope, IIdentityService identityService, IObjectsWatcher objectsWatcher,
        INotificationService notificationService, IEffectsPlayer effectsPlayer,
        IPlayerModifiersService playerModifiersService, IMappedObjectsService mappedObjectsService, ILogger logger,
        IGame game)
    {
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _identityService = identityService;
        _objectsWatcher = objectsWatcher;
        _notificationService = notificationService;
        _effectsPlayer = effectsPlayer;
        _playerModifiersService = playerModifiersService;
        _mappedObjectsService = mappedObjectsService;
        _logger = logger;
        _game = game;
    }

    public CancellationTokenSource StopTime(TimeSpan duration, IObject relativeObject,
        Action? timeStoppedCallback = default, Action? timeResumedCallback = default)
    {
        _timeStopDuration = duration;
        _relativeObject = relativeObject;
        _timeResumedCallback = timeResumedCallback;
        _cts = new CancellationTokenSource();

        var exclusions = _identityService.GetAlivePlayers()
            .Where(x => x.GetImmunityFlags().HasFlag<ImmunityFlag>(ImmunityFlag.ImmunityToTimeStop))
            .Select(x => x.Instance!);
        StartSmoothStoppingEntities(relativeObject, exclusions);

        _ctr = _cts.Token.Register(ResumeTime);

        Awaiter.Start((Action)Delegate.Combine(OnTimeStopped, timeStoppedCallback), SmoothEntitiesStopDuration,
            _cts.Token);

        return _cts;
    }

    private void StartSmoothStoppingEntities(IObject relativeObject, IEnumerable<IObject> objectsExclusions)
    {
        var relativePosition = relativeObject.GetWorldPosition();
        var timerIterations = (int)(SmoothEntitiesStopDuration.TotalMilliseconds / 100);
        var queueProvider = new EntitiesTimeStopQueueProvider(_objectsWatcher, objectsExclusions, _game,
            _extendedEvents, relativePosition, timerIterations);
        queueProvider.Initialize();

        var timer = new PeriodicTimer(SmoothEntitiesStopDuration.Divide(timerIterations), TimeBehavior.RealTime,
            () =>
            {
                PlayBorderEffect(relativePosition, queueProvider.CurrentChunkRadius);

                var entities = queueProvider.GetNextChunk();
                foreach (var entity in entities)
                {
                    StopEntity(entity);
                }
            }, queueProvider.Dispose, timerIterations, _extendedEvents, _cts!.Token);
        timer.Start();
    }

    private void PlayBorderEffect(Vector2 center, float radius)
    {
        for (var i = 0; i < 30; i++)
        {
            var vector = new Vector2(radius, 0);
            var effectPosition = center + vector.Rotate(2 * Math.PI / 30 * i);
            _effectsPlayer.PlayEffect(EffectName.Electric, effectPosition);
        }
    }

    private void StopEntity(IEntity entity)
    {
        if (_timeStoppedEntities.ContainsKey(entity.Id))
            return;

        var timeStoppedEntity = CreateTimeStoppedEntity(entity);
        timeStoppedEntity.Initialize();

        _timeStoppedEntities.Add(entity.Id, timeStoppedEntity);
    }

    private ITimeStoppedEntity CreateTimeStoppedEntity(IEntity entity) => entity switch
    {
        ObjectEntity { Object: var @object, Object.Name: var name } when name.Contains("WaterZone") ||
                                                                         name.Contains("AcidZone") =>
            new TimeStoppedLiquid(@object, _game),
        ObjectEntity { Object: var @object, Object.Name: var name } when name.Contains("Bg") =>
            new TimeStoppedBackgroundObject(@object),
        ObjectEntity { Object: IPlayer player } => new TimeStoppedPlayer(player, _game, _effectsPlayer, _extendedEvents,
            _playerModifiersService, _identityService),
        ObjectEntity { Object: IObjectGrenadeThrown grenade } =>
            new TimeStoppedGrenade(grenade, _game, _effectsPlayer, _extendedEvents),
        ObjectEntity { Object: IObjectMineThrown mine } => new TimeStoppedMine(mine, _game, _effectsPlayer,
            _extendedEvents, _mappedObjectsService),
        ObjectEntity { Object: var @object } => new TimeStoppedDynamicObject(@object, _game, _effectsPlayer,
            _extendedEvents),
        ProjectileEntity { Projectile: var projectile } => new TimeStoppedProjectile(projectile, _extendedEvents),
        FireNodeEntity { FireNode: var fireNode } => new TimeStoppedFireNode(fireNode, _game, _effectsPlayer,
            _extendedEvents),
        _ => throw new ArgumentException(nameof(entity))
    };

    private void OnTimeStopped()
    {
        _notificationService.CreatePopupNotification("TIME IS STOPPED", Color.Yellow, _timeStopDuration);

        _objectsCreatedSubscription = _extendedEvents.HookOnCreated(OnObjectsCreated, EventHookMode.Default);
        _projectilesCreatedSubscription =
            _extendedEvents.HookOnProjectilesCreated(OnProjectilesCreated, EventHookMode.Default);

        _fireNodesWatcher = new FireNodesWatcher(FireNodesWatcherUpdatePeriod, _cts!.Token, _game, _extendedEvents,
            OnFireNodesCreated);
        _fireNodesWatcher.Start();

        Awaiter.Start(ResumeTime, _timeStopDuration, _cts.Token);
    }

    private void OnObjectsCreated(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (@object.IsMissile)
            {
                SlowDownAndStopMissile(@object);
                continue;
            }

            StopEntity(new ObjectEntity(@object));
        }
    }

    private void OnProjectilesCreated(Event<IProjectile[]> @event)
    {
        var projectiles = @event.Args;
        foreach (var projectile in projectiles)
        {
            SlowDownAndStopProjectile(projectile);
        }
    }

    private void OnFireNodesCreated(IEnumerable<FireNode> fireNodes)
    {
        foreach (var fireNode in fireNodes)
        {
            StopEntity(new FireNodeEntity(fireNode, _game));
        }
    }

    private void SlowDownAndStopMissile(IObject missile)
    {
        _logger.Debug("slowing down missile {missile}", missile.Name);

        var objectSlower = new ObjectSlower(missile, _game, MissileSloMoDuration, _cts!.Token, _extendedEvents);
        objectSlower.Initialize();

        Awaiter.Start(delegate
        {
            objectSlower.Stop();

            _logger.Debug("stopping missile {missile}", missile.Name);

            StopEntity(new ObjectEntity(missile));
        }, MissileSloMoDuration, _cts.Token);
    }

    private void SlowDownAndStopProjectile(IProjectile projectile)
    {
        _logger.Debug("slowing down projectile {projectile}", projectile.ProjectileItem);

        var projectileSlower = new ProjectileSlower(projectile, _cts!.Token, _extendedEvents);
        projectileSlower.Initialize();

        Awaiter.Start(delegate
        {
            projectileSlower.Stop();

            _logger.Debug("stopping projectile {projectile}", projectile.ProjectileItem);

            StopEntity(new ProjectileEntity(projectile));
        }, ProjectileSloMoDuration, _cts.Token);
    }

    private void ResumeTime()
    {
        Dispose();
        StartSmoothResumingObjects();

        Awaiter.Start((Action)Delegate.Combine(OnTimeResumed, _timeResumedCallback), SmoothObjectResumingDuration);
    }

    private void StartSmoothResumingObjects()
    {
        var relativePosition = _relativeObject?.GetWorldPosition() ?? Vector2.Zero;
        var timerIterations = (int)(SmoothObjectResumingDuration.TotalMilliseconds / 100);
        var chunkedEntities = _timeStoppedEntities.Values
            .OrderBy(x => Vector2.Distance(x.Position, relativePosition))
            .Chunk((int)Math.Ceiling((double)_timeStoppedEntities.Count / timerIterations))
            .ToList();

        var index = 0;
        var timer = new PeriodicTimer(SmoothObjectResumingDuration.Divide(timerIterations), TimeBehavior.RealTime, () =>
        {
            if (index >= chunkedEntities.Count)
                return;

            PlayBorderEffect(relativePosition,
                Vector2.Distance(relativePosition, chunkedEntities[index].Last().Position));
            foreach (var entity in chunkedEntities[index])
            {
                entity.ResumeTime();
            }

            index++;
        }, default, timerIterations, _extendedEvents);
        timer.Start();
    }

    private void OnTimeResumed()
    {
        _timeStoppedEntities.Clear();
        _notificationService.ClosePopupNotification();
    }

    private void Dispose()
    {
        _objectsCreatedSubscription?.Dispose();
        _projectilesCreatedSubscription?.Dispose();
        _ctr!.Value.Dispose();
        _cts!.Dispose();
        _fireNodesWatcher?.Stop();
    }
}