using System;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class RespawnRandomPlayer : ILoot
{
    public Item Item => Item.RespawnRandomPlayer;
    public string Name => "Respawn random player";

    private readonly IRespawner _respawner;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;

    public RespawnRandomPlayer(LootConstructorArgs args)
        => (_respawner, _identityService, _notificationService) =
            (args.Respawner, args.IdentityService, args.NotificationService);

    public void Run()
    {
        var userToRespawn = _identityService
            .GetDeadUsers()
            .ToList()
            .Shuffle()
            .GetRandomElement();

        var playerInstance = _respawner.RespawnUserAtRandomSpawnPoint(userToRespawn);
        var player = _identityService.GetPlayerByInstance(playerInstance);
        playerInstance.SetHealth(playerInstance.GetMaxHealth() / 2);

        _notificationService.CreateChatNotification($"{userToRespawn.Name} WAS RESPAWNED", Color.White);
        _notificationService.CreateDialogueNotification("RESPAWNED", Color.White, TimeSpan.FromSeconds(3), playerInstance);
    }
}