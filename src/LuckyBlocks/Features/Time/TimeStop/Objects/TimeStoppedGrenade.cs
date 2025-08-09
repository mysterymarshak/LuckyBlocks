using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Time.TimeStop.Objects;

internal class TimeStoppedGrenade : TimeStoppedDynamicObject
{
    private readonly IObjectGrenadeThrown _grenade;

    private float _explosionTimer;

    public TimeStoppedGrenade(IObjectGrenadeThrown grenade, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents) : base(grenade, game, effectsPlayer, extendedEvents)
    {
        _grenade = grenade;
    }

    protected override void InitializeInternal()
    {
        base.InitializeInternal();

        _explosionTimer = _grenade.GetExplosionTimer();
        ExtendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
    }

    protected override void ResumeTimeInternal()
    {
        base.ResumeTimeInternal();

        _grenade.SetExplosionTimer(_explosionTimer);
    }

    private void OnUpdate(Event<float> @event)
    {
        _grenade.SetExplosionTimer(_explosionTimer);
    }
}