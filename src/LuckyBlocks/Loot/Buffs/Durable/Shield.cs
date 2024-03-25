using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.PlayerModifiers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class Shield : DurableBuffBase, IImmunityFlagsIndicatorBuff
{
    public static readonly PlayerModifiers ModifiedModifiers = new()
    {
        ExplosionDamageTakenModifier = 0,
        FireDamageTakenModifier = 0,
        ImpactDamageTakenModifier = 0,
        MeleeDamageTakenModifier = 0,
        ProjectileDamageTakenModifier = 0,
        ProjectileCritChanceTakenModifier = 0
    };

    public override string Name => "Shield";
    public override TimeSpan Duration => TimeSpan.FromSeconds(7);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.FullDamageImmunity;

    protected override Color BuffColor => Color.Blue;
    
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;

    private PlayerModifiers? _playerModifiers;

    public Shield(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
        => (_playerModifiersService, _notificationService, _args) =
            (args.PlayerModifiersService, args.NotificationService, args);

    public override IDurableBuff Clone()
    {
        return new Shield(Player, _args, TimeLeft);
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        _notificationService.CreateChatNotification($"You are under shield again for {TimeLeft.TotalSeconds}s",
            BuffColor, Player.UserIdentifier);
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
        ShowDialogue("SHIELD", BuffColor, TimeLeft);
    }
}