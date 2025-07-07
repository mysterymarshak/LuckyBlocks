using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal enum WizardFinishCondition
{
    None = 0,

    RanOutOfCasts,

    LastCastedMagicFinishNotification
}

internal abstract class WizardBase : FinishableBuffBase, IWizard
{
    public abstract int CastsCount { get; }
    public int CastsLeft { get; private set; }

    protected virtual Color ChatColor => BuffColor;

    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly WizardFinishCondition _wizardFinishCondition;

    private bool _disposed;

    protected WizardBase(Player wizard, BuffConstructorArgs args, int castsLeft = default,
        WizardFinishCondition wizardFinishCondition = WizardFinishCondition.RanOutOfCasts) :
        base(wizard, args.NotificationService, args.LifetimeScope) =>
        (_notificationService, _effectsPlayer, _wizardFinishCondition, CastsLeft) = (args.NotificationService,
            args.EffectsPlayer, wizardFinishCondition, castsLeft == default ? CastsCount : castsLeft);

    public override void Run()
    {
        var playerInstance = Player.Instance;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        ExtendedEvents.HookOnPlayerMeleeAction(playerInstance, OnMeleeAction, EventHookMode.Default);

        _notificationService.CreateChatNotification($"{Player.Name} is {Name.ToUpper()}", ChatColor);
        ShowDialogue(Name.ToUpper(), BuffColor, TimeSpan.FromSeconds(3), default, default, true);

        _notificationService.CreateChatNotification("[ALT + A] TO USE MAGIC", ChatColor, Player.UserIdentifier);
        ShowCastsCount();
    }

    public void ApplyAgain(IBuff additionalBuff)
    {
        var additionalBuffCasts = ((IWizard)additionalBuff).CastsLeft;
        CastsLeft += additionalBuffCasts;

        ShowCastsCount();
    }

    public override void ExternalFinish()
    {
        OnFinishInternal();
    }

    public abstract IWizard Clone();

    protected virtual bool CanUseMagic() => true;
    protected virtual bool ShouldPlayUseSound() => true;
    protected abstract void OnUseMagic();

    protected virtual void OnFinish()
    {
    }

    private void OnMeleeAction(Event<PlayerMeleeHitArg[]> @event)
    {
        var playerInstance = Player.Instance!;

        if (playerInstance is { IsMeleeAttacking: false, IsJumpAttacking: false } || !playerInstance.IsWalking)
            return;

        if (CastsLeft == 0)
            return;

        UseMagic();
    }

    private void UseMagic()
    {
        var playerInstance = Player.Instance!;

        if (!CanUseMagic())
        {
            _notificationService.CreateChatNotification("You can't use magic now", ExtendedColors.ImperialRed,
                Player.UserIdentifier);
            return;
        }

        CastsLeft--;

        if (ShouldPlayUseSound())
        {
            _effectsPlayer.PlaySoundEffect("BarrelExplode", playerInstance.GetWorldPosition());
        }

        OnUseMagic();
        ShowCastsCount();

        if (CastsLeft == 0 && _wizardFinishCondition == WizardFinishCondition.RanOutOfCasts)
        {
            OnFinishInternal();
        }
    }

    private void ShowCastsCount()
    {
        _notificationService.CreateChatNotification($"Casts left: {CastsLeft}", Color.Yellow, Player.UserIdentifier);
    }

    private void OnFinishInternal()
    {
        if (_disposed)
            return;
        
        _disposed = true;
        
        ExtendedEvents.Clear();
        LifetimeScope.Dispose();
        SendFinishNotification();
        OnFinish();
    }
}