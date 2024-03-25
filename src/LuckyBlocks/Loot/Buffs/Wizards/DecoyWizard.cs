using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class DecoyWizard : WizardBase
{
    public override string Name => "Decoy wizard";
    public override int CastsCount => 1;

    protected override Color BuffColor => Color.White;

    private readonly IMagicFactory _magicFactory;
    private readonly BuffConstructorArgs _args;

    public DecoyWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) :
        base(wizard, args, castsLeft) => (_magicFactory, _args) = (args.MagicFactory, args);

    public override IWizard Clone()
    {
        return new DecoyWizard(Player, _args, CastsLeft);
    }

    protected override void OnUseMagic()
    {
        var magic = _magicFactory.CreateMagic<DecoyMagic>(Player, _args);
        magic.Cast();
    }

    protected override bool CanUseMagic()
    {
        var playerInstance = Player.Instance!;
        return playerInstance.GetTeam() == PlayerTeam.Independent;
    }
}