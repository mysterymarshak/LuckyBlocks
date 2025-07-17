using LuckyBlocks.Data;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class DecoyWizard : WizardBase
{
    public override string Name => "Decoy wizard";
    public override int CastsCount => 1;

    protected override Color BuffColor => Color.White;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly BuffConstructorArgs _args;

    public DecoyWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1) : base(wizard, args, castsLeft,
        WizardFinishCondition.LastCastedMagicFinishNotification)
    {
        _magicService = args.MagicService;
        _magicFactory = args.MagicFactory;
        _args = args;
    }

    protected override WizardBase CloneInternal()
    {
        return new DecoyWizard(Player, _args, CastsLeft);
    }

    protected override bool CanUseMagic()
    {
        var playerInstance = Player.Instance!;
        return playerInstance.GetTeam() == PlayerTeam.Independent;
    }

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        var magic = _magicFactory.CreateMagic<DecoyMagic>(Player);
        return _magicService.Cast(magic);
    }
}