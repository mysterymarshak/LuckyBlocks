using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Entities;
using LuckyBlocks.Repositories;
using OneOf;
using OneOf.Types;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Identity;

internal interface IIdentityService
{
    OneOf<Player, Unknown> GetPlayerByUserId(int userId);
    OneOf<Player, Unknown> GetPlayerById(int uniqueId);
    Player GetPlayerByInstance(IPlayer player);
    IEnumerable<Player> GetAlivePlayers();
    IEnumerable<IUser> GetDeadUsers();
}

internal class IdentityService : IIdentityService
{
    private readonly IPlayersRepository _playersRepository;
    private readonly IGame _game;
    private readonly ILogger _logger;

    public IdentityService(IPlayersRepository playersRepository, IGame game, ILogger logger)
        => (_playersRepository, _game, _logger) = (playersRepository, game, logger);

    public OneOf<Player, Unknown> GetPlayerByUserId(int userId)
    {
        var getPlayerResult = _playersRepository.GetPlayerByUserId(userId);

        if (getPlayerResult.IsT1)
        {
            _logger.Warning("attempt to get unknown player with user id  '{UserId}'", userId);
        }

        return getPlayerResult;
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

    public IEnumerable<Player> GetAlivePlayers()
    {
        return _playersRepository.GetAlivePlayers();
    }

    public IEnumerable<IUser> GetDeadUsers()
    {
        return _game.GetActiveUsers().Where(x => x.GetPlayer()?.IsDead ?? true);
    }
}