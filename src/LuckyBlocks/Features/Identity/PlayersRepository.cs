using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal interface IPlayersRepository
{
    OneOf<Success, Unknown> ValidateUser(int userId);
    OneOf<Player, Unknown> GetPlayerById(int uniqueId);
    Player GetPlayerByInstance(IPlayer playerInstance);
    IEnumerable<Player> GetAlivePlayers(bool includeFakePlayers = true);
    void RemoveFakePlayer(int uniqueId);
}

internal class PlayersRepository : IPlayersRepository
{
    private readonly ILogger _logger;
    private readonly IGame _game;
    private readonly Dictionary<int, Player> _players = new();
    private readonly Dictionary<int, Player> _fakePlayers = new();

    public PlayersRepository(ILogger logger, IGame game)
    {
        _logger = logger;
        _game = game;
    }

    public OneOf<Success, Unknown> ValidateUser(int userId)
    {
        var getPlayerResult = GetPlayerByUserId(userId);
        return getPlayerResult.IsT0 ? new Success() : new Unknown();
    }

    public OneOf<Player, Unknown> GetPlayerById(int uniqueId)
    {
        var playerInstance = _game.GetPlayer(uniqueId);
        return playerInstance is null ? new Unknown() : GetPlayerByUserId(playerInstance.UserIdentifier);
    }

    public Player GetPlayerByInstance(IPlayer playerInstance)
    {
        if (playerInstance.IsFake())
        {
            return GetFakePlayer(playerInstance);
        }

        var userId = playerInstance.UserIdentifier;

        var getPlayerResult = GetPlayerByUserId(userId);
        if (!getPlayerResult.TryPickT0(out var player, out _))
        {
            _logger.Error("IPlayer: {Player}:{Name}, User: {User}", playerInstance, playerInstance.Name,
                playerInstance.GetUser());
            throw new Exception($"Player with user id '{userId}' not found.");
        }

        return player;
    }

    public IEnumerable<Player> GetAlivePlayers(bool includeFakePlayers = true)
    {
        var players = _game.GetPlayers();
        return players
            .Where(x => (includeFakePlayers ? x.IsValid() : x.IsValidUser()) && !x.IsDead)
            .Select(GetPlayerByInstance);
    }

    public void RemoveFakePlayer(int uniqueId)
    {
        if (_fakePlayers.Remove(uniqueId))
        {
            _logger.Debug("Removed fake player with id {UniqueId}", uniqueId);
        }
    }

    private Player GetFakePlayer(IPlayer playerInstance)
    {
        if (!_fakePlayers.TryGetValue(playerInstance.UniqueId, out var fakePlayer))
        {
            fakePlayer = new Player(new FakeUser(playerInstance));
            _fakePlayers.Add(playerInstance.UniqueId, fakePlayer);

            _logger.Debug("Created fake player with id {UniqueId} and name {Name}",
                playerInstance.UniqueId, playerInstance.Name);
        }

        return fakePlayer;
    }

    private OneOf<Player, Unknown> GetPlayerByUserId(int userId)
    {
        return _players.TryGetValue(userId, out var player) ? player : CreatePlayerByUserId(userId);
    }

    private OneOf<Player, Unknown> CreatePlayerByUserId(int userId)
    {
        var user = _game.GetActiveUser(userId);

        if (user is null)
        {
            return new Unknown();
        }

        return CreatePlayer(user);
    }

    private Player CreatePlayer(IUser? user)
    {
        ArgumentWasNullException.ThrowIfNull(user);

        var player = new Player(user);
        _players.Add(user.UserIdentifier, player);

        return player;
    }
}