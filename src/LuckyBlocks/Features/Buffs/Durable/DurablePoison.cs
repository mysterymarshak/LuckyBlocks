using System;
using System.Globalization;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.Profiles;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class DurablePoison : DurableRepressibleByImmunityFlagsBuffBase
{
    public override ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToPoison;
    public override string Name => "Poison";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);
    public override Color BuffColor => ExtendedColors.SwampGreen;

    private const float TotalDamage = 30f;

    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    private readonly TimerBase _timer;
    private readonly IProfilesService _profilesService;

    public DurablePoison(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
    {
        _notificationService = args.NotificationService;
        _profilesService = args.ProfilesService;
        _args = args;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier, OnTick, null, int.MaxValue,
            ExtendedEvents);
    }

    protected override DurableBuffBase CloneInternal(Player player)
    {
        return new DurablePoison(player, _args, TimeLeft);
    }

    protected override void OnRunInternal()
    {
        ShowPersistentDialogue("POISONED");

        _profilesService.RequestProfileChanging<DurablePoison>(Player);

        _timer.Start();
        OnTick();
    }

    protected override void OnApplyAgainInternal()
    {
        ShowApplyAgainChatMessage("poisoned");
    }

    protected override void OnFinishInternal()
    {
        _timer.Stop();
        _profilesService.RequestProfileRestoring<DurablePoison>(Player);
    }

    private void OnTick()
    {
        ArgumentWasNullException.ThrowIfNull(PlayerInstance);

        var damage = TotalDamage / Duration.TotalSeconds;
        PlayerInstance.SetHealth(PlayerInstance.GetHealth() - (float)damage);

        _notificationService.CreateTextNotification($"-{damage}", ExtendedColors.SwampGreen,
            TimeSpan.FromMilliseconds(1000), PlayerInstance);
    }
}