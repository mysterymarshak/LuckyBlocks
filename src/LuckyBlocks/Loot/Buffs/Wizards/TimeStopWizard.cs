using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Features.Time;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class TimeStopWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Time stop wizard";
    public override int CastsCount => 1;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToTimeStop;

    protected override Color BuffColor => Color.Yellow;

    private readonly IMagicFactory _magicFactory;
    private readonly ITimeStopService _timeStopService;
    private readonly BuffConstructorArgs _args;

    public TimeStopWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) : base(wizard, args,
        castsLeft, WizardFinishCondition.LastCastedMagicFinishNotification)
        => (_magicFactory, _timeStopService, _args) = (args.MagicFactory, args.TimeStopService, args);

    public override IWizard Clone()
    {
        return new TimeStopWizard(Player, _args, CastsLeft);
    }

    protected override void OnUseMagic()
    {
        var magic = _magicFactory.CreateMagic<TimeStopMagic>(Player, _args);
        magic
            .WhenFinish
            .Invoke(_ => FinishCheck());
        magic.Cast();
    }

    protected override bool CanUseMagic()
    {
        return !_timeStopService.IsTimeStopped;
    }

    private void FinishCheck()
    {
        if (CastsLeft == 0)
        {
            ExternalFinish();
        }
    }
}