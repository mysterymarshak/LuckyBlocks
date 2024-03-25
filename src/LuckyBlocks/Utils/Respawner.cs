using System.Linq;
using LuckyBlocks.Extensions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

internal interface IRespawner
{
    IPlayer RespawnUserAtRandomSpawnPoint(IUser user);
    IPlayer RespawnPlayer(IUser user, IProfile profile);
    IPlayer RespawnPlayer(IUser user, IProfile profile, Vector2 position, int direction);
}

internal class Respawner : IRespawner
{
    private readonly IGame _game;

    public Respawner(IGame game)
        => (_game) = (game);

    public IPlayer RespawnUserAtRandomSpawnPoint(IUser user)
    {
        var location = _game.GetObjectsByName("SpawnPlayer")
            .ToList()
            .Shuffle()
            .GetRandomElement();

        var player = user.GetPlayer();

        return RespawnPlayer(user, player?.GetProfile() ?? user.GetProfile(), location.GetWorldPosition(),
            location.GetFaceDirection());
    }

    public IPlayer RespawnPlayer(IUser user, IProfile profile)
    {
        if (user.GetPlayer() is { } existingPlayer)
        {
            var position = existingPlayer.GetWorldPosition();
            var direction = existingPlayer.GetFaceDirection();
            existingPlayer.RemoveDelayed();
            
            return CreatePlayer(user, profile, position, direction);
        }

        return RespawnUserAtRandomSpawnPoint(user);
    }
    
    public IPlayer RespawnPlayer(IUser user, IProfile profile, Vector2 position, int direction)
    {
        if (user.GetPlayer() is { } existingPlayer)
        {
            existingPlayer.RemoveDelayed();
        }
        
        return CreatePlayer(user, profile, position, direction);
    }

    private IPlayer CreatePlayer(IUser user, IProfile profile, Vector2 position, int direction)
    {
        var player = _game.CreatePlayer(position);

        player.SetProfile(profile);
        player.SetFaceDirection(direction);
        player.SetUser(user);

        if (user.IsBot)
        {
            SetBotSoul(player, user);
        }

        return player;
    }

    private void SetBotSoul(IPlayer player, IUser user)
    {
        var behavior = new BotBehavior(true, user.BotPredefinedAIType);
        player.SetBotBehavior(behavior);
    }
}