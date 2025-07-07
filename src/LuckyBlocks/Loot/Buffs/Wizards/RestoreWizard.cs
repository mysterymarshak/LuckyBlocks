using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class RestoreWizard : WizardBase
{
    public override string Name => "Restore wizard";
    public override int CastsCount => 2;

    protected override Color BuffColor => ExtendedColors.Amethyst;

    private readonly IMagicFactory _magicFactory;
    private readonly INotificationService _notificationService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly BuffConstructorArgs _args;

    private RestoreMagic? _magic;

    public RestoreWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) : base(wizard, args,
        castsLeft)
    {
        _magicFactory = args.MagicFactory;
        _notificationService = args.NotificationService;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
    }

    public override IWizard Clone()
    {
        return new RestoreWizard(Player, _args, CastsLeft);
    }

    public override void Run()
    {
        base.Run();

        _magic = _magicFactory.CreateMagic<RestoreMagic>(Player, _args);
    }

    protected override bool ShouldPlayUseSound() => CastsLeft == 0;

    protected override void OnUseMagic()
    {
        if (CastsLeft == 1)
        {
            var playerInstance = Player.Instance!;
            _effectsPlayer.PlayEffect(EffectName.ItemGleam,
                playerInstance.GetWorldPosition() + new Vector2(0, 9) +
                playerInstance.GetFaceDirection() * new Vector2(12, 0));
            
            _notificationService.CreateChatNotification("You saved your state", BuffColor);
        }
        else if (CastsLeft == 0)
        {
            ShowDialogue("State restored", BuffColor, TimeSpan.FromMilliseconds(2500));
        }

        _magic!.Cast();
    }
}