using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils.Watchers;

interface IThrowableWeaponsWatcher
{
    void OnThrowablePickedUp(Player player);
    void OnThrowableRemoved(Player player);
    void OnGrenadeThrown(Grenade grenade);
}

internal class ThrowableWeaponsWatcher : IThrowableWeaponsWatcher
{
    private readonly ILogger _logger;
    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly List<Player> _playersWithThrowable = [];
    private readonly List<Grenade> _thrownGrenades = [];

    private IEventSubscription? _updateEventSubscription;

    public ThrowableWeaponsWatcher(ILogger logger, IGame game, ILifetimeScope lifetimeScope)
    {
        _logger = logger;
        _game = game;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void OnThrowablePickedUp(Player player)
    {
        if (!_playersWithThrowable.Contains(player))
        {
            _playersWithThrowable.Add(player);
        }

        _updateEventSubscription ??= _extendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    public void OnThrowableRemoved(Player player)
    {
        _playersWithThrowable.Remove(player);
    }

    public void OnGrenadeThrown(Grenade grenade)
    {
        _thrownGrenades.Add(grenade);
    }

    private void OnUpdate(Event<float> @event)
    {
        try
        {
            UpdatePlayersThrowable();
            UpdateThrownGrenades();

            if (_thrownGrenades.Count == 0 && _playersWithThrowable.Count == 0)
            {
                _updateEventSubscription!.Dispose();
                _updateEventSubscription = null;

                _logger.Debug("Dispose throwables subscription");
            }
        }
        catch (Exception exception)
        {
            _logger.Error("Error while processing throwables update event: {Message}", exception);
        }
    }

    private void UpdatePlayersThrowable()
    {
        var playersWithHoldingGrenades = _playersWithThrowable.Where(x =>
            x is { Instance.IsHoldingActiveThrowable: true, WeaponsData.CurrentWeaponDrawn: Throwable } or
                { Instance.IsHoldingActiveThrowable: false, WeaponsData.ThrowableItem.IsActive: true });

        foreach (var player in playersWithHoldingGrenades)
        {
            var throwable = player.WeaponsData.ThrowableItem;
            var playerInstance = player.Instance!;
            var oldIsActive = throwable.IsActive;
            var newIsActive = playerInstance.IsHoldingActiveThrowable;

            if (throwable is Grenade grenade)
            {
                grenade.Update(newIsActive, playerInstance.GetActiveThrowableTimer());
            }
            else if (throwable.IsActive != newIsActive)
            {
                player.UpdateWeaponData(WeaponItemType.Thrown);
            }

            if (newIsActive && !oldIsActive)
            {
                throwable.RaiseEvent(WeaponEvent.Activated);
            }

            _logger.Verbose("Throwable update: {WeaponItem}, player: {Player}, isHolding: {IsHolding}",
                throwable.WeaponItem, player.Name, playerInstance.IsHoldingActiveThrowable);
        }
    }

    private void UpdateThrownGrenades()
    {
        for (var i = _thrownGrenades.Count - 1; i >= 0; i--)
        {
            var grenade = _thrownGrenades[i];
            var grenadeObject = (IObjectGrenadeThrown)_game.GetObject(grenade.ObjectId);
            if (!grenadeObject.IsValid() || grenadeObject.ExplosionResultedInDud)
            {
                _thrownGrenades.RemoveAt(i);
                continue;
            }

            grenade.Update(false, grenadeObject.GetExplosionTimer());

            _logger.Verbose("Grenade update: {WeaponItem}, id: {ObjectId}, timer: {TimeLeft}", grenade.WeaponItem,
                grenadeObject.UniqueId, grenade.TimeToExplosion);
        }
    }
}