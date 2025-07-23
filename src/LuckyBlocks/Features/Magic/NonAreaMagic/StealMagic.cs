using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Features.Time.TimeStop;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class StealMagic : NonAreaMagicBase
{
    public event Action? EnterStealMode;
    public event Action? Steal;
    public event Action<string>? StealFail;

    public override string Name => "Steal magic";

    private readonly IIdentityService _identityService;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ITimeStopService _timeStopService;
    private readonly MagicConstructorArgs _args;
    private readonly IExtendedEvents _extendedEvents;
    private readonly PeriodicTimer _timer;

    private bool _isInSelection;
    private IPlayer? _selectedPlayer;
    private VirtualKeyEvent _previousKeyState;

    public StealMagic(Player wizard, MagicConstructorArgs args) : base(wizard, args)
    {
        _identityService = args.IdentityService;
        _weaponPowerupsService = args.WeaponPowerupsService;
        _effectsPlayer = args.EffectsPlayer;
        _timeStopService = args.TimeStopService;
        _args = args;
        var thisScope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100), TimeBehavior.TimeModifier, OnTimerCallback, null,
            int.MaxValue, _extendedEvents);
    }

    public override void Cast()
    {
        var magicWasRestoredAndDidntCast = IsCloned && !_isInSelection;
        if (magicWasRestoredAndDidntCast)
            return;

        var magicWasntRestoredAndInSelection = !IsCloned && _isInSelection;
        if (magicWasntRestoredAndInSelection)
        {
            StealFromSelectedPlayer();
            return;
        }

        _extendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Default);
        _selectedPlayer = PickAlivePlayer();
        _timer.Start();

        var magicWasCastedAndRestored = IsCloned && _isInSelection;
        if (magicWasCastedAndRestored)
            return;

        _isInSelection = true;
        EnterStealMode?.Invoke();
    }

    public override MagicBase Copy()
    {
        return new StealMagic(Wizard, _args) { _isInSelection = _isInSelection };
    }

    protected override void OnFinishInternal()
    {
        Dispose();
    }

    private void StealFromSelectedPlayer()
    {
        if (_selectedPlayer?.IsValidUser() == true)
        {
            var player = _identityService.GetPlayerByInstance(_selectedPlayer);
            var playerWeaponsData = _weaponPowerupsService.CreateWeaponsDataCopy(player);
            _weaponPowerupsService.RestoreWeaponsDataFromCopy(Wizard, playerWeaponsData);
            _selectedPlayer.RemoveAllWeapons();

            Steal?.Invoke();
            ExternalFinish();
            return;
        }

        StealFail?.Invoke("NO SELECTED PLAYER");
        ExternalFinish();
    }

    private void OnKeyInput(Event<IPlayer, VirtualKeyInfo[]> @event)
    {
        var (playerInstance, args, _) = @event;
        if (playerInstance != Wizard.Instance)
            return;

        if (_timeStopService.IsTimeStopped)
            return;

        foreach (var keyInput in args)
        {
            if (keyInput.Key != VirtualKey.WALKING)
                continue;

            if (keyInput.Event == VirtualKeyEvent.Released && _previousKeyState == VirtualKeyEvent.Pressed)
            {
                _selectedPlayer = PickNextPlayer();
                _previousKeyState = VirtualKeyEvent.Released;
                return;
            }

            _previousKeyState = VirtualKeyEvent.Pressed;
        }
    }

    private IPlayer? PickNextPlayer()
    {
        var otherPlayers = GetPlayers();
        return otherPlayers.FirstOrDefault(x => x.GetWorldPosition().X > _selectedPlayer?.GetWorldPosition().X) ??
               PickAlivePlayer();
    }

    private IPlayer? PickAlivePlayer()
    {
        var otherPlayers = GetPlayers();

        if (otherPlayers.Count == 0)
        {
            return null;
        }

        return otherPlayers.First();
    }

    private List<IPlayer> GetPlayers() => _identityService.GetAlivePlayers(false)
        .Where(x => x.Instance != Wizard.Instance && x.WeaponsData.HasAnyWeapon() &&
                    !x.GetImmunityFlags().HasFlag<ImmunityFlag>(ImmunityFlag.ImmunityToSteal))
        .Select(x => x.Instance!)
        .OrderBy(x => x.GetWorldPosition().X)
        .ToList();

    private void OnTimerCallback()
    {
        if (_selectedPlayer is null)
        {
            throw new NullReferenceException(nameof(_selectedPlayer));
        }

        if (!_selectedPlayer.IsValidUser() || _selectedPlayer.IsDead || !PlayerHasAnyWeapon(_selectedPlayer))
        {
            _selectedPlayer = PickAlivePlayer();

            if (_selectedPlayer is null)
            {
                StealFail?.Invoke("NO ONE HAS WEAPON");
                ExternalFinish();
                return;
            }
        }

        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, _selectedPlayer.GetWorldPosition() + new Vector2(0, 20),
            _selectedPlayer.Name, Color.Red, 1f, 3f, true);
    }

    private bool PlayerHasAnyWeapon(IPlayer playerInstance)
    {
        var getPlayerResult = _identityService.GetPlayerById(playerInstance.UniqueId);
        return getPlayerResult.TryPickT0(out var player, out _) && player.HasAnyWeapon();
    }

    private void Dispose()
    {
        _extendedEvents.Clear();
        _timer.Stop();
    }
}