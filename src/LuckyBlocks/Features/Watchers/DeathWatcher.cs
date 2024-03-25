using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IPlayerDeathsWatcher
{
    void Initialize();
}

internal class PlayerDeathsWatcher : IPlayerDeathsWatcher
{
    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    private readonly IExtendedEvents _extendedEvents;
    private readonly ILogger _logger;
    private readonly Dictionary<int, Player> _players;

    public PlayerDeathsWatcher(IBuffsService buffsService, IIdentityService identityService, ILogger logger,
        ILifetimeScope lifetimeScope)
        => (_buffsService, _identityService, _logger, _extendedEvents, _players) = (buffsService, identityService,
            logger, lifetimeScope.BeginLifetimeScope().Resolve<IExtendedEvents>(), new());

    public void Initialize()
    {
        _extendedEvents.HookOnDead(OnDeadPre, EventHookMode.GlobalSharedPre);
        _extendedEvents.HookOnDead(OnDeadPost, EventHookMode.GlobalSharedPost);
    }

    private bool OnDeadPre(Event<IPlayer> @event)
    {
        var playerInstance = @event.Args;
        if (!playerInstance.IsUser)
            return false;
        
        _players[playerInstance.UniqueId] = _identityService.GetPlayerByInstance(playerInstance);
        
        return false;
    }

    private void OnDeadPost(Event<IPlayer> @event)
    {
        var playerInstance = @event.Args;
        if (!_players.TryGetValue(playerInstance.UniqueId, out var player))
            return;

        _players.Remove(playerInstance.UniqueId);
        
        _buffsService.RemoveAllBuffs(player);
    }
}