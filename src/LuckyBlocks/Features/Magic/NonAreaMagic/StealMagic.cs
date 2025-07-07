using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.NonAreaMagic;

internal class StealMagic : NonAreaMagicBase
{
    public override string Name => "Steal magic";

    private readonly IIdentityService _identityService;
    private readonly IWeaponPowerupsService _weaponPowerupsService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly ITimeStopService _timeStopService;
    private readonly IExtendedEvents _extendedEvents;
    private readonly PeriodicTimer _timer;

    private bool _isInSelection;
    private IPlayer? _selectedPlayer;
    private VirtualKeyEvent _previousKeyState;

    public StealMagic(Player wizard, BuffConstructorArgs args) : base(wizard, args)
    {
        _identityService = args.IdentityService;
        _weaponPowerupsService = args.WeaponPowerupsService;
        _effectsPlayer = args.EffectsPlayer;
        _timeStopService = args.TimeStopService;
        var thisScope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
        _timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100), TimeBehavior.TimeModifier, OnTimerCallback, null,
            int.MaxValue, _extendedEvents);
    }

    public override void Cast()
    {
        if (_isInSelection)
        {
            StealFromSelectedPlayer();
            return;
        }

        _extendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Default);
        _selectedPlayer = PickAlivePlayer();
        _timer.Start();
        _isInSelection = true;
    }

    protected override void OnFinished()
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
        }

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

    private List<IPlayer> GetPlayers()
    {
        var otherPlayers = _identityService.GetAlivePlayers(false)
            .Where(x => x.Instance != Wizard.Instance && x.WeaponsData.HasAnyWeapon())
            .Select(x => x.Instance!)
            .OrderBy(x => x.GetWorldPosition().X)
            .ToList();
        return otherPlayers;
    }

    private void OnTimerCallback()
    {
        if (_selectedPlayer is null)
        {
            throw new NullReferenceException(nameof(_selectedPlayer));
        }

        var getPlayerResult = _identityService.GetPlayerById(_selectedPlayer.UniqueId);
        if (!_selectedPlayer.IsValidUser() || _selectedPlayer.IsDead || !getPlayerResult.IsT0 ||
            !getPlayerResult.AsT0.HasAnyWeapon())
        {
            _selectedPlayer = PickAlivePlayer();
            
            if (_selectedPlayer is null)
            {
                Dispose();
                return;
            }
        }

        _effectsPlayer.PlayEffect(EffectName.CustomFloatText, _selectedPlayer.GetWorldPosition() + new Vector2(0, 20),
            _selectedPlayer.Name, Color.Red, 1f, 3f, true);
    }

    private void Dispose()
    {
        _extendedEvents.Clear();
        _timer.Stop();
    }
}