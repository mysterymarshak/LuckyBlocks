using System;
using System.Threading;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.PlayerModifiers;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;
using SFDPlayerModifiers = SFDGameScriptInterface.PlayerModifiers;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class TimeStopMagic : NonAreaMagicBase
{
    public override string Name => "Time stop magic";

    private static TimeSpan TimeStopDuration => TimeSpan.FromSeconds(7);

    private readonly ITimeStopService _timeStopService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly INotificationService _notificationService;
    private readonly IPlayerModifiersService _playerModifiersService;
    private readonly MagicConstructorArgs _args;

    private TimeStopEffect? _timeStopEffect;
    private CancellationTokenSource? _timeStoppingCts;
    private CancellationTokenSource? _resumeTimeCts;
    private SFDPlayerModifiers? _playerModifiers;

    public TimeStopMagic(Player wizard, MagicConstructorArgs args) : base(wizard, args)
    {
        _timeStopService = args.TimeStopService;
        _effectsPlayer = args.EffectsPlayer;
        _game = args.Game;
        _notificationService = args.NotificationService;
        _playerModifiersService = args.PlayerModifiersService;
        _args = args;
    }

    public override void Cast()
    {
        _game.RunCommand("/slomo 0");

        _timeStoppingCts = new CancellationTokenSource();

        GiveBoostsToWizard();
        PlayTimeStopEffect();

        var wizardInstance = Wizard.Instance!;

        _timeStopService.StopTime(TimeStopDuration, wizardInstance);
        _notificationService.CreateDialogueNotification("TOKI WO TOMARE!", Color.Yellow, _timeStopService.TimeStopDelay,
            wizardInstance, realTime: true);

        ExtendedEvents.HookOnDead(wizardInstance, OnDead, EventHookMode.Default);
        Awaiter.Start(OnTimeStop, _timeStopService.TimeStopDelay, _timeStoppingCts.Token);
    }

    public override MagicBase Copy()
    {
        return new TimeStopMagic(Wizard, _args);
    }

    protected override void OnFinishInternal()
    {
        Dispose();
    }

    private void GiveBoostsToWizard()
    {
        var wizardInstance = Wizard.Instance!;

        wizardInstance.SetSpeedBoostTime((float)(_timeStopService.TimeStopDelay + TimeStopDuration).TotalMilliseconds);

        _playerModifiers = wizardInstance.GetModifiers();
        _playerModifiersService.AddModifiers(Wizard, new SFDPlayerModifiers { CanBurn = 0 });
    }

    private void RemoveBoostsFromWizard()
    {
        var wizardInstance = Wizard.Instance;

        wizardInstance?.SetSpeedBoostTime(0);

        _playerModifiersService.RevertModifiers(Wizard, new SFDPlayerModifiers { CanBurn = 0 }, _playerModifiers!);
    }

    private void PlayTimeStopEffect()
    {
        _timeStopEffect = new TimeStopEffect(_game, _effectsPlayer, ExtendedEvents, TimeStopDuration,
            _timeStopService.TimeStopDelay);
        _timeStopEffect.Play(_timeStoppingCts!.Token);
    }

    private void OnTimeStop()
    {
        _game.RunCommand("/settime 1");

        var wizardInstance = Wizard.Instance!;
        if (wizardInstance.IsCaughtByPlayerInGrab || wizardInstance.IsCaughtByPlayerInDive)
        {
            ForceResumeTime();
            return;
        }

        _resumeTimeCts = new CancellationTokenSource();
        Awaiter.Start(OnResumeTime, TimeStopDuration, _resumeTimeCts.Token);
        ScheduleTimeLeftDialogueCreation();

        _timeStopEffect!.PlayTickEffect(_resumeTimeCts.Token);
    }

    private void OnResumeTime()
    {
        ExtendedEvents.Clear();
        _timeStopEffect!.PlayTimeResumeEffect();

        RemoveBoostsFromWizard();

        Awaiter.Start(ExternalFinish, _timeStopService.TimeStopDelay);
    }

    private void OnDead(Event @event)
    {
        RemoveTimeStopEffect();
        ForceResumeTime();
    }

    private void RemoveTimeStopEffect()
    {
        _timeStoppingCts?.Cancel();
        _game.RunCommand("/settime 1");
    }

    private void ForceResumeTime()
    {
        _resumeTimeCts?.Cancel();
        _timeStopService.ForceResumeTime();
        OnResumeTime();
    }

    private void ScheduleTimeLeftDialogueCreation()
    {
        var secondsToTimeResuming = 3;
        Awaiter.Start(delegate
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.RealTime,
                delegate
                {
                    _notificationService.CreateDialogueNotification($"TIME RESUMING IN {secondsToTimeResuming--}",
                        Color.Yellow, TimeSpan.FromSeconds(1), Wizard.Instance!, realTime: true);
                }, null, secondsToTimeResuming, ExtendedEvents, _resumeTimeCts!.Token);
            timer.Start();
        }, TimeStopDuration - TimeSpan.FromSeconds(secondsToTimeResuming + 1), _resumeTimeCts!.Token);
    }

    private void Dispose()
    {
        _timeStopEffect?.Dispose();
        _timeStoppingCts?.Dispose();
        _resumeTimeCts?.Dispose();

        _notificationService.ClosePopupNotification();
        ExtendedEvents.Clear();
    }

    private class TimeStopEffect
    {
        private readonly IGame _game;
        private readonly IEffectsPlayer _effectsPlayer;
        private readonly DynamicPeriodicTimer _timeStopTimer;
        private readonly PeriodicTimer _periodicTimer;
        private readonly DynamicPeriodicTimer _timeResumeTimer;

        private float _timeModifierAtStart;
        private float _endTimeModifier;
        private CancellationTokenRegistration _timeStopTimerCtr;
        private CancellationTokenRegistration _tickTimerCtr;

        public TimeStopEffect(IGame game, IEffectsPlayer effectsPlayer, IExtendedEvents extendedEvents,
            TimeSpan timeStopDuration, TimeSpan timeStopDelay)
        {
            _game = game;
            _effectsPlayer = effectsPlayer;
            _timeStopTimer = new DynamicPeriodicTimer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(400),
                timeStopDelay, OnClockEffect, default, TimeBehavior.RealTime, extendedEvents);
            _periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1), TimeBehavior.RealTime, OnTickEffect, default,
                (int)timeStopDuration.Divide(TimeSpan.FromSeconds(1)), extendedEvents);
            _timeResumeTimer = new DynamicPeriodicTimer(TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(100),
                timeStopDelay, OnClockEffect, default, TimeBehavior.RealTime, extendedEvents);
        }

        public void Play(CancellationToken cancellationToken)
        {
            _timeModifierAtStart = _game.SlowmotionModifier;
            _timeStopTimerCtr = cancellationToken.Register(_timeStopTimer.Stop);
            _endTimeModifier = 0.1f;
            _timeStopTimer.Start();
        }

        public void PlayTickEffect(CancellationToken cancellationToken)
        {
            _tickTimerCtr = cancellationToken.Register(_periodicTimer.Stop);
            _periodicTimer.Start();
        }

        public void PlayTimeResumeEffect()
        {
            _timeModifierAtStart = 0.1f;
            _endTimeModifier = 1f;
            _timeResumeTimer.Start();
        }

        private void OnTickEffect()
        {
            _effectsPlayer.PlaySoundEffect("ButtonPush1", Vector2.Zero);
        }

        private void OnClockEffect(DynamicPeriodicTimerTickArgs args)
        {
            var step = (_timeModifierAtStart - _endTimeModifier) / args.StepsCount;
            var modifier = Math.Round(_timeModifierAtStart - step * args.StepIndex, 1);

            _game.RunCommand($"/settime {modifier}");
            _effectsPlayer.PlaySoundEffect("ButtonPush1", Vector2.Zero);
        }

        public void Dispose()
        {
            _timeStopTimerCtr.Dispose();
            _tickTimerCtr.Dispose();
        }
    }
}