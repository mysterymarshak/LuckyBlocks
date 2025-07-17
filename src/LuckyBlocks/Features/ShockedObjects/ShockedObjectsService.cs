using System;
using Autofac;
using LuckyBlocks.Features.Entities;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.Mediator;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.ShockedObjects;

internal interface IShockedObjectsService
{
    void Initialize();
    void Shock(IObject @object, TimeSpan shockDuration);
    bool IsShocked(IObject @object);
}

internal class ShockedObjectsService : IShockedObjectsService
{
    private readonly IEntitiesService _entitiesService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ITimeStopService _timeStopService;
    private readonly ITimeProvider _timeProvider;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IExtendedEvents _extendedEvents;

    public ShockedObjectsService(IEntitiesService entitiesService, IEffectsPlayer effectsPlayer,
        ITimeStopService timeStopService, ITimeProvider timeProvider, IGame game, ILogger logger, IMediator mediator,
        ILifetimeScope lifetimeScope)
    {
        _entitiesService = entitiesService;
        _effectsPlayer = effectsPlayer;
        _timeStopService = timeStopService;
        _timeProvider = timeProvider;
        _game = game;
        _logger = logger;
        _mediator = mediator;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        var extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _extendedEvents = extendedEvents;
    }

    public void Initialize()
    {
        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void Shock(IObject @object, TimeSpan shockDuration)
    {
        var shockedObject = new ShockedObject(@object, shockDuration, _effectsPlayer, _game);
        shockedObject.Initialize();
        _entitiesService.Add(shockedObject);

        _logger.Verbose("Object '{Id}': {Name} was shocked for {Time}ms", @object.UniqueId, @object.Name,
            Math.Round(shockedObject.TimeLeft.TotalMilliseconds));
    }

    public bool IsShocked(IObject @object)
    {
        return _entitiesService.IsRegistered(@object);
    }

    private void OnShockEnded(ShockedObject shockedObject)
    {
        shockedObject.Dispose();
        _entitiesService.Remove(shockedObject.ObjectId);

        _logger.Verbose("Shock from object '{Id}': {Name} was removed", shockedObject.ObjectId, shockedObject.Name);
    }

    private void OnUpdate(Event<float> @event)
    {
        var elapsed = @event.Args * _timeProvider.GameSlowMoModifier;

        var shockedObjects = _entitiesService.GetAllUnsafe<ShockedObject>();
        if (shockedObjects.Count == 0)
            return;

        var isTimeStopped = _timeStopService.IsTimeStopped;

        for (var index = shockedObjects.Count - 1; index >= 0; index--)
        {
            var entity = shockedObjects[index];
            var shockedObject = (ShockedObject)entity;
            var touchedObjects = shockedObject.Update(elapsed, isTimeStopped);

            if (shockedObject.Charge <= ShockedObject.ELEMENTARY_CHARGE)
            {
                OnShockEnded(shockedObject);
                continue;
            }

            var notification = new ObjectsTouchedShockObjectNotification(shockedObject, touchedObjects);
            _mediator.Publish(notification);
        }
    }
}