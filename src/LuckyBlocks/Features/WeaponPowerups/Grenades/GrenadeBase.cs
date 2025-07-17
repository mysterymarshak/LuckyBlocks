using System;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Grenades;

internal abstract class GrenadeBase
{
    public event Action<IObjectGrenadeThrown, GrenadeBase>? Destroy;

    public bool IsCloned { get; private set; }

    protected IObjectGrenadeThrown Grenade { get; private set; }

    private readonly IExtendedEvents _extendedEvents;
    private readonly Action<IObject, IExtendedEvents> _createPaintDelegate;

    private IEventSubscription? _disposeEventSubscription;

    protected GrenadeBase(IObjectGrenadeThrown grenade, IExtendedEvents extendedEvents,
        Action<IObject, IExtendedEvents> createPaintDelegate)
    {
        Grenade = grenade;
        _extendedEvents = extendedEvents;
        _createPaintDelegate = createPaintDelegate;
    }

    public GrenadeBase Clone()
    {
        var clonedGrenade = CloneInternal();
        clonedGrenade.IsCloned = true;
        return clonedGrenade;
    }

    public void MoveTo(IObjectGrenadeThrown grenade)
    {
        Grenade = grenade;
        Initialize();
    }

    public virtual void Initialize()
    {
        _disposeEventSubscription =
            _extendedEvents.HookOnDestroyed(Grenade, _ => Dispose(), EventHookMode.Default);
        Grenade.SetDudChance(0f);

        _createPaintDelegate.Invoke(Grenade, _extendedEvents);
    }

    protected abstract GrenadeBase CloneInternal();

    protected virtual void Dispose()
    {
        _disposeEventSubscription?.Dispose();
        Destroy?.Invoke(Grenade, this);
    }
}