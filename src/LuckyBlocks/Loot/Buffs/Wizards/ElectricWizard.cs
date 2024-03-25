using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class ElectricWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Electric wizard";
    public override int CastsCount => 3;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToShock;

    protected override Color BuffColor => ExtendedColors.Electric;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly BuffConstructorArgs _args;

    public ElectricWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) :
        base(wizard, args, castsLeft) =>
        (_magicService, _magicFactory, _args) = (args.MagicService, args.MagicFactory, args);

    public override IWizard Clone()
    {
        return new ElectricWizard(Player, _args, CastsLeft);
    }

    protected override void OnUseMagic()
    {
        var playerInstance = Player.Instance!;
        var magic = _magicFactory.CreateAreaMagic<ElectricMagic>(Player, _args);

        _magicService.Cast(magic, playerInstance);
    }
}