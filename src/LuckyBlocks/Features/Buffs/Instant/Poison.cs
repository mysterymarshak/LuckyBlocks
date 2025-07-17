using System;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Notifications;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Instant;

internal class Poison : IInstantBuff, IRepressibleByImmunityFlagsBuff
{
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToPoison;
    public string Name => "Poison";

    private readonly Player _player;
    private readonly int _damage;
    private readonly INotificationService _notificationService;

    public Poison(Player player, int damage, INotificationService notificationService)
        => (_player, _damage, _notificationService) = (player, damage, notificationService);

    public void Run()
    {
        var playerInstance = _player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        var health = playerInstance.GetHealth();
        playerInstance.SetHealth(health - _damage);

        _notificationService.CreateDialogueNotification("POISONED", Color.Red, TimeSpan.FromMilliseconds(2000),
            playerInstance, true);
    }
}