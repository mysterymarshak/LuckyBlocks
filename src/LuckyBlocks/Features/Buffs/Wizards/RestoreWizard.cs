using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class RestoreWizard : WizardBase
{
    public override string Name => "Restore wizard";
    public override int CastsCount => 2;
    public override Color BuffColor => ExtendedColors.Amethyst;

    private readonly IMagicFactory _magicFactory;
    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly BuffConstructorArgs _args;
    private readonly IMagicService _magicService;

    private RestoreMagic? _magic;
    private bool _stateSaved;

    public RestoreWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1) : base(wizard, args,
        castsLeft)
    {
        _magicService = args.MagicService;
        _magicFactory = args.MagicFactory;
        _notificationService = args.NotificationService;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
    }

    protected override WizardBase CloneInternal(Player player)
    {
        return new RestoreWizard(player, _args, CastsLeft) { _stateSaved = _stateSaved };
    }

    protected override void BindMagicInternal(IMagic magic)
    {
        var restoreMagic = (RestoreMagic)magic;
        _magic = restoreMagic;

        _magic.StateRestore += OnStateRestored;
        ShowChatMessage("[ALT + A] for restore saved state");

        // if there is a magic for bind => magic casted => state saved
    }

    protected override void OnRunInternal()
    {
        if (IsCloned && _stateSaved)
            return;

        _magic = _magicFactory.CreateMagic<RestoreMagic>(Player);
        _magic.StateSave += OnStateSaved;
        _magic.StateRestore += OnStateRestored;
    }

    protected override bool ShouldPlayUseSound() => _stateSaved;

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        return _magicService.Cast(_magic!);
    }

    protected override void OnFinishInternal()
    {
        if (_magic is null)
            return;

        _magic.StateSave -= OnStateSaved;
        _magic.StateRestore -= OnStateRestored;
    }

    private void OnStateSaved()
    {
        _effectsPlayer.PlayHandGleamEffect(PlayerInstance!);

        _stateSaved = true;
        ShowChatMessage("You saved your state");
        ShowChatMessage("[ALT + A] for restore");
    }

    private void OnStateRestored()
    {
        ShowDialogue("State restored", TimeSpan.FromMilliseconds(2500), BuffColor);
    }
}