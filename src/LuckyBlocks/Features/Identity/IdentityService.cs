using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Mediator;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal interface IIdentityService
{
    void Initialize();
    Player RegisterFake(Player sourcePlayer, IPlayer fakeInstance);
    void RemoveFakePlayer(Player player);
    IEnumerable<Player> GetFakesForPlayer(Player sourcePlayer);
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
    private readonly IMediator _mediator;
    private readonly IExtendedEvents _extendedEvents;

    public IdentityService(IPlayersRepository playersRepository, IGame game, ILogger logger, IMediator mediator,
        ILifetimeScope scope)
    {
        _playersRepository = playersRepository;
        _game = game;
        _logger = logger;
        _mediator = mediator;
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
        _extendedEvents.HookOnUserLeft(OnUserLeft, EventHookMode.Default);
        _extendedEvents.HookOnDestroyed(OnObjectDestroyed, EventHookMode.Default);
    }

    public Player RegisterFake(Player sourcePlayer, IPlayer fakeInstance)
    {
        return _playersRepository.RegisterFake(sourcePlayer, fakeInstance);
    }

    public void RemoveFakePlayer(Player player)
    {
        _playersRepository.RemoveFakePlayer(player);
    }

    public IEnumerable<Player> GetFakesForPlayer(Player sourcePlayer)
    {
        return _playersRepository.GetFakesForPlayer(sourcePlayer);
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

    private void OnUserLeft(Event<IUser[], DisconnectionType> @event)
    {
        var users = @event.Arg1;
        foreach (var user in users)
        {
            UnregisterUser(user.UserIdentifier);
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

    private void RegisterUser(int userIdentifier)
    {
        var validationResult = _playersRepository.ValidateUser(userIdentifier);
        if (!validationResult.TryPickT0(out var player, out _))
        {
            _logger.Error("Failed to register user with id '{UserId}': {Message}", userIdentifier,
                validationResult.AsT1);
            return;
        }

        var notification = new UserRegisteredNotification(player);
        _mediator.Publish(notification);

        _logger.Debug("Registered user with id '{UserId}'", userIdentifier);
    }

    private void UnregisterUser(int userIdentifier)
    {
        _playersRepository.UnregisterUser(userIdentifier);
        _logger.Debug("Unregistered user with id '{UserId}'", userIdentifier);
    }
}