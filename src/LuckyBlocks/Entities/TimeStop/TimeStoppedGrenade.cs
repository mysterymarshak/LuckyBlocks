using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities.TimeStop;

internal class TimeStoppedGrenade : TimeStoppedDynamicObject
{
    private readonly IObjectGrenadeThrown _grenade;

    private float _explosionTimer;

    public TimeStoppedGrenade(IObjectGrenadeThrown grenade, IGame game, IEffectsPlayer effectsPlayer,
        IExtendedEvents extendedEvents) : base(grenade, game, effectsPlayer, extendedEvents)
        => (_grenade) = (grenade);

    protected override void InitializeInternal()
    {
        base.InitializeInternal();

        _explosionTimer = _grenade.GetExplosionTimer();
    }

    protected override void ResumeTimeInternal()
    {
        base.ResumeTimeInternal();

        _grenade.SetExplosionTimer(_explosionTimer);
    }

    protected override void OnUpdate()
    {
        _grenade.SetExplosionTimer(_explosionTimer);
    }
}