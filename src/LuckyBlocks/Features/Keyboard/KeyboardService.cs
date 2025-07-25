using System.Collections.Generic;
using Autofac;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Time;
using LuckyBlocks.Utils;

namespace LuckyBlocks.Features.Keyboard;

internal interface IKeyboardService
{
    Keyboard ResolveForPlayer(Player player);
    void DisposeForPlayer(Player player);
}

internal class KeyboardService : IKeyboardService
{
    private readonly ITimeProvider _timeProvider;
    private readonly Dictionary<Player, Keyboard> _keyboards = new();
    private readonly IExtendedEvents _extendedEvents;

    public KeyboardService(ITimeProvider timeProvider, ILifetimeScope lifetimeScope)
    {
        _timeProvider = timeProvider;
        var thisScope = lifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public Keyboard ResolveForPlayer(Player player)
    {
        if (!_keyboards.TryGetValue(player, out var keyboard))
        {
            keyboard = new Keyboard(player, _timeProvider, _extendedEvents);
            _keyboards.Add(player, keyboard);
        }

        return keyboard;
    }

    public void DisposeForPlayer(Player player)
    {
        if (!_keyboards.TryGetValue(player, out var keyboard))
            return;

        keyboard.Dispose();
    }
}