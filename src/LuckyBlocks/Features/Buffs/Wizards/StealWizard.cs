using System;
using System.Linq;
using System.Threading;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Magic;
using LuckyBlocks.Features.Magic.NonAreaMagic;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;
using Timer = LuckyBlocks.Utils.Timers.Timer;

namespace LuckyBlocks.Features.Buffs.Wizards;

internal class StealWizard : WizardBase, IImmunityFlagsIndicatorBuff
{
    public override string Name => "Steal wizard";
    public override int CastsCount => 1;
    public ImmunityFlag ImmunityFlags => ImmunityFlag.ImmunityToSteal;
    public override Color BuffColor => ExtendedColors.NightBlack;
    public override Color ChatColor => ExtendedColors.Night;

    private static TimeSpan TimeForSteal => TimeSpan.FromSeconds(10);

    private readonly IMagicService _magicService;
    private readonly IMagicFactory _magicFactory;
    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly BuffConstructorArgs _args;
    private readonly Timer _stealTimer;
    private readonly PeriodicTimer _stealTimeWarningTimer;

    private bool _isInStealMode;
    private StealMagic? _magic;
    private int _secondsWarningCount = 3;
    private CancellationTokenSource? _cts;

    public StealWizard(Player wizard, BuffConstructorArgs args, int castsLeft = -1, TimeSpan timeLeft = default) : base(
        wizard, args, castsLeft, WizardFinishCondition.LastCastedMagicFinishNotification)
    {
        _magicService = args.MagicService;
        _magicFactory = args.MagicFactory;
        _identityService = args.IdentityService;
        _notificationService = args.NotificationService;
        _args = args;
        _stealTimer = new Timer(timeLeft == default ? TimeForSteal : timeLeft, TimeBehavior.TimeModifier,
            OnTimeForStealPassed, ExtendedEvents);
        _stealTimeWarningTimer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.TimeModifier, OnStealTimerTick,
            null, _secondsWarningCount, ExtendedEvents);
    }

    protected override WizardBase CloneInternal(Player player)
    {
        return new StealWizard(player, _args, CastsLeft, _stealTimer.TimeLeft) { _isInStealMode = _isInStealMode };
    }

    protected override void BindMagicInternal(IMagic magic)
    {
        var stealMagic = (StealMagic)magic;
        _magic = stealMagic;

        if (_isInStealMode)
        {
            _cts = new CancellationTokenSource();
            ShowStealModeHints();
            _stealTimer.Start();
        }
        else
        {
            _magic.EnterStealMode += OnStealModeEntered;
        }

        _magic.Steal += OnStole;
        _magic.StealFail += OnStealFailed;
    }

    protected override void OnRunInternal()
    {
        var magicWasCasted = IsCloned && _isInStealMode;
        if (magicWasCasted)
            return;

        _magic = _magicFactory.CreateMagic<StealMagic>(Player);
        _magic.EnterStealMode += OnStealModeEntered;
        _magic.Steal += OnStole;
        _magic.StealFail += OnStealFailed;
    }

    protected override bool CanUseMagic() => _identityService.GetAlivePlayers(false)
        .Any(x => x != Player && x.HasAnyWeapon() &&
                  !x.GetImmunityFlags().HasFlag<ImmunityFlag>(ImmunityFlag.ImmunityToSteal));

    protected override IFinishCondition<IMagic> OnUseMagic()
    {
        return _magicService.Cast(_magic!);
    }

    protected override void OnFinishInternal()
    {
        if (_magic is null)
            return;

        _cts!.Cancel();
        _cts.Dispose();
        _stealTimeWarningTimer.Stop();
        _stealTimer.Stop();
        _magic.ExternalFinish();
    }

    private void OnStealModeEntered()
    {
        _magic!.EnterStealMode -= OnStealModeEntered;

        _cts = new CancellationTokenSource();
        _isInStealMode = true;

        ShowStealModeHints();
        _stealTimer.Start();
    }

    private void ShowStealModeHints()
    {
        ShowDialogue($"{_stealTimer.TimeLeft.Seconds}s FOR STEAL!", TimeSpan.FromSeconds(2), BuffColor);
        ShowChatMessage("[ALT] for change selected player");
        ShowChatMessage("[ALT + A] to steal weapons from selected player");

        if (_stealTimer.TimeLeft > TimeSpan.FromSeconds(_secondsWarningCount + 1))
        {
            Awaiter.Start(_stealTimeWarningTimer.Start,
                _stealTimer.TimeLeft - TimeSpan.FromSeconds(_secondsWarningCount + 1), _cts!.Token);
        }
        else
        {
            _secondsWarningCount = _stealTimer.TimeLeft.Seconds;
            _stealTimeWarningTimer.SetElapsed(TimeSpan.FromMilliseconds(_stealTimer.TimeLeft.Milliseconds));
            _stealTimeWarningTimer.Start();
        }
    }

    private void OnStole()
    {
        _magic!.Steal -= OnStole;

        ShowDialogue("STOLE!", TimeSpan.FromSeconds(2), BuffColor, ignoreFinish: true);
    }

    private void OnStealFailed(string reason)
    {
        _magic!.StealFail -= OnStealFailed;

        ShowDialogue(reason, TimeSpan.FromSeconds(2), BuffColor, ignoreFinish: true);
    }

    private void OnStealTimerTick()
    {
        ShowDialogue($"{_secondsWarningCount--}s LEFT FOR STEAL!", TimeSpan.FromSeconds(1), BuffColor);
    }

    private void OnTimeForStealPassed()
    {
        _magic!.ExternalFinish();

        ShowDialogue("TIME IS UP", TimeSpan.FromSeconds(2), BuffColor, ignoreFinish: true);
    }
}