using System;
using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Entities;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Watchers;

internal interface IChainsawWeaponsWatcher
{
    void StartTrackingChainsaw(Player player);
    void RemoveChainsawTracking(Player player, Weapon chainsaw);
}

[Inject]
internal class ChainsawWeaponsWatcher : IChainsawWeaponsWatcher
{
    private readonly IIdentityService _identityService;

    [InjectLogger]
    private static ILogger Logger { get; set; }

    private readonly List<Weapon> _drawnChainsaws = [];
    private readonly List<Player> _playersWithChainsaw = [];
    private readonly IExtendedEvents _extendedEvents;

    private IEventSubscription? _updateEventSubscription;

    public ChainsawWeaponsWatcher(IIdentityService identityService, ILifetimeScope lifetimeScope)
    {
        _identityService = identityService;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void StartTrackingChainsaw(Player player)
    {
        if (_playersWithChainsaw.Contains(player))
            return;

        var weaponsData = player.WeaponsData;
        var chainsaw = weaponsData.MeleeWeapon;

        HookEvents(chainsaw);
        _playersWithChainsaw.Add(player);

        if (weaponsData.CurrentWeaponDrawn.WeaponItem == chainsaw.WeaponItem)
        {
            OnChainsawDrawn(chainsaw);
        }
    }

    public void RemoveChainsawTracking(Player player, Weapon chainsaw)
    {
        RemoveDrawnChainsaw(chainsaw);
        UnhookEvents(chainsaw);
        _playersWithChainsaw.Remove(player);
    }

    private void HookEvents(Weapon chainsaw)
    {
        chainsaw.Draw += OnChainsawDrawn;
        chainsaw.Hide += OnChainsawHidden;
    }

    private void UnhookEvents(Weapon chainsaw)
    {
        chainsaw.Draw -= OnChainsawDrawn;
        chainsaw.Hide -= OnChainsawHidden;
    }

    private void OnChainsawDrawn(Weapon weapon)
    {
        var playerInstance = weapon.Owner;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        Logger.Debug("Chainsaw drawn {Durability} ({Player})", ((Melee)weapon).CurrentDurability, playerInstance.Name);

        _drawnChainsaws.Add(weapon);
        _updateEventSubscription ??= _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    private void OnChainsawHidden(Weapon weapon)
    {
        var playerInstance = weapon.Owner;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        Logger.Debug("Chainsaw hidden {Durability} ({Player})", ((Melee)weapon).CurrentDurability, playerInstance.Name);
        RemoveDrawnChainsaw(weapon);
    }

    private void OnUpdate(Event<float> @event)
    {
        try
        {
            foreach (var chainsaw in _drawnChainsaws)
            {
                var playerInstance = chainsaw.Owner;
                if (playerInstance?.IsValid() == true)
                {
                    var player = _identityService.GetPlayerByInstance(playerInstance);
                    player.UpdateWeaponData(WeaponItemType.Melee);

                    Logger.Verbose("Chainsaw updated for player: {Player}, durability: {Durability}", player.Name,
                        ((Melee)chainsaw).CurrentDurability);
                }
            }
        }
        catch (Exception exception)
        {
            Logger.Error("Error while processing chainsaw update event: {Message}", exception);
        }
    }

    private void RemoveDrawnChainsaw(Weapon weapon)
    {
        _drawnChainsaws.Remove(weapon);

        if (_drawnChainsaws.Count == 0)
        {
            _updateEventSubscription?.Dispose();
            _updateEventSubscription = null;
        }
    }
}