using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IDrawnWeaponsWatcher
{
    void Initialize();
}

[Inject]
internal class DrawnWeaponsWatcher : IDrawnWeaponsWatcher
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

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

            if (currentDrawn.WeaponItemType != realCurrentDrawn ||
                (playerInstance.CurrentMeleeMakeshiftWeapon.WeaponItem != WeaponItem.NONE &&
                 weaponsData.CurrentWeaponDrawn is not MeleeTemp))
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
}