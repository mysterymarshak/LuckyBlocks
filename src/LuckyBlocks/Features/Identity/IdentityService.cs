using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal interface IIdentityService
{
    void Initialize();
    OneOf<Player, Unknown> GetPlayerById(int uniqueId);
    Player GetPlayerByInstance(IPlayer player);
    IEnumerable<Player> GetAlivePlayers(bool includeFakePlayers = true);
    IEnumerable<IUser> GetDeadUsers();
}

internal class IdentityService : IIdentityService
{
    private readonly IPlayersRepository _playersRepository;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly IExtendedEvents _extendedEvents;

    public IdentityService(IPlayersRepository playersRepository, IGame game, ILogger logger, ILifetimeScope scope)
    {
        _playersRepository = playersRepository;
        _game = game;
        _logger = logger;
        var thisScope = scope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        foreach (var user in _game.GetActiveUsers())
        {
            RegisterUser(user.UserIdentifier);
        }

        _extendedEvents.HookOnUserJoined(OnUserJoined, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectDestroyed, EventHookMode.Default);
    }

    public OneOf<Player, Unknown> GetPlayerById(int uniqueId)
    {
        var getPlayerResult = _playersRepository.GetPlayerById(uniqueId);
        if (getPlayerResult.IsT1)
        {
            _logger.Warning("attempt to get unknown player with id  '{UniqueId}'", uniqueId);
        }

        return getPlayerResult;
    }

    public Player GetPlayerByInstance(IPlayer player)
    {
        return _playersRepository.GetPlayerByInstance(player);
    }

    public IEnumerable<Player> GetAlivePlayers(bool includeFakePlayers = true)
    {
        return _playersRepository.GetAlivePlayers(includeFakePlayers);
    }

    public IEnumerable<IUser> GetDeadUsers()
    {
        return _game.GetActiveUsers().Where(x => x.GetPlayer()?.IsDead ?? true);
    }

    private void OnUserJoined(Event<IUser[]> @event)
    {
        foreach (var user in @event.Args)
        {
            RegisterUser(user.UserIdentifier);
        }
    }

    private void OnObjectDestroyed(Event<IObject[]> @event)
    {
        foreach (var @object in @event.Args)
        {
            if (@object is IPlayer { IsBot: true, UserIdentifier: 0 })
            {
                _playersRepository.RemoveFakePlayer(@object.UniqueId);
            }
        }
    }

    private void RegisterUser(int userId)
    {
        var validationResult = _playersRepository.ValidateUser(userId);
        if (!validationResult.IsT0)
        {
            _logger.Error("Failed to register user with id '{UserId}': {Message}", userId, validationResult.AsT1);
            return;
        }

        _logger.Debug("Registered user with id '{UserId}'", userId);
    }
}