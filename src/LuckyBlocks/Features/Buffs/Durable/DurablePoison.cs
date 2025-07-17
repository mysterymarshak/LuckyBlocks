using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Notifications;
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

    private const string PoisonedColorName = "ClothingDarkGreen";
    private const float TotalDamage = 30f;

    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    private readonly TimerBase _timer;

    public DurablePoison(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
    {
        _notificationService = args.NotificationService;
        _args = args;
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier, OnTick, null, int.MaxValue,
            ExtendedEvents);
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new DurablePoison(Player, _args, TimeLeft);
    }

    protected override void OnRunInternal()
    {
        ShowPersistentDialogue("POISONED");
        ShowChatMessage();

        var profile = Player.Profile;
        var poisonedProfile = profile.ToSingleColor(PoisonedColorName);
        PlayerInstance!.SetProfile(poisonedProfile);

        _timer.Start();
        OnTick();
    }

    protected override void OnApplyAgainInternal()
    {
        ShowChatMessage();
    }

    protected override void OnFinishInternal()
    {
        _timer.Stop();

        if (Player.IsInstanceValid())
        {
            PlayerInstance!.SetProfile(Player.Profile);
        }
    }

    private void OnTick()
    {
        ArgumentWasNullException.ThrowIfNull(PlayerInstance);

        var damage = TotalDamage / Duration.TotalSeconds;
        PlayerInstance.SetHealth(PlayerInstance.GetHealth() - (float)damage);

        _notificationService.CreateTextNotification($"-{damage}", ExtendedColors.SwampGreen,
            TimeSpan.FromMilliseconds(1000), PlayerInstance);
    }

    private void ShowChatMessage()
    {
        ShowChatMessage($"You are poisoned for {TimeLeft.TotalSeconds}s");
    }
}