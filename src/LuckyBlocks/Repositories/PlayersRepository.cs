using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using OneOf;
using OneOf.Types;
using SFDGameScriptInterface;

namespace LuckyBlocks.Repositories;

internal interface IPlayersRepository
{
    OneOf<Success, Unknown> ValidateUser(int userId);
    OneOf<Player, Unknown> GetPlayerByUserId(int userId);
    OneOf<Player, Unknown> GetPlayerById(int uniqueId);
    Player GetPlayerByInstance(IPlayer player);
    IEnumerable<Player> GetAlivePlayers();
}

internal class PlayersRepository : IPlayersRepository
{
    private readonly IGame _game;
    private readonly Dictionary<int, Player> _players;

    public PlayersRepository(IGame game)
        => (_game, _players) = (game, new());

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
    
    public OneOf<Player, Unknown> GetPlayerByUserId(int userId)
    {
        return _players.TryGetValue(userId, out var player) ? player : CreatePlayerByUserId(userId);
    }

    public Player GetPlayerByInstance(IPlayer instance)
    {
        var userId = instance.UserIdentifier;

        if (userId == 0)
        {
            throw new InvalidOperationException("user id was 0");
        }

        if (!_players.TryGetValue(userId, out var player))
        {
            player = CreatePlayer(instance.GetUser());
        }

        return player;
    }

    public IEnumerable<Player> GetAlivePlayers()
    {
        var players = _game.GetPlayers();
        return players
            .Where(x => x.IsUser)
            .Select(GetPlayerByInstance)
            .Where(x => x.Instance?.IsDead == false);
    }

    private OneOf<Player, Unknown> CreatePlayerByUserId(int userId)
    {
        var user = _game.GetActiveUser(userId);

        if (user is null)
            return new Unknown();

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