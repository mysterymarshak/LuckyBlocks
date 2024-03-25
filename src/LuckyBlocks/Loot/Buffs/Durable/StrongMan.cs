using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class StrongMan : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public static readonly PlayerModifiers ModifiedModifiers = new()
    {
        ProjectileDamageDealtModifier = 100,
        MeleeDamageDealtModifier = 100,
        MeleeForceModifier = 10,
        MeleeStunImmunity = 1
    };

    public override string Name => "Strong man";
    public override TimeSpan Duration => TimeSpan.FromSeconds(5);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToWind;
    
    protected override Color BuffColor => ExtendedColors.ImperialRed;
    
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    
    private PlayerModifiers? _playerModifiers;

    public StrongMan(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args,
        timeLeft)
        => (_playerModifiersService, _notificationService, _args) =
            (args.PlayerModifiersService, args.NotificationService, args);

    public override IDurableBuff Clone()
    {
        return new StrongMan(Player, _args, TimeLeft);
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        
        var playerInstance = Player.Instance!;
        playerInstance.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);
        
        _notificationService.CreateChatNotification($"You are strong again for {TimeLeft.TotalSeconds}s", BuffColor,
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
        var playerInstance = Player.Instance!;
        
        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
        
        playerInstance.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
    }

    private void UpdateDialogue()
    {
        ShowDialogue("STRONG", BuffColor, TimeLeft);
    }
}