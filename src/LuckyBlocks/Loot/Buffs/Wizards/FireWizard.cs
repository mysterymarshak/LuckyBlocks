using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.AreaMagic;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class FireWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Fire wizard";
    public override int CastsCount => 3;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToFire | ImmunityFlag.ImmunityToFreeze;

    protected override Color BuffColor => ExtendedColors.Orange;

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly BuffConstructorArgs _args;

    public FireWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) : base(wizard, args, castsLeft)
        => (_magicService, _magicFactory, _args) = (args.MagicService, args.MagicFactory, args);

    public override IWizard Clone()
    {
        return new FireWizard(Player, _args, CastsLeft);
    }

    protected override void OnUseMagic()
    {
        var playerInstance = Player.Instance!;
        var magic = _magicFactory.CreateAreaMagic<FireMagic>(Player, _args);
        
        _magicService.Cast(magic, playerInstance);
    }
}