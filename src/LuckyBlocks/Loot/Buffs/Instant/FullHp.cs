using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Instant;

internal class FullHp : IInstantBuff
{
    public string Name => "Full HP";

    private readonly Player _player;
    private readonly INotificationService _notificationService;

    public FullHp(Player player, BuffConstructorArgs args)
        => (_player, _notificationService) = (player, args.NotificationService);

    public void Run()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);
        
        var maxHealth = playerInstance.GetMaxHealth();
        var healValue = maxHealth - playerInstance.GetHealth();

        playerInstance.SetHealth(maxHealth);

        _notificationService.CreateTextNotification($"+{Math.Round(healValue)}", Color.Green, TimeSpan.FromSeconds(2),
            playerInstance);
    }
}