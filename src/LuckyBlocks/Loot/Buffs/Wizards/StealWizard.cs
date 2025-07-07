using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class StealWizard : WizardBase
{
    public override string Name => "Steal wizard";
    public override int CastsCount => 2;

    protected override Color BuffColor => ExtendedColors.NightBlack;
    protected override Color ChatColor => Color.Yellow;

    private readonly IMagicFactory _magicFactory;
    private readonly IIdentityService _identityService;
    private readonly BuffConstructorArgs _args;

    private StealMagic? _magic;

    public StealWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) : base(wizard, args, castsLeft)
    {
        _identityService = args.IdentityService;
        _magicFactory = args.MagicFactory;
        _args = args;
    }

    public override IWizard Clone()
    {
        return new StealWizard(Player, _args, CastsLeft);
    }

    public override void Run()
    {
        base.Run();

        _magic ??= _magicFactory.CreateMagic<StealMagic>(Player, _args);
        _magic
            .WhenFinish
            .Invoke(_ => OnStealFinish());
    }

    protected override bool CanUseMagic() =>
        _identityService.GetAlivePlayers(false).Any(x => x != Player && x.HasAnyWeapon());

    protected override void OnUseMagic()
    {
        _magic!.Cast();
    }

    protected override void OnFinish()
    {
        _magic?.ExternalFinish();
    }

    private void OnStealFinish()
    {
        ExternalFinish();
    }
}