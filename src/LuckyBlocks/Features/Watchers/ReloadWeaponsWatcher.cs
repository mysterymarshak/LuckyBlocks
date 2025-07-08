using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IReloadWeaponsWatcher
{
    void Initialize();
    void StartReloadTracking(Player player, WeaponItemType weaponItemType);
}

[Inject]
internal class ReloadWeaponsWatcher : IReloadWeaponsWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly Dictionary<int, List<ReloadAwaitingData>> _reloadsData = new();
    private readonly IExtendedEvents _extendedEvents;

    public ReloadWeaponsWatcher(ILifetimeScope lifetimeScope)
    {
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Initialize()
    {
        _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    private void OnUpdate(Event<float> @event)
    {
        foreach (var entry in _reloadsData)
        {
            var data = entry.Value;

            for (var i = data.Count - 1; i >= 0; i--)
            {
                var reloadData = data[i];
                var args = reloadData.Args;

                if (FinishCondition(args))
                {
                    var player = args.Player;
                    var weaponsData = player.WeaponsData;
                    var firearm = (Firearm)weaponsData.GetWeaponByType(reloadData.WeaponItemType, false);

                    firearm.RaiseEvent(WeaponEvent.Reloaded);
                    data.RemoveAt(i);

                    continue;
                }

                Callback(args);
            }
        }
    }

    public void StartReloadTracking(Player player, WeaponItemType weaponItemType)
    {
        var playerInstance = player.Instance!;
        var weaponsData = player.WeaponsData;
        var savedWeapon = weaponsData.CurrentWeaponDrawn with { } as Firearm;

        if (_reloadsData.TryGetValue(playerInstance.UniqueId, out var awaitingReloads))
        {
            if (awaitingReloads.Any(x => x.WeaponItemType == weaponItemType))
                return;
        }
        else
        {
            awaitingReloads = [];
            _reloadsData.Add(playerInstance.UniqueId, awaitingReloads);
        }

        Logger.Debug("Start watching reloading for player: {Player}, waiting for changing {WeaponItemType}",
            player.Name, weaponItemType);

        var args = new AwaitingWeaponChangeTimerArgs(player, weaponItemType, savedWeapon!);
        awaitingReloads.Add(new ReloadAwaitingData(args, weaponItemType));
    }

    private static void Callback(AwaitingWeaponChangeTimerArgs args)
    {
        var player = args.Player;
        player.UpdateWeaponData(args.WeaponItemType);

        Logger.Debug("Updated weapon for player: {Player}, waiting for changing {WeaponItemType}", player.Name,
            args.WeaponItemType);
    }

    private static bool FinishCondition(AwaitingWeaponChangeTimerArgs args)
    {
        var player = args.Player;
        var playerInstance = player.Instance;
        var weaponsData = player.WeaponsData;

        return playerInstance?.IsValid() != true || playerInstance.IsDead ||
               weaponsData.CurrentWeaponDrawn.WeaponItemType != args.WeaponItemType ||
               (weaponsData.GetWeaponByType(args.WeaponItemType, false) as Firearm)!.CurrentSpareMags !=
               args.SavedWeapon.CurrentSpareMags;
    }

    private record AwaitingWeaponChangeTimerArgs(
        Player Player,
        WeaponItemType WeaponItemType,
        Firearm SavedWeapon);

    private record ReloadAwaitingData(AwaitingWeaponChangeTimerArgs Args, WeaponItemType WeaponItemType);
}