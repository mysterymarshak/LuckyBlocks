using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Notifications;

namespace LuckyBlocks.Features.Buffs.Durable;

internal abstract class DurableRepressibleByImmunityFlagsBuffBase : DurableBuffBase,
    IDurableRepressibleByImmunityFlagsBuff
{
    public abstract ImmunityFlag ImmunityFlags { get; }

    private readonly INotificationService _notificationService;

    protected DurableRepressibleByImmunityFlagsBuffBase(Player player, BuffConstructorArgs args,
        TimeSpan timeLeft = default) : base(player, args, timeLeft)
    {
        _notificationService = args.NotificationService;
    }

    public void Repress(IFinishableBuff buff)
    {
        _notificationService.CreateChatNotification($"{Name} was repressed by immunities of '{buff.Name}'",
            buff.ChatColor, Player.UserIdentifier);
        ExternalFinish();
    }
}