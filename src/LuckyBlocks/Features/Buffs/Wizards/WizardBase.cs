using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
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

    public int CastsLeft
    {
        get => field < 0 ? (field = GetInitialCastsCount()) : field;
        private set;
    }

    public bool IsCloned { get; private set; }

    protected bool IsUsingMagic { get; set; }

    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IMagicService _magicService;
    private readonly WizardFinishCondition _wizardFinishCondition;
    private readonly int _initialCastsLeft;

    private bool _isAwaitingFinish;

    protected WizardBase(Player wizard, BuffConstructorArgs args, int castsLeft = -1,
        WizardFinishCondition wizardFinishCondition = WizardFinishCondition.RanOutOfCasts) : base(wizard, args)
    {
        _effectsPlayer = args.EffectsPlayer;
        _wizardFinishCondition = wizardFinishCondition;
        _magicService = args.MagicService;
        _initialCastsLeft = castsLeft;
        CastsLeft = -1;
    }

    public IWizard Clone(Player? player = null)
    {
        var clonedWizard = CloneInternal(player ?? Player);
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

        // todo: decoy wizard dialogue after time revert shouldnt be displayed if there're decoys
        ShowDialogue(Name.ToUpper(), TimeSpan.FromSeconds(3), BuffColor, default, default, true);
        ShowChatMessage("[ALT + A] TO USE MAGIC");
        ShowCastsCount();

        OnRunInternal();
    }

    public void ApplyAgain(IBuff additionalBuff)
    {
        var additionalBuffCasts = ((IWizard)additionalBuff).CastsLeft;
        CastsLeft += additionalBuffCasts;

        ShowCastsCount();
    }

    public override string GetExtendedName()
    {
        return $"{Name} ({CastsLeft:D})";
    }

    protected abstract WizardBase CloneInternal(Player player);

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
        var position = playerInstance.GetWorldPosition();

        if (!CanUseMagic() || Player.IsFake() || (!_magicService.IsMagicAllowed && !IsUsingMagic))
        {
            _effectsPlayer.PlaySoundEffect("BreakGlassSmall", position);
            _effectsPlayer.PlayEffect("DestroyGlass", playerInstance.GetHandPosition());
            ShowChatMessage("You can't use magic now", ExtendedColors.ImperialRed);

            return;
        }

        _effectsPlayer.PlayHandGleamEffect(playerInstance);
        if (ShouldPlayUseSound())
        {
            _effectsPlayer.PlaySoundEffect("BarrelExplode", position);
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
        ShowChatMessage($"Casts left: {CastsLeft}");
    }

    private int GetInitialCastsCount() => IsCloned ? _initialCastsLeft : CastsCount;
}