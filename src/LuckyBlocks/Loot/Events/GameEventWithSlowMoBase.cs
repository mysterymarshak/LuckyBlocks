using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Utils;

namespace LuckyBlocks.Loot.Events;

internal abstract class GameEventWithSlowMoBase : ILoot
{
    public abstract Item Item { get; }
    public abstract string Name { get; }

    protected abstract TimeSpan SlowMoDuration { get; }

    private readonly IEffectsPlayer _effectsPlayer;
    private readonly INotificationService _notificationService;

    protected GameEventWithSlowMoBase(LootConstructorArgs args)
    {
        _effectsPlayer = args.EffectsPlayer;
        _notificationService = args.NotificationService;
    }

    public void Run()
    {
        _effectsPlayer.PlaySloMoEffect(SlowMoDuration);
        _notificationService.CreatePopupNotification(Name.ToUpper(), ExtendedColors.ImperialRed, SlowMoDuration);

        Awaiter.Start(OnSlowMoEnded, SlowMoDuration);
    }

    protected abstract void OnSlowMoEnded();
}