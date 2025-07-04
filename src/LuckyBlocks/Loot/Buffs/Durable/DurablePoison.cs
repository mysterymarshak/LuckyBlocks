using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
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
        ShowDialogue("Poisoned", BuffColor, TimeSpan.FromMilliseconds(Math.Min(3000, TimeLeft.TotalMilliseconds)));
        ShowMessage();

        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier, OnTick, default, int.MaxValue,
            ExtendedEvents);
        _timer.Start();
    }

    protected override void OnAppliedAgain()
    {
        ShowMessage();
    }

    protected override void OnFinished()
    {
        _timer.Stop();
    }

    private void OnTick()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        var damage = TotalDamage / Duration.TotalSeconds;
        playerInstance.SetHealth(playerInstance.GetHealth() - (float)damage);
    }

    private void ShowMessage()
    {
        _notificationService.CreateChatNotification($"You are poisoned for {TimeLeft.TotalSeconds}s", BuffColor,
            Player.UserIdentifier);
    }
}