using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class Dwarf : DurableBuffBase
{
    public static readonly PlayerModifiers ModifiedModifiers = new()
    {
        MeleeForceModifier = 0.7f,
        SizeModifier = 0.5f,
        RunSpeedModifier = 2f,
        SprintSpeedModifier = 2f,
        EnergyRechargeModifier = 2f,
        ClimbingSpeed = 2f
    };

    public override string Name => "Dwarf";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);

    protected override Color BuffColor => ExtendedColors.Emerald;
    
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;

    private PlayerModifiers? _playerModifiers;

    public Dwarf(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
        => (_playerModifiersService, _notificationService, _args) =
            (args.PlayerModifiersService, args.NotificationService, args);

    public override IDurableBuff Clone()
    {
        return new Dwarf(Player, _args, TimeLeft);
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        _notificationService.CreateChatNotification($"You're a dwarf again for {TimeLeft.TotalSeconds}s", BuffColor,
            Player.UserIdentifier);
    }

    protected override void OnFinished()
    {
        DisableBuff();
    }

    protected override void OnRan()
    {
        var playerInstance = Player.Instance!;
        _playerModifiers = playerInstance.GetModifiers();

        EnableBuff();
        UpdateDialogue();
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
        ShowDialogue("DWARF", BuffColor, TimeLeft);
    }
}