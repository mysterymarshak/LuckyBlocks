using LuckyBlocks.Data;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Features.Time.TimeStop;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class TimeStopWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Time stop wizard";
    public override int CastsCount => 1;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToTimeStop;

    protected override Color BuffColor => Color.Yellow;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly ITimeStopService _timeStopService;
    private readonly BuffConstructorArgs _args;

    public TimeStopWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1) : base(wizard, args,
        castsLeft, WizardFinishCondition.LastCastedMagicFinishNotification)
    {
        _magicService = args.MagicService;
        _magicFactory = args.MagicFactory;
        _timeStopService = args.TimeStopService;
        _args = args;
    }

    protected override WizardBase CloneInternal()
    {
        return new TimeStopWizard(Player, _args, CastsLeft);
    }

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        var magic = _magicFactory.CreateMagic<TimeStopMagic>(Player);
        return _magicService.Cast(magic);
    }

    protected override bool CanUseMagic() => !_timeStopService.IsTimeStopped;
}