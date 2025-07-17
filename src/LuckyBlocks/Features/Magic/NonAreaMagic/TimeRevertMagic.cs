using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.Time.TimeRevert;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class TimeRevertMagic : NonAreaMagicBase
{
    public override string Name => "Time revert magic";

    private static TimeSpan TimeForChooseSnapshot => TimeSpan.FromSeconds(10);

    private readonly ITimeStopService _timeStopService;
    private readonly ITimeRevertService _timeRevertService;
    private readonly ITimeProvider _timeProvider;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly INotificationService _notificationService;
    private readonly IGame _game;
    private readonly MagicConstructorArgs _args;
    private readonly IExtendedEvents _extendedEvents;
    private readonly PeriodicTimer _timer;

    [field: MaybeNull]
    private List<RealitySnapshot> Snapshots =>
        field ??= _timeRevertService.Snapshots
            .OrderByDescending(x => x.ElapsedGameTime)
            .ToList();

    private int _selectedSnapshotIndex;
    private int _selectedSnapshotId;
    private VirtualKeyEvent _previousAltKeyState;
    private VirtualKeyEvent _previousAttackKeyState;
    private CancellationTokenSource? _cts;
    private float _elapsedRealTimeWhenStarted;
    private IEventSubscription? _keyInputSubscription;

    public TimeRevertMagic(Player wizard, MagicConstructorArgs args) : base(wizard, args)
    {
        _timeStopService = args.TimeStopService;
        _timeRevertService = args.TimeRevertService;
        _timeProvider = args.TimeProvider;
        _effectsPlayer = args.EffectsPlayer;
        _notificationService = args.NotificationService;
        _game = args.Game;
        _args = args;
        var thisScope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100),
            TimeBehavior.TimeModifier | TimeBehavior.IgnoreTimeStop, OnTimerCallback, null,
            int.MaxValue, _extendedEvents);
    }

    public override void Cast()
    {
        _timeStopService.StopTime(TimeSpan.MaxValue, Wizard.Instance!);
        _cts = new CancellationTokenSource();
        _elapsedRealTimeWhenStarted = _game.TotalElapsedRealTime;

        Awaiter.Start(OnTimeStopped, _timeStopService.TimeStopDelay);
        Awaiter.Start(OnTimeForChoosePassed, TimeForChooseSnapshot, _cts.Token);
    }

    protected override MagicBase CloneInternal()
    {
        return new TimeRevertMagic(Wizard, _args);
    }

    protected override void OnFinishInternal()
    {
        _timer.Stop();
        _extendedEvents.Clear();
        _cts?.Cancel();
        _cts?.Dispose();
    }

    private void OnKeyInput(Event<IPlayer, VirtualKeyInfo[]> @event)
    {
        var (playerInstance, args, _) = @event;
        if (playerInstance != Wizard.Instance)
            return;

        foreach (var keyInput in args)
        {
            if (keyInput.Key is not (VirtualKey.WALKING or VirtualKey.ATTACK))
                continue;

            if (keyInput.Key == VirtualKey.WALKING)
            {
                OnAltKeyPressed(keyInput);
            }
            else
            {
                OnAttackKeyPressed(keyInput);
            }

            return;
        }
    }

    private void OnAltKeyPressed(VirtualKeyInfo keyInput)
    {
        if (keyInput.Event == VirtualKeyEvent.Released && _previousAltKeyState == VirtualKeyEvent.Pressed)
        {
            _selectedSnapshotIndex = _selectedSnapshotIndex + 1 == _timeRevertService.SnapshotsCount
                ? 0
                : _selectedSnapshotIndex + 1;
            _selectedSnapshotId = Snapshots[_selectedSnapshotIndex].Id;
            _previousAltKeyState = VirtualKeyEvent.Released;
            return;
        }

        _previousAltKeyState = VirtualKeyEvent.Pressed;
    }

    private void OnAttackKeyPressed(VirtualKeyInfo keyInput)
    {
        if (keyInput.Event == VirtualKeyEvent.Released && _previousAttackKeyState == VirtualKeyEvent.Pressed)
        {
            RevertTime();
            _previousAttackKeyState = VirtualKeyEvent.Released;
            return;
        }

        _previousAttackKeyState = VirtualKeyEvent.Pressed;
    }

    private void OnTimerCallback()
    {
        const int snapshotsInRow = 3;
        var snapshotIndex = 0;
        var wizardInstance = Wizard.Instance!;
        var position = wizardInstance.GetWorldPosition();
        var baseMin = position + new Vector2(-100, -150);
        var baseMax = position + new Vector2(125, -15);
        var additionalOffset = Vector2.Zero;
        var cameraArea = _game.GetCameraArea();

        if (baseMin.X < cameraArea.Left)
        {
            additionalOffset.X += cameraArea.Left - baseMin.X + 10;
        }
        else if (baseMax.X > cameraArea.Right)
        {
            additionalOffset.X -= baseMax.X - cameraArea.Right - 10;
        }

        if (baseMax.Y > cameraArea.Top)
        {
            additionalOffset.Y -= cameraArea.Top - baseMax.Y - 10;
        }
        else if (baseMin.Y < cameraArea.Bottom)
        {
            additionalOffset.Y += cameraArea.Bottom - baseMin.Y + 10;
        }

        foreach (var snapshot in Snapshots)
        {
            var textPosition = position + additionalOffset + new Vector2(-50 + 50 * (snapshotIndex % snapshotsInRow),
                -30 * (snapshotIndex / snapshotsInRow) - 15);
            var textColor = snapshotIndex == _selectedSnapshotIndex ? ExtendedColors.KillerQueen : Color.Grey;
            var timeBehind = TimeSpan.FromMilliseconds(_timeProvider.ElapsedGameTime - snapshot.ElapsedGameTime);

            _effectsPlayer.PlayEffect(EffectName.CustomFloatText, textPosition,
                $"{Math.Round(timeBehind.TotalSeconds)}s",
                textColor, 1f, 3f, true);

            snapshotIndex++;
        }

        var rows = (int)Math.Ceiling(_timeRevertService.SnapshotsCount / (double)snapshotsInRow);
        const string tipBaseText = "HOW MUCH TIME YOU WANNA REVERT?";
        const string tip3SecondsLeft = "3 SECONDS LEFT";
        const string tip2SecondsLeft = "2 SECONDS LEFT";
        const string tip1SecondLeft = "1 SECOND LEFT";

        var tipOffset = position + additionalOffset + new Vector2(0, -45 * rows);
        var timeLeft = TimeForChooseSnapshot.TotalMilliseconds -
                       (_game.TotalElapsedGameTime - _elapsedRealTimeWhenStarted);
        var tipText = timeLeft switch
        {
            > 3000f => tipBaseText,
            > 2000f and <= 3000f => tip3SecondsLeft,
            > 1000f and <= 2000f => tip2SecondsLeft,
            <= 1000f => tip1SecondLeft
        };

        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, tipOffset, tipText, Color.Grey, 1f, 3f, true);
    }

    private void OnTimeStopped()
    {
        _selectedSnapshotIndex = 0;
        _selectedSnapshotId = Snapshots[0].Id;
        _keyInputSubscription = _extendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Default);
        _timer.Start();
        _notificationService.CreateChatNotification("[ALT] for change interval", ExtendedColors.KillerQueen,
            Wizard.UserIdentifier);
        _notificationService.CreateChatNotification("[A] for confirm your choice", ExtendedColors.KillerQueen,
            Wizard.UserIdentifier);
        _notificationService.CreateChatNotification(
            $"You have {TimeForChooseSnapshot.Seconds}s to choose an interval! Otherwise, a random one will be selected",
            ExtendedColors.KillerQueen, Wizard.UserIdentifier);
    }

    private void OnTimeForChoosePassed()
    {
        _selectedSnapshotId = Snapshots.GetRandomElement().Id;
        RevertTime();
    }

    private void RevertTime()
    {
        _keyInputSubscription?.Dispose();
        _game.RunCommand("/slomo 1");
        _timeStopService.ForceResumeTime();
        Awaiter.Start(OnTimeResumed, _timeStopService.TimeStopDelay);
    }

    private void OnTimeResumed()
    {
        _game.RunCommand("/slomo 0");
        _timeRevertService.RestoreFromSnapshot(_selectedSnapshotId);
        ExternalFinish();
    }
}