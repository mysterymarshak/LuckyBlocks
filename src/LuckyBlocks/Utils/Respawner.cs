﻿using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
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
    private readonly IIdentityService _identityService;
    private readonly IGame _game;

    public Respawner(IIdentityService identityService, IGame game)
    {
        _identityService = identityService;
        _game = game;
    }

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
        var playerInstance = _game.CreatePlayer(position);

        playerInstance.SetProfile(profile);
        playerInstance.SetFaceDirection(direction);
        playerInstance.SetUser(user);

        if (user.IsBot)
        {
            SetBotSoul(playerInstance, user);
        }
        else
        {
            var player = _identityService.GetPlayerByInstance(playerInstance);
            player.InvalidateWeaponsDataOwner();
        }
        
        return playerInstance;
    }

    private void SetBotSoul(IPlayer player, IUser user)
    {
        var behavior = new BotBehavior(true, user.BotPredefinedAIType);
        player.SetBotBehavior(behavior);
    }
}