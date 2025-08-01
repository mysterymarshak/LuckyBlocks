using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Keyboard.States;
using LuckyBlocks.Features.Time;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Keyboard;

internal class Keyboard
{
    private readonly Dictionary<int, KeyboardEventSubscription> _subscriptions = new();
    private readonly Dictionary<VirtualKey, WatchingVirtualKey> _watchingKeys = new();
    private readonly Player _player;
    private readonly ITimeProvider _timeProvider;
    private readonly IExtendedEvents _extendedEvents;

    private int _lastSubscriptionId;
    private IEventSubscription? _keyInputSubscription;

    public Keyboard(Player player, ITimeProvider timeProvider, IExtendedEvents extendedEvents)
    {
        _player = player;
        _timeProvider = timeProvider;
        _extendedEvents = extendedEvents;
    }

    public IKeyboardEventSubscription HookPress(IEnumerable<VirtualKey> keys, Action callback,
        TimeSpan resetCooldown = default)
    {
        _keyInputSubscription ??= _extendedEvents.HookOnKeyInput(OnKeyInput, EventHookMode.Default);

        foreach (var key in keys)
        {
            RegisterKey(key);
        }

        _lastSubscriptionId++;
        var watchingKeys = keys.Select(x => _watchingKeys[x]).ToList();
        var subscription = new KeyboardEventSubscription(_lastSubscriptionId, callback,
            new AllKeysPressedStrategy(watchingKeys, resetCooldown, _timeProvider));
        _subscriptions.Add(_lastSubscriptionId, subscription);

        return subscription;
    }

    public void Unhook(IKeyboardEventSubscription subscription)
    {
        _subscriptions.Remove(subscription.Id);
        var keysWithoutSubscriptions = _watchingKeys
            .Where(x => _subscriptions
                .All(y => !y.Value.StateHandler.WatchingKeys
                    .Select(z => z.Key)
                    .Contains(x.Key)))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in keysWithoutSubscriptions)
        {
            _watchingKeys.Remove(key);
        }

        if (_subscriptions.Count == 0)
        {
            Dispose();
        }
    }

    public void Dispose()
    {
        _subscriptions.Clear();
        _watchingKeys.Clear();
        _keyInputSubscription?.Dispose();
    }

    private void RegisterKey(VirtualKey key)
    {
        if (_watchingKeys.TryGetValue(key, out var watchingKey))
            return;

        watchingKey = new WatchingVirtualKey(key);
        _watchingKeys.Add(key, watchingKey);
    }

    private void OnKeyInput(Event<IPlayer, VirtualKeyInfo[]> @event)
    {
        var playerInstance = @event.Arg1;
        
        if (playerInstance != _player.Instance)
            return;

        if (playerInstance.IsDead)
            return;

        var keyInputs = @event.Arg2;
        foreach (var keyInput in keyInputs)
        {
            if (!_watchingKeys.TryGetValue(keyInput.Key, out var watchingKey))
                continue;

            watchingKey.UpdateFromEvent(keyInput, _timeProvider.ElapsedRealTime);
        }

        foreach (var subscription in _subscriptions.Values)
        {
            subscription.InvalidateState();
        }
    }

    private class KeyboardEventSubscription : IKeyboardEventSubscription
    {
        public int Id { get; }
        public IStateHandleStrategy StateHandler { get; }

        private readonly Action _callback;

        public KeyboardEventSubscription(int id, Action callback, IStateHandleStrategy stateHandler)
        {
            Id = id;
            _callback = callback;
            StateHandler = stateHandler;
        }

        public void ResetCooldown()
        {
            StateHandler.ResetCooldown();
        }

        public void InvalidateState()
        {
            var shouldTrigger = StateHandler.Handle();
            if (shouldTrigger)
            {
                Trigger();
            }
        }

        private void Trigger()
        {
            _callback.Invoke();
        }
    }
}