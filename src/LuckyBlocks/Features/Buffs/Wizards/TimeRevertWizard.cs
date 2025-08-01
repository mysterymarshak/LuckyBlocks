using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Features.Time.TimeRevert;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class TimeRevertWizard : WizardBase
{
    public override string Name => "Time Revert Wizard";
    public override int CastsCount => 1;
    public override Color BuffColor => ExtendedColors.KillerQueen;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly ITimeRevertService _timeRevertService;
    private readonly BuffConstructorArgs _args;

    public TimeRevertWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1) : base(wizard, args,
        castsLeft, WizardFinishCondition.LastCastedMagicFinishNotification)
    {
        _timeRevertService = args.TimeRevertService;
        _magicService = args.MagicService;
        _magicFactory = args.MagicFactory;
        _args = args;
    }

    protected override WizardBase CloneInternal(Player player)
    {
        return new TimeRevertWizard(player, _args, CastsLeft);
    }

    protected override bool CanUseMagic() => _timeRevertService.TimeCanBeReverted;

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        var magic = _magicFactory.CreateMagic<TimeRevertMagic>(Player);
        return _magicService.Cast(magic);
    }
}