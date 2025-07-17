using System;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

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
    public bool IsCloned { get; private set; }

    protected virtual Color ChatColor => BuffColor;

    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly WizardFinishCondition _wizardFinishCondition;
    private readonly int _initialCastsLeft;

    private bool _isAwaitingFinish;

    protected WizardBase(Player wizard, BuffConstructorArgs args, int castsLeft = -1,
        WizardFinishCondition wizardFinishCondition = WizardFinishCondition.RanOutOfCasts) : base(wizard, args)
    {
        _notificationService = args.NotificationService;
        _effectsPlayer = args.EffectsPlayer;
        _wizardFinishCondition = wizardFinishCondition;
        _initialCastsLeft = castsLeft;
    }

    public IWizard Clone()
    {
        var clonedWizard = CloneInternal();
        clonedWizard.IsCloned = true;
        return clonedWizard;
    }

    public void BindMagic(IMagic magic)
    {
        if (CastsLeft == 0 && _wizardFinishCondition == WizardFinishCondition.LastCastedMagicFinishNotification)
        {
            var whenFinish = magic.WhenFinish;
            AwaitFinish(whenFinish);
        }

        BindMagicInternal(magic);
    }

    public sealed override void Run()
    {
        CastsLeft = GetInitialCastsCount();

        ExtendedEvents.HookOnPlayerMeleeAction(PlayerInstance!, OnMeleeAction, EventHookMode.Default);

        ShowDialogue(Name.ToUpper(), TimeSpan.FromSeconds(3), BuffColor, default, default, true);
        _notificationService.CreateChatNotification("[ALT + A] TO USE MAGIC", ChatColor, Player.UserIdentifier);
        ShowCastsCount();

        OnRunInternal();
    }

    public void ApplyAgain(IBuff additionalBuff)
    {
        var additionalBuffCasts = ((IWizard)additionalBuff).CastsLeft;
        CastsLeft += additionalBuffCasts;

        ShowCastsCount();
    }

    protected abstract WizardBase CloneInternal();

    protected virtual void BindMagicInternal(IMagic magic)
    {
    }

    protected virtual void OnRunInternal()
    {
    }

    protected virtual bool CanUseMagic() => true;
    protected virtual bool ShouldPlayUseSound() => true;
    protected abstract IFinishCondition<IMagic> OnUseMagic();

    private void OnMeleeAction(Event<PlayerMeleeHitArg[]> @event)
    {
        var playerInstance = Player.Instance!;

        if (playerInstance is { IsMeleeAttacking: false, IsJumpAttacking: false } || !playerInstance.IsWalking)
            return;

        // if (CastsLeft == 0)
        //     return;

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

        if (ShouldPlayUseSound())
        {
            _effectsPlayer.PlaySoundEffect("BarrelExplode", playerInstance.GetWorldPosition());
        }

        var whenFinish = OnUseMagic();

        if (_isAwaitingFinish)
            return;

        CastsLeft--;
        ShowCastsCount();

        if (CastsLeft != 0)
            return;

        switch (_wizardFinishCondition)
        {
            case WizardFinishCondition.RanOutOfCasts:
                InternalFinish();
                break;
            case WizardFinishCondition.LastCastedMagicFinishNotification:
                AwaitFinish(whenFinish);
                break;
        }
    }

    private void AwaitFinish(IFinishCondition<IMagic> whenFinish)
    {
        whenFinish.Invoke(_ => InternalFinish());
        _isAwaitingFinish = true;
    }

    private void ShowCastsCount()
    {
        _notificationService.CreateChatNotification($"Casts left: {CastsLeft}", ChatColor, Player.UserIdentifier);
    }

    private int GetInitialCastsCount() => IsCloned ? _initialCastsLeft : CastsCount;
}