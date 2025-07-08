using System;
using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Loot.Buffs.Durable;
using LuckyBlocks.Notifications;
using LuckyBlocks.Repositories;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.ShockedObjects;

internal interface IShockedObjectsService
{
    void Initialize();
    ShockedObject Shock(IObject @object, TimeSpan shockDuration);
    bool IsShocked(IObject @object);
}

internal class ShockedObjectsService : IShockedObjectsService
{
    private readonly IShockedObjectsRepository _shockedObjectsRepository;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ITimeStopService _timeStopService;
    private readonly ITimeProvider _timeProvider;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly IMediator _mediator;
    private readonly IExtendedEvents _extendedEvents;

    public ShockedObjectsService(IShockedObjectsRepository shockedObjectsRepository, IEffectsPlayer effectsPlayer,
        ITimeStopService timeStopService, ITimeProvider timeProvider, IGame game, ILogger logger, IMediator mediator,
        ILifetimeScope lifetimeScope)
    {
        _shockedObjectsRepository = shockedObjectsRepository;
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

    public ShockedObject Shock(IObject @object, TimeSpan shockDuration)
    {
        var shockedObject = new ShockedObject(@object, shockDuration, _effectsPlayer, _game);
        shockedObject.Initialize();

        _shockedObjectsRepository.AddShockedObject(shockedObject);

        _logger.Debug("Object '{Id}': {Name} was shocked for {Time}ms", @object.UniqueId, @object.Name,
            Math.Round(shockedObject.TimeLeft.TotalMilliseconds));

        return shockedObject;
    }

    public bool IsShocked(IObject @object)
    {
        return _shockedObjectsRepository.IsShockedObject(@object);
    }

    private void OnShockEnded(ShockedObject shockedObject, int index)
    {
        shockedObject.Dispose();
        _shockedObjectsRepository.RemoveShockedObject(shockedObject.ObjectId, index);

        _logger.Debug("Shock from object '{Id}': {Name} was removed", shockedObject.ObjectId, shockedObject.Name);
    }

    private void OnUpdate(Event<float> @event)
    {
        var elapsed = @event.Args * _timeProvider.GameSlowMoModifier;

        var shockedObjects = _shockedObjectsRepository.GetShockedObjects();
        if (shockedObjects.Count == 0)
            return;

        var isTimeStopped = _timeStopService.IsTimeStopped;

        for (var i = shockedObjects.Count - 1; i >= 0; i--)
        {
            var shockedObject = shockedObjects[i];
            var touchedObjects = shockedObject.Update(elapsed, isTimeStopped);

            if (shockedObject.Charge <= ShockedObject.ELEMENTARY_CHARGE)
            {
                OnShockEnded(shockedObject, i);
                continue;
            }

            var notification = new ObjectsTouchedShockObjectNotification(shockedObject, touchedObjects);
            _mediator.Publish(notification);
        }
    }
}