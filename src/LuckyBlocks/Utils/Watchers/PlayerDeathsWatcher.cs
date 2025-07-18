using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils.Watchers;

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
    {
        _buffsService = buffsService;
        _identityService = identityService;
        _logger = logger;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _players = new Dictionary<int, Player>();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnDead(OnDeadPre, EventHookMode.GlobalSharedPre);
        _extendedEvents.HookOnDead(OnDeadPost, EventHookMode.GlobalSharedPost);
    }

    private bool OnDeadPre(Event<IPlayer> @event)
    {
        var playerInstance = @event.Args;
        if (!playerInstance.IsUser && !playerInstance.IsFake())
            return false;

        _players[playerInstance.UniqueId] = _identityService.GetPlayerByInstance(playerInstance);

        return false;
    }

    private void OnDeadPost(Event<IPlayer, PlayerDeathArgs> @event)
    {
        var playerInstance = @event.Arg1;
        var args = @event.Arg2;
        if (!_players.TryGetValue(playerInstance.UniqueId, out var player))
            return;

        _players.Remove(playerInstance.UniqueId);
        _buffsService.RemoveAllBuffs(player);

        if (player.IsFake() && args.Removed)
        {
            _identityService.RemoveFakePlayer(player);
        }
    }
}