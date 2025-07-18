using System;
using LuckyBlocks.Reflection;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Utils;

public interface IExtendedEvents
{
    [GameCallbackType(typeof(Events.ObjectTerminatedCallback))]
    IEventSubscription HookOnDestroyed(IObject obj, Action<Event> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.ObjectTerminatedCallback))]
    IEventSubscription HookOnDestroyed(Action<Event<IObject[]>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.ObjectCreatedCallback))]
    IEventSubscription HookOnCreated(Action<Event<IObject[]>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.ObjectDamageCallback))]
    IEventSubscription HookOnDamage(IObject obj, Action<Event<ObjectDamageArgs>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDamageCallback))]
    IEventSubscription HookOnDamage(IPlayer player, Action<Event<PlayerDamageArgs>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDamageCallback))]
    IEventSubscription HookOnDamage(Action<Event<IPlayer, PlayerDamageArgs>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(IPlayer player, Func<Event<PlayerDeathArgs>, bool> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(IPlayer player, Action<Event<PlayerDeathArgs>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(IPlayer player, Action<Event<IPlayer, PlayerDeathArgs>> callback,
        EventHookMode hookMode, bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(Action<Event<IPlayer>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(Action<Event<IPlayer, PlayerDeathArgs>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(Func<Event<IPlayer>, bool> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerDeathCallback))]
    IEventSubscription HookOnDead(IPlayer player, Action<Event> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.ProjectileHitCallback))]
    IEventSubscription HookOnProjectileHit(IProjectile projectile, Action<Event<ProjectileHitArgs>> callback,
        EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerWeaponAddedActionCallback))]
    IEventSubscription HookOnWeaponAdded(IPlayer player, Action<Event<PlayerWeaponAddedArg>> callback,
        EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerWeaponAddedActionCallback))]
    IEventSubscription HookOnWeaponAdded(Action<Event<IPlayer, PlayerWeaponAddedArg>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerWeaponRemovedActionCallback))]
    IEventSubscription HookOnWeaponRemoved(Action<Event<IPlayer, PlayerWeaponRemovedArg>> callback,
        EventHookMode hookMode, bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerMeleeActionCallback))]
    IEventSubscription HookOnPlayerMeleeAction(IPlayer player, Action<Event<PlayerMeleeHitArg[]>> callback,
        EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerMeleeActionCallback))]
    IEventSubscription HookOnPlayerMeleeAction(Action<Event<IPlayer, PlayerMeleeHitArg[]>> callback,
        EventHookMode hookMode, bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.UpdateCallback))]
    IEventSubscription HookOnUpdate(Action<Event<float>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.UpdateCallback))]
    IEventSubscription HookOnUpdate(Func<Event<float>, bool> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.UserMessageCallback))]
    IEventSubscription HookOnMessage(Action<Event<UserMessageCallbackArgs>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.ProjectileCreatedCallback))]
    IEventSubscription HookOnProjectilesCreated(Action<Event<IProjectile[]>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.UserJoinCallback))]
    IEventSubscription HookOnUserJoined(Action<Event<IUser[]>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerKeyInputCallback))]
    IEventSubscription HookOnKeyInput(Action<Event<IPlayer, VirtualKeyInfo[]>> callback,
        EventHookMode hookMode, bool ignoreHandled = false);

    [GameCallbackType(typeof(Events.PlayerCreatedCallback))]
    IEventSubscription HookOnPlayerCreated(Action<Event<IPlayer[]>> callback, EventHookMode hookMode,
        bool ignoreHandled = false);

    void Clear();
}

[Inject]
internal partial class ExtendedEvents : IExtendedEvents
{
    [InjectLogger]
    private static ILogger Logger { get; set; }

    private static partial ILogger GetLogger()
    {
        return Logger;
    }
}