using System;
using System.Threading;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Durable;

internal class Freeze : DurableBuffBase, IRepressibleByImmunityFlagsBuff
{
    public override string Name => "Freeze";
    public override TimeSpan Duration => TimeSpan.FromSeconds(5);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFreeze;
    
    protected override Color BuffColor => ExtendedColors.Electric;

    private const string FREEZE_COLOR_NAME = "ClothingBlue";
    
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    
    private bool _isBurning;
    private CancellationTokenSource? _burningEventCts;

    public Freeze(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
        => (_notificationService, _args) = (args.NotificationService, args);

    public override IDurableBuff Clone()
    {
        return new Freeze(Player, _args, TimeLeft) { _isBurning = _isBurning };
    }

    protected override void OnAppliedAgain()
    {
        UpdateDialogue();
        _notificationService.CreateChatNotification($"You're frozen again for {TimeLeft.TotalSeconds}s", BuffColor,
            Player.UserIdentifier);
    }

    protected override void OnFinished()
    {
        DisableBuff();
        
        _burningEventCts?.Cancel();        
        _burningEventCts?.Dispose();        
        
        ExtendedEvents.Clear();
    }
    
    protected override void OnRan()
    {
        EnableBuff();
        UpdateDialogue();
    }

    private void EnableBuff()
    {
        var profile = Player.Profile;
        var frozenProfile = profile.ToSingleColor(FREEZE_COLOR_NAME);
        var playerInstance = Player.Instance!;

        playerInstance.SetProfile(frozenProfile);
        playerInstance.SetInputMode(PlayerInputMode.Disabled);

        ExtendedEvents.HookOnDamage(playerInstance, OnDamage, EventHookMode.Default);
    }

    private void DisableBuff()
    {
        if (!Player.IsValid())
            return;

        var playerInstance = Player.Instance!;
        playerInstance.SetInputMode(PlayerInputMode.Enabled);
        playerInstance.SetProfile(Player.Profile);
    }

    private void OnDamage(Event<PlayerDamageArgs> @event)
    {
        var isBurnDamage = @event.Args.DamageType == PlayerDamageEventType.Fire;
        if (!isBurnDamage)
            return;
            
        if (_isBurning)
            return;

        _isBurning = true;

        _burningEventCts = new CancellationTokenSource();
        Awaiter.Start(ExternalFinish, TimeSpan.FromMilliseconds(300), _burningEventCts.Token);
    }

    private void UpdateDialogue()
    {
        ShowDialogue("FREEZE", BuffColor, TimeLeft);
    }
}