using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IDrawnWeaponsWatcher
{
    void StartDrawnTracking(Player player, WeaponItemType weaponItemType);
    void Initialize();
}

[Inject]
internal class DrawnWeaponsWatcher : IDrawnWeaponsWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly Dictionary<int, AwaitingDrawnChangeData> _drawnWeaponTimers = null!;
    private readonly Dictionary<Player, Weapon> _previouslyDrawnWeapons = new();
    private readonly IIdentityService _identityService;
    private readonly IExtendedEvents _extendedEvents;

    public DrawnWeaponsWatcher(IIdentityService identityService, ILifetimeScope lifetimeScope)
    {
        _identityService = identityService;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    private void OnUpdate(Event<float> @event)
    {
        var alivePlayers = _identityService.GetAlivePlayers();

        foreach (var player in alivePlayers)
        {
            var playerInstance = player.Instance!;
            var weaponsData = player.WeaponsData;
            var currentDrawn = weaponsData.CurrentWeaponDrawn;
            var realCurrentDrawn = playerInstance.CurrentWeaponDrawn;
            
            if (!_previouslyDrawnWeapons.TryGetValue(player, out var previouslyDrawnWeapon))
            {
                previouslyDrawnWeapon = currentDrawn;
                _previouslyDrawnWeapons.Add(player, previouslyDrawnWeapon);
            }

            if (currentDrawn.WeaponItemType != realCurrentDrawn)
            {
                if (realCurrentDrawn == WeaponItemType.NONE)
                {
                    currentDrawn.RaiseEvent(WeaponEvent.Hidden);
                    weaponsData.UpdateDrawn();
                    
                    Logger.Debug("Hidden {WeaponItem} for player {Player}", currentDrawn.WeaponItem, player.Name);
                }
                else
                {
                    if (currentDrawn.WeaponItemType != WeaponItemType.NONE)
                    {
                        currentDrawn.RaiseEvent(WeaponEvent.Hidden);
                        Logger.Debug("Hidden {WeaponItem} for player {Player}", currentDrawn.WeaponItem, player.Name);
                    }
                    
                    weaponsData.UpdateDrawn();

                    var newDrawn = weaponsData.CurrentWeaponDrawn;
                    newDrawn.RaiseEvent(WeaponEvent.Drawn);
                    
                    Logger.Debug("Drawn {WeaponItem} for player {Player}", newDrawn.WeaponItem, player.Name);
                }
            }
        }
    }

    public void StartDrawnTracking(Player player, WeaponItemType awaitingWeaponItemType)
    {
        var playerInstance = player.Instance!;
        var weaponsData = player.WeaponsData;
        var currentDrawn = weaponsData.CurrentWeaponDrawn;
        var playerId = playerInstance.UniqueId;

        if (_drawnWeaponTimers.TryGetValue(playerId, out var data))
        {
            var existingTimer = data.Timer;
            existingTimer.Stop();
            
            _drawnWeaponTimers.Remove(playerId);
        }
        
        if (currentDrawn.WeaponItemType == awaitingWeaponItemType)
            return;

        var args = new AwaitingDrawnChangeTimerArgs(player, awaitingWeaponItemType, currentDrawn.WeaponItemType, () => _drawnWeaponTimers.Remove(playerId));
        var timer = new PeriodicTimer<AwaitingDrawnChangeTimerArgs>(TimeSpan.Zero, TimeBehavior.TimeModifier, Callback,
            FinishCondition, FinishCallback, args, _extendedEvents);
        timer.Start();

        var awaitingData =
            new AwaitingDrawnChangeData(timer);
        _drawnWeaponTimers.Add(playerId, awaitingData);

        static void Callback(AwaitingDrawnChangeTimerArgs args)
        {
            var player = args.Player;
            var playerInstance = player.Instance!;
            var weaponsData = player.WeaponsData;

            weaponsData.UpdateDrawn();

            Logger.Debug(
                "Updated drawn weapon for player: {Player}, new drawn: {WeaponItem}, drawn: {DrawnItemType}, wainting for {WeaponItemType}",
                player.Name, weaponsData.CurrentWeaponDrawn.WeaponItem, playerInstance.CurrentWeaponDrawn,
                args.WeaponItemType);
        }

        static bool FinishCondition(AwaitingDrawnChangeTimerArgs args)
        {
            var playerInstance = args.Player.Instance;
            var weaponsData = args.Player.WeaponsData;

            return playerInstance?.IsValid() != true || playerInstance.IsDead ||
                   playerInstance.CurrentWeaponDrawn == args.WeaponItemType &&
                   weaponsData.CurrentWeaponDrawn.WeaponItemType == args.WeaponItemType;
        }

        static void FinishCallback(AwaitingDrawnChangeTimerArgs args)
        {
            var player = args.Player;
            var weaponsData = player.WeaponsData;

            if (args.PreviousWeaponItemType != WeaponItemType.NONE)
            {
                var weapon = weaponsData.GetWeaponByType(args.PreviousWeaponItemType);
                if (!weapon.IsInvalid)
                {
                    weapon.RaiseEvent(WeaponEvent.Hidden);
                    Logger.Debug("Hidden {WeaponItem} for player {Player}", weapon.WeaponItem, player.Name);  
                }
            }
            
            var removeTimerDelegate = args.RemoveTimerDelegate;
            removeTimerDelegate.Invoke();

            if (args.WeaponItemType == WeaponItemType.NONE)
                return;

            var playerInstance = player.Instance;
            if (playerInstance?.IsValid() != true)
                return;

            var currentDrawn = weaponsData.CurrentWeaponDrawn;
            currentDrawn.RaiseEvent(WeaponEvent.Drawn);
            Logger.Debug("Drawn {WeaponItem} for player {Player}", currentDrawn.WeaponItem, player.Name);
        }
    }

    private record AwaitingDrawnChangeTimerArgs(
        Player Player,
        WeaponItemType WeaponItemType,
        WeaponItemType PreviousWeaponItemType,
        Action RemoveTimerDelegate);

    private record AwaitingDrawnChangeData(TimerBase Timer);
}