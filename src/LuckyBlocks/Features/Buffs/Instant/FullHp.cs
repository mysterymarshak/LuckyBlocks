using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Instant;

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