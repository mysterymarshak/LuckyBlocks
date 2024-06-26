﻿using System;
using Autofac;
using LuckyBlocks.Entities;
using LuckyBlocks.Repositories;
using LuckyBlocks.Utils;
using Mediator;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.ShockedObjects;

internal interface IShockedObjectsService
{
    ShockedObject Shock(IObject @object, TimeSpan shockDuration);
    bool IsShocked(IObject @object);
    void OnShockEnded(IObject @object);
}

internal class ShockedObjectsService : IShockedObjectsService
{
    private readonly IShockedObjectsRepository _shockedObjectsRepository;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    private readonly ILifetimeScope _lifetimeScope;

    public ShockedObjectsService(IShockedObjectsRepository shockedObjectsRepository, IEffectsPlayer effectsPlayer,
        IGame game, IMediator mediator, ILogger logger, ILifetimeScope lifetimeScope) =>
        (_shockedObjectsRepository, _effectsPlayer, _game, _mediator, _lifetimeScope, _logger) = (
            shockedObjectsRepository, effectsPlayer, game, mediator, lifetimeScope.BeginLifetimeScope(), logger);

    public ShockedObject Shock(IObject @object, TimeSpan shockDuration)
    {
        var shockedObject = new ShockedObject(@object, shockDuration, _effectsPlayer, _game, _mediator,
            _lifetimeScope.BeginLifetimeScope());
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

    public void OnShockEnded(IObject @object)
    {
        _shockedObjectsRepository.RemoveShockedObject(@object.UniqueId);
        _logger.Debug("Shock from object '{Id}': {Name} was removed", @object.UniqueId, @object.Name);
    }
}