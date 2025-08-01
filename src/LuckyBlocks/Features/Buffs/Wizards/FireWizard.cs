using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class FireWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Fire Wizard";
    public override int CastsCount => 3;

    public ImmunityFlag ImmunityFlags =>
        ImmunityFlag.ImmunityToFire | ImmunityFlag.ImmunityToFreeze | ImmunityFlag.ImmunityToWater;

    public override Color BuffColor => ExtendedColors.Orange;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly BuffConstructorArgs _args;

    public FireWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1) : base(wizard, args, castsLeft)
        => (_magicService, _magicFactory, _args) = (args.MagicService, args.MagicFactory, args);

    protected override WizardBase CloneInternal(Player player)
    {
        return new FireWizard(player, _args, CastsLeft);
    }

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        var magic = _magicFactory.CreateAreaMagic<FireMagic>(Player);
        return _magicService.Cast(magic);
    }
}