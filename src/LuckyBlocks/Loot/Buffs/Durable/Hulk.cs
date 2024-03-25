using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class Hulk : DurableBuffBase
{
    public static readonly PlayerModifiers ModifiedModifiers = new()
    {
        SizeModifier = 2,
        RunSpeedModifier = 0.75f,
        SprintSpeedModifier = 0.75f,
        MeleeDamageDealtModifier = 3,
        MeleeForceModifier = 10,
        ExplosionDamageTakenModifier = 0.8f,
        FireDamageTakenModifier = 0.8f,
        ImpactDamageTakenModifier = 0.8f,
        MeleeDamageTakenModifier = 0.8f,
        ProjectileCritChanceTakenModifier = 0.8f,
        ProjectileDamageTakenModifier = 0.8f
    };

    public override string Name => "Hulk";
    public override TimeSpan Duration => TimeSpan.FromSeconds(10);

    protected override Color BuffColor => ExtendedColors.Emerald;

    private const string HULK_COLOR_NAME = "ClothingGreen";
    
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    
    private PlayerModifiers? _playerModifiers;

    public Hulk(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
        => (_playerModifiersService, _notificationService, _args) =
            (args.PlayerModifiersService, args.NotificationService, args);

    public override IDurableBuff Clone()
    {
        return new Hulk(Player, _args, TimeLeft);
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();

        var playerInstance = Player.Instance!;
        playerInstance.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);
        
        _notificationService.CreateChatNotification($"You are a hulk again for {TimeLeft.TotalSeconds}s", BuffColor,
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
        var profile = Player.Profile;
        var hulkProfile = profile.ToSingleColor(HULK_COLOR_NAME);
        
        playerInstance.SetProfile(hulkProfile);
        playerInstance.SetStrengthBoostTime((float)TimeLeft.TotalMilliseconds);
        
        _playerModifiersService.AddModifiers(Player, ModifiedModifiers);
    }

    private void DisableBuff()
    {
        _playerModifiersService.RevertModifiers(Player, ModifiedModifiers, _playerModifiers!);
        
        if (!Player.IsValid())
            return;
        
        var playerInstance = Player.Instance!;
        playerInstance.SetProfile(Player.Profile);
    }

    private void UpdateDialogue()
    {
        ShowDialogue("HULK", BuffColor, TimeLeft);
    }
}