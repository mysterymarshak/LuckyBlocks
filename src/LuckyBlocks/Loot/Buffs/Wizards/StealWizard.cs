using System;
using System.Linq;
using System.Threading;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Buffs.Wizards;

internal class StealWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Steal wizard";
    public override int CastsCount => 2;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToSteal;

    protected override Color BuffColor => ExtendedColors.NightBlack;
    protected override Color ChatColor => Color.Yellow;

    private static TimeSpan TimeForSteal => TimeSpan.FromSeconds(10);

    private readonly IMagicFactory _magicFactory;
    private readonly IIdentityService _identityService;
    private readonly BuffConstructorArgs _args;
    private readonly PeriodicTimer _stealTimer;

    private StealMagic? _magic;
    private int _secondsLeft = 3;
    private CancellationTokenSource? _cts;

    public StealWizard(Player wizard, BuffConstructorArgs args, int castsLeft = default) : base(wizard, args, castsLeft)
    {
        _identityService = args.IdentityService;
        _magicFactory = args.MagicFactory;
        _args = args;
        _stealTimer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier, OnStealTimerTick, null, 3,
            ExtendedEvents);
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

        if (CastsLeft == 1)
        {
            ShowDialogue($"{TimeForSteal.Seconds}s FOR STEAL!", BuffColor, TimeSpan.FromSeconds(2));

            _cts = new CancellationTokenSource();

            Awaiter.Start(() => _stealTimer.Start(), TimeForSteal - TimeSpan.FromSeconds(4), _cts.Token);
            Awaiter.Start(OnTimeForStealPassed, TimeForSteal, _cts.Token);
        }
    }

    protected override void OnFinish()
    {
        if (_magic is null)
            return;

        _cts!.Cancel();
        _cts.Dispose();

        _stealTimer.Stop();
        _magic.ExternalFinish();

        if (_magic.IsStole)
        {
            ShowDialogue("STOLE!", BuffColor, TimeSpan.FromSeconds(2));
        }
        else if (_magic.NoOneHasWeapon)
        {
            ShowDialogue("NO WEAPONS ON THE MAP!", BuffColor, TimeSpan.FromSeconds(2));
        }
    }

    private void OnStealTimerTick()
    {
        ShowDialogue($"{_secondsLeft--}s LEFT FOR STEAL!", BuffColor, TimeSpan.FromSeconds(1));
    }

    private void OnTimeForStealPassed()
    {
        ExternalFinish();
    }

    private void OnStealFinish()
    {
        ExternalFinish();
    }
}