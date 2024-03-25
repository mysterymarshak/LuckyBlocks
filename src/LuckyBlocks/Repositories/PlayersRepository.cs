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

    public OneOf<Player, Unknown> GetPlayerByUserId(int userId)
    {
        if (!_players.TryGetValue(userId, out Player player))
        {
            var createPlayerResult = CreatePlayerByUserId(userId);

            if (createPlayerResult.TryPickT1(out var unknown, out player))
                return unknown;

            _players.Add(userId, player);
        }

        return player;
    }

    public OneOf<Player, Unknown> GetPlayerById(int uniqueId)
    {
        var playerInstance = _game.GetPlayer(uniqueId);

        if (playerInstance is null)
            return new Unknown();

        if (!_players.TryGetValue(playerInstance.UserIdentifier, out var player))
        {
            player = CreatePlayer(playerInstance.GetUser());
        }

        return player;
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
            _players.Add(userId, player);
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
        return new Player(user);
    }
}