using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class DurablePoison : DurableBuffBase, IRepressibleByImmunityFlagsBuff
{
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToPoison;
    public override string Name => "Poison";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);

    protected override Color BuffColor => ExtendedColors.SwampGreen;

    private const string PoisonedColorName = "ClothingDarkGreen";
    private const float TotalDamage = 30f;

    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;

    private TimerBase _timer = null!;

    public DurablePoison(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default, bool cloned = false) :
        base(player, args, timeLeft, cloned)
    {
        _notificationService = args.NotificationService;
        _args = args;
    }

    public override IDurableBuff Clone()
    {
        return new DurablePoison(Player, _args, TimeLeft);
    }

    protected override void OnRan()
    {
        ShowDialogue("POISONED", BuffColor, TimeLeft);
        ShowMessage();

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier, OnTick, default, int.MaxValue,
            ExtendedEvents);
        _timer.Start();
        OnTick();

        var profile = Player.Profile;
        var playerInstance = Player.Instance!;
        var poisonedProfile = profile.ToSingleColor(PoisonedColorName);
        playerInstance.SetProfile(poisonedProfile);
    }

    protected override void OnAppliedAgain()
    {
        ShowMessage();
    }

    protected override void OnFinished()
    {
        _timer.Stop();

        if (Player.Instance?.IsValid() == true)
        {
            var playerInstance = Player.Instance!;
            playerInstance.SetProfile(Player.Profile);
        }
    }

    private void OnTick()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        var damage = TotalDamage / Duration.TotalSeconds;
        playerInstance.SetHealth(playerInstance.GetHealth() - (float)damage);

        _notificationService.CreateTextNotification($"-{damage}", ExtendedColors.SwampGreen,
            TimeSpan.FromMilliseconds(1000), playerInstance);
    }

    private void ShowMessage()
    {
        _notificationService.CreateChatNotification($"You are poisoned for {TimeLeft.TotalSeconds}s", BuffColor,
            Player.UserIdentifier);
    }
}