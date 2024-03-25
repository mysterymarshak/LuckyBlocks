using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Wayback;

internal interface IWaybackMachine
{
    bool CanBeUsed { get; }
    void Initialize();
    void RestoreFromRandomSnapshot();
}

internal class WaybackMachine : IWaybackMachine
{
    public bool CanBeUsed => !_isBusy && _snapshots.Count >= MIN_SNAPSHOTS_TO_USE;

    private const int MAX_SNAPSHOTS = 10;
    private const int MIN_SNAPSHOTS_TO_USE = 3;

    private static TimeSpan SnapshotsFrequency => TimeSpan.FromMilliseconds(3500);
    private static TimeSpan DelayBeforeWayback => TimeSpan.FromMilliseconds(1500);

    private readonly IGame _game;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IExtendedEvents _extendedEvents;
    private readonly INotificationService _notificationService;
    private readonly IIdentityService _identityService;
    private readonly IRespawner _respawner;
    private readonly ILogger _logger;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly PeriodicTimer _snapshotCreationTimer;
    private readonly List<WaybackSnapshot> _snapshots;

    private List<IObject>? _objects;
    private bool _isBusy;

    public WaybackMachine(IGame game, IEffectsPlayer effectsPlayer, INotificationService notificationService,
        IIdentityService identityService, IRespawner respawner, ILogger logger, ILifetimeScope lifetimeScope)
    {
        _game = game;
        _effectsPlayer = effectsPlayer;
        _notificationService = notificationService;
        _identityService = identityService;
        _respawner = respawner;
        _logger = logger;
        _lifetimeScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = _lifetimeScope.Resolve<IExtendedEvents>();
        _snapshotCreationTimer = new PeriodicTimer(SnapshotsFrequency, TimeBehavior.TimeModifier, CreateSnapshot, default, int.MaxValue,
            _lifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>());
        _snapshots = new(MAX_SNAPSHOTS);
    }

    public void Initialize()
    {
        throw new NotSupportedException();
        
        _objects = _game.GetObjects<IObject>()
            .Where(x => x.GetBodyType() != BodyType.Static)
            .Where(x => x.GetCollisionFilter().MaskBits != 0)
            // static objects in snapshot creation unsupported because performance reason
            .Where(x => x.GetPhysicsLayer() == PhysicsLayer.Active)
            .ToList();

        _extendedEvents.HookOnCreated(OnObjectsCreated, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectsDestroyed, EventHookMode.Default);

        _snapshotCreationTimer.Start();
    }

    public void RestoreFromRandomSnapshot()
    {
        _isBusy = true;

        _snapshotCreationTimer.Stop();

        _effectsPlayer.PlaySloMoEffect(DelayBeforeWayback);
        _notificationService.CreatePopupNotification("BACK TO THE PAST", ExtendedColors.ImperialRed,
            DelayBeforeWayback);

        var random = SharedRandom.Instance;
        var snapshotId = random.Next(_snapshots.Count - 1);

        Awaiter.Start(delegate()
        {
            Restore(snapshotId);

            _isBusy = false;

            _snapshotCreationTimer.Reset();
            _snapshotCreationTimer.Start();
        }, DelayBeforeWayback);
    }

    private void Restore(int snapshotId)
    {
        var snapshot = _snapshots[snapshotId];

        RestoreObjects(snapshot.Objects);

        _snapshots.Clear();

        var waybackTime = TimeSpan.FromMilliseconds(_game.TotalElapsedGameTime - snapshot.Time).Seconds;
        _notificationService.CreateChatNotification($"NAZAD NA {waybackTime} SECUND", ExtendedColors.ImperialRed);
    }

    private void RestoreObjects(List<IWaybackObject> waybackObjects)
    {
        foreach (var @object in _objects!.ToList())
        {
            if (waybackObjects.Any(x => x.Object == @object))
                continue;

            @object.Remove();
        }

        foreach (var waybackObject in waybackObjects)
        {
            waybackObject.Restore(_game);
        }
    }

    private void CreateSnapshot()
    {
        if (_snapshots.Count == MAX_SNAPSHOTS)
        {
            _snapshots.RemoveAt(0);
        }

        var waybackObjects = _objects!
            .Select(CreateWaybackObject)
            .ToList();
        
        var snapshot = new WaybackSnapshot(_game.TotalElapsedGameTime, waybackObjects);
        _snapshots.Add(snapshot);
    }

    private IWaybackObject CreateWaybackObject(IObject @object) => @object switch
    {
        IPlayer player => new LightWaybackPlayer(player, _identityService, _respawner),
        _ => new LightWaybackObject(@object)
    };

    private void OnObjectsCreated(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            if (@object.GetCollisionFilter().MaskBits == 0)
                continue;
            
            _objects!.Add(@object);
        }
    }

    private void OnObjectsDestroyed(Event<IObject[]> @event)
    {
        var objects = @event.Args;
        foreach (var @object in objects)
        {
            _objects!.Remove(@object);
        }
    }
}