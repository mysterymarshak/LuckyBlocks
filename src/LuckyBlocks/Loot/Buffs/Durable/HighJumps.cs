using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class HighJumps : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public static readonly PlayerModifiers ModifiedModifiers = new()
    {
        JumpHeight = 2f
    };

    public override string Name => "High jumps";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFall;

    protected override Color BuffColor => Color.White;

    private readonly INotificationService _notificationService;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly BuffConstructorArgs _args;

    private PlayerModifiers? _playerModifiers;

    public HighJumps(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) :
        base(player, args, timeLeft) => (_notificationService, _playerModifiersService, _args) =
        (args.NotificationService, args.PlayerModifiersService, args);

    public override IDurableBuff Clone()
    {
        return new HighJumps(Player, _args, TimeLeft);
    }

    protected override void OnRan()
    {
        var playerInstance = Player.Instance!;
        _playerModifiers = playerInstance.GetModifiers();

        EnableBuff();
        UpdateDialogue();
    }

    protected override void OnFinished()
    {
        DisableBuff();
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        _notificationService.CreateChatNotification($"You are a strong-legs man again for {TimeLeft.TotalSeconds}s",
            BuffColor, Player.UserIdentifier);
    }

    private void EnableBuff()
    {
        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
    }

    private void UpdateDialogue()
    {
        ShowDialogue("HIGH JUMPS", BuffColor, TimeLeft);
    }
}