using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Reflection;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IReloadWeaponsWatcher
{
    void StartReloadTracking(Player player, WeaponItemType weaponItemType);
}

[Inject]
internal class ReloadWeaponsWatcher : IReloadWeaponsWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }
    
    private readonly Dictionary<int, List<ReloadAwaitingData>> _reloadWeaponTimers = new();
    private readonly IExtendedEvents _extendedEvents;

    public ReloadWeaponsWatcher(ILifetimeScope lifetimeScope)
    {
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }
    
    public void StartReloadTracking(Player player, WeaponItemType weaponItemType)
    {
        var playerInstance = player.Instance!;
        var weaponsData = player.WeaponsData;
        var savedWeapon = weaponsData.CurrentWeaponDrawn with { } as Firearm;

        if (_reloadWeaponTimers.TryGetValue(playerInstance.UniqueId, out var awaitingReloads))
        {
            if (awaitingReloads.Any(x => x.WeaponItemType == weaponItemType))
                return;
        }
        else
        {
            awaitingReloads = [];
            _reloadWeaponTimers.Add(playerInstance.UniqueId, awaitingReloads);
        }

        Logger.Debug("Start watching reloading for player: {Player}, waiting for changing {WeaponItemType}",
            player.Name, weaponItemType);

        var args = new AwaitingWeaponChangeTimerArgs(player, weaponItemType, savedWeapon!, awaitingReloads);
        var timer = new PeriodicTimer<AwaitingWeaponChangeTimerArgs>(TimeSpan.Zero, TimeBehavior.TimeModifier, Callback,
            FinishCondition, FinishCallback, args, _extendedEvents);
        timer.Start();

        awaitingReloads.Add(new ReloadAwaitingData(timer, weaponItemType));

        static void Callback(AwaitingWeaponChangeTimerArgs args)
        {
            var player = args.Player;
            player.UpdateWeaponData(args.WeaponItemType);

            Logger.Debug("Updated weapon for player: {Player}, waiting for changing {WeaponItemType}", player.Name,
                args.WeaponItemType);
        }

        static bool FinishCondition(AwaitingWeaponChangeTimerArgs args)
        {
            var player = args.Player;
            var playerInstance = player.Instance;
            var weaponsData = player.WeaponsData;

            return playerInstance?.IsValid() != true || playerInstance.IsDead ||
                   weaponsData.CurrentWeaponDrawn.WeaponItemType != args.WeaponItemType ||
                   (weaponsData.GetWeaponByType(args.WeaponItemType) as Firearm)!.CurrentSpareMags !=
                   args.SavedWeapon.CurrentSpareMags;
        }

        static void FinishCallback(AwaitingWeaponChangeTimerArgs args)
        {
            var reloadsData = args.ReloadsData;
            reloadsData.RemoveAll(x => x.WeaponItemType == args.WeaponItemType);
        }
    }
    
    private record AwaitingWeaponChangeTimerArgs(
        Player Player,
        WeaponItemType WeaponItemType,
        Firearm SavedWeapon,
        List<ReloadAwaitingData> ReloadsData);

    private record ReloadAwaitingData(TimerBase Timer, WeaponItemType WeaponItemType);
}