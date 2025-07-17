using System;
using System.Threading;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Durable;

internal class Freeze : DurableBuffBase, IRepressibleByImmunityFlagsBuff
{
    public override string Name => "Freeze";
    public override TimeSpan Duration => TimeSpan.FromSeconds(5);
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFreeze;

    protected override Color BuffColor => ExtendedColors.Electric;

    private const string FreezeColorName = "ClothingBlue";

    private readonly BuffConstructorArgs _args;

    private bool _isBurning;
    private CancellationTokenSource? _burningEventCts;

    public Freeze(Player player, BuffConstructorArgs args, TimeSpan timeLeft = default) : base(player, args, timeLeft)
    {
        _args = args;
    }

    protected override DurableBuffBase CloneInternal()
    {
        return new Freeze(Player, _args, TimeLeft);
    }

    protected override void OnRunInternal()
    {
        EnableBuff();
        UpdateDialogue();
    }

    protected override void OnApplyAgainInternal()
    {
        UpdateDialogue();
        ShowChatMessage($"You're frozen again for {TimeLeft.TotalSeconds}s");
    }

    protected override void OnFinishInternal()
    {
        DisableBuff();

        _burningEventCts?.Cancel();
        _burningEventCts?.Dispose();
    }

    private void EnableBuff()
    {
        var profile = Player.Profile;
        var frozenProfile = profile.ToSingleColor(FreezeColorName);

        PlayerInstance!.SetProfile(frozenProfile);
        PlayerInstance.SetInputMode(PlayerInputMode.Disabled);

        ExtendedEvents.HookOnDamage(PlayerInstance, OnDamage, EventHookMode.Default);
    }

    private void DisableBuff()
    {
        if (!Player.IsInstanceValid())
            return;

        PlayerInstance!.SetInputMode(PlayerInputMode.Enabled);
        PlayerInstance.SetProfile(Player.Profile);
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
        ShowPersistentDialogue("FREEZE");
    }
}