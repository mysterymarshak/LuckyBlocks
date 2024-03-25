using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class HighJumps : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "High jumps";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFall;

    protected override Color BuffColor => Color.White;

    private const int JUMP_VELOCITY = 14;

    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    private readonly Action _cachedBoostAction;

    private JumpsWatcher? _jumpsWatcher;

    public HighJumps(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
        => (_notificationService, _args, _cachedBoostAction) = (args.NotificationService, args, Boost);

    public override IDurableBuff Clone()
    {
        return new HighJumps(Player, _args, TimeLeft);
    }

    protected override void OnRan()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        _jumpsWatcher = new(playerInstance, ExtendedEvents);
        _jumpsWatcher.Jump += OnJump;
        _jumpsWatcher.Start();

        UpdateDialogue();
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        _notificationService.CreateChatNotification($"You are a strong-legs man again for {TimeLeft.TotalSeconds}s",
            BuffColor, Player.UserIdentifier);
    }

    protected override void OnFinished()
    {
        _jumpsWatcher?.Dispose();
    }

    private void OnJump()
    {
        Awaiter.Start(_cachedBoostAction, TimeSpan.Zero);
    }

    private void Boost()
    {
        var playerInstance = Player.Instance;
        if (!Player.IsValid() || playerInstance!.IsDead)
            return;

        var velocity = playerInstance.GetLinearVelocity();
        playerInstance.SetLinearVelocity(new Vector2(velocity.X, JUMP_VELOCITY));
    }

    private void UpdateDialogue()
    {
        ShowDialogue("HIGH JUMPS", BuffColor, TimeLeft);
    }
}