﻿using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Immunity;

internal class ImmunityToDeath : IApplicableImmunity
{
    public string Name => "Immunity to death";
    public ImmunityFlag Flag => ImmunityFlag.ImmunityToDeath;

    private readonly Player _player;
    private readonly IRespawner _respawner;
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger _logger;
    private readonly IExtendedEvents _extendedEvents;

    public ImmunityToDeath(Player player, ImmunityConstructorArgs args, ILifetimeScope lifetimeScope)
        => (_player, _respawner, _lifetimeScope, _logger, _extendedEvents) = (player, args.Respawner, lifetimeScope, args.Logger, lifetimeScope.Resolve<IExtendedEvents>());

    public void Apply()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);
        
        _extendedEvents.HookOnDead(playerInstance, OnDead, EventHookMode.GlobalThisPre);
    }

    public void Remove()
    {
        Dispose();  
    }
    
    private bool OnDead(Event<PlayerDeathArgs> @event)
    {
        var args = @event.Args;
        if (!args.Removed)
        {
            _respawner.RespawnPlayer(_player.User, _player.Profile);
            _logger.Debug("{PlayerName} respawned from immunity to death", _player.Name);
        }
        
        Dispose();
        
        return false;
    }
    
    private void Dispose()
    {
        _lifetimeScope.Dispose();
        _extendedEvents.Clear();
    }
}