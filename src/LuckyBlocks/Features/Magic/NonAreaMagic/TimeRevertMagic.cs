using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Autofac;
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

    private const int SnapshotsInRow = 3;

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

    public override MagicBase Copy()
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
            if (keyInput.Key is not (VirtualKey.ATTACK or VirtualKey.AIM_CLIMB_UP or VirtualKey.AIM_CLIMB_DOWN
                or VirtualKey.AIM_RUN_LEFT or VirtualKey.AIM_RUN_RIGHT))
                continue;

            if (keyInput.Key == VirtualKey.ATTACK)
            {
                OnAttackKeyPressed(keyInput);
            }
            else
            {
                OnArrowKeyPressed(keyInput);
            }

            return;
        }
    }

    private void OnArrowKeyPressed(VirtualKeyInfo keyInput)
    {
        if (keyInput.Event != VirtualKeyEvent.Released)
            return;

        var isFirstRow = _selectedSnapshotIndex < SnapshotsInRow;
        var isSingleRow = Snapshots.Count <= SnapshotsInRow;
        var indexInRow = _selectedSnapshotIndex % SnapshotsInRow;
        var rowsCount = (int)Math.Ceiling(Snapshots.Count / (double)SnapshotsInRow);
        var lastIndex = Snapshots.Count - 1;
        var lastRowStartIndex = (rowsCount - 1) * SnapshotsInRow;
        var newIndex = _selectedSnapshotIndex + keyInput.Key switch
        {
            VirtualKey.AIM_CLIMB_DOWN when IsOutOfBounds(_selectedSnapshotIndex + SnapshotsInRow) && isFirstRow => 0,
            VirtualKey.AIM_CLIMB_DOWN when IsOutOfBounds(_selectedSnapshotIndex + SnapshotsInRow) => indexInRow +
                (Snapshots.Count - _selectedSnapshotIndex),
            VirtualKey.AIM_CLIMB_DOWN => SnapshotsInRow,
            VirtualKey.AIM_CLIMB_UP when IsOutOfBounds(_selectedSnapshotIndex - SnapshotsInRow) && isSingleRow => 0,
            VirtualKey.AIM_CLIMB_UP when IsOutOfBounds(_selectedSnapshotIndex - SnapshotsInRow) &&
                                         lastIndex >= lastRowStartIndex + indexInRow => lastRowStartIndex,
            VirtualKey.AIM_CLIMB_UP when IsOutOfBounds(_selectedSnapshotIndex - SnapshotsInRow) =>
                lastRowStartIndex - (SnapshotsInRow - indexInRow) - _selectedSnapshotIndex,
            VirtualKey.AIM_CLIMB_UP => -SnapshotsInRow,
            VirtualKey.AIM_RUN_LEFT => -1,
            VirtualKey.AIM_RUN_RIGHT => 1
        };

        if (newIndex < 0)
        {
            newIndex = Snapshots.Count + newIndex;
        }

        newIndex %= Snapshots.Count;

        _selectedSnapshotIndex = newIndex;
        _selectedSnapshotId = Snapshots[_selectedSnapshotIndex].Id;

        bool IsOutOfBounds(int index) => index < 0 || index >= Snapshots.Count;
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
            var textPosition = position + additionalOffset + new Vector2(-50 + 50 * (snapshotIndex % SnapshotsInRow),
                -30 * (snapshotIndex / SnapshotsInRow) - 15);
            var textColor = snapshotIndex == _selectedSnapshotIndex ? ExtendedColors.KillerQueen : Color.Grey;
            var timeBehind = TimeSpan.FromMilliseconds(_timeProvider.ElapsedGameTime - snapshot.ElapsedGameTime);

            _effectsPlayer.PlayEffect(EffectName.CustomFloatText, textPosition,
                $"{Math.Round(timeBehind.TotalSeconds)}s",
                textColor, 1f, 3f, true);

            snapshotIndex++;
        }

        var rows = (int)Math.Ceiling(Snapshots.Count / (double)SnapshotsInRow);
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
        _notificationService.CreateChatNotification("[ARROWS] for change interval", ExtendedColors.KillerQueen,
            Wizard.UserIdentifier);
        _notificationService.CreateChatNotification("[A] for confirm your choice", ExtendedColors.KillerQueen,
            Wizard.UserIdentifier);
        _notificationService.CreateChatNotification(
            $"You have {TimeForChooseSnapshot.Seconds}s to choose an interval! Otherwise, a random one will be selected",
            ExtendedColors.KillerQueen, Wizard.UserIdentifier);
        // todo: move to wizard
    }

    private void OnTimeForChoosePassed()
    {
        _selectedSnapshotId = Snapshots.GetRandomElement().Id;
        RevertTime();
    }

    private void RevertTime()
    {
        _cts!.Cancel();
        _keyInputSubscription?.Dispose();
        _game.RunCommand("/slomo 1");
        _timeStopService.ForceResumeTime(OnTimeResumed);
    }

    private void OnTimeResumed()
    {
        _game.RunCommand("/slomo 0");
        _timeRevertService.RestoreFromSnapshot(_selectedSnapshotId);
        ExternalFinish();
    }
}