using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal class TimeStoppedGrenade : TimeStoppedDynamicObject
{
    private readonly IObjectGrenadeThrown _grenade;

    private float _explosionTimer;
    private IEventSubscription? _updateSubscription;

    public TimeStoppedGrenade(IObjectGrenadeThrown grenade, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents) : base(grenade, game, effectsPlayer, extendedEvents)
    {
        _grenade = grenade;
    }

    protected override void InitializeInternal()
    {
        base.InitializeInternal();

        _explosionTimer = _grenade.GetExplosionTimer();
        _updateSubscription = ExtendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    protected override void ResumeTimeInternal()
    {
        base.ResumeTimeInternal();

        _grenade.SetExplosionTimer(_explosionTimer);
    }

    protected override void DisposeInternal()
    {
        base.DisposeInternal();

        _updateSubscription?.Dispose();
    }

    private void OnUpdate(Event<float> @event)
    {
        _grenade.SetExplosionTimer(_explosionTimer);
    }
}