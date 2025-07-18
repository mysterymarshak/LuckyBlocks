using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class ElectricWizard : WizardBase, IDelayedImmunityRemovalBuff
{
    public override string Name => "Electric wizard";
    public override int CastsCount => 3;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToShock;
    public TimeSpan ImmunityRemovalDelay => TimeSpan.FromSeconds(2);
    public override Color BuffColor => ExtendedColors.Electric;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly BuffConstructorArgs _args;

    public ElectricWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1) : base(wizard, args,
        castsLeft)
    {
        _magicService = args.MagicService;
        _magicFactory = args.MagicFactory;
        _args = args;
    }

    protected override WizardBase CloneInternal(Player player)
    {
        return new ElectricWizard(player, _args, CastsLeft);
    }

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        var magic = _magicFactory.CreateAreaMagic<ElectricMagic>(Player, _args);
        return _magicService.Cast(magic);
    }
}