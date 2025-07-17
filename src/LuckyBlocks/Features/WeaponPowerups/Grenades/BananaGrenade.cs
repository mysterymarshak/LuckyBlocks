using System;
using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Grenades;

internal class BananaGrenade : GrenadeBase
{
    private static TimeSpan SplitDelay => TimeSpan.FromMilliseconds(150);
    private static TimeSpan ChildGrenadesExplosionTimer => TimeSpan.FromMilliseconds(700);

    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Action<IObject, IExtendedEvents> _createPaintDelegate;
    private readonly Func<IObjectGrenadeThrown, BananaGrenade> _createGrenadeDelegate;

    private float _explosionTimer;
    private bool _isDisposed;
    private bool _isSplittingScheduled;
    private bool _allowSplitting;

    public BananaGrenade(IObjectGrenadeThrown grenade, IGame game, IExtendedEvents extendedEvents,
        Action<IObject, IExtendedEvents> createPaintDelegate,
        Func<IObjectGrenadeThrown, BananaGrenade> createGrenadeDelegate) : base(grenade, extendedEvents,
        createPaintDelegate)
    {
        _game = game;
        _extendedEvents = extendedEvents;
        _createPaintDelegate = createPaintDelegate;
        _createGrenadeDelegate = createGrenadeDelegate;
        _allowSplitting = true;
    }

    public override void Initialize()
    {
        base.Initialize();

        if (IsCloned)
        {
            Grenade.SetExplosionTimer(_explosionTimer);
        }
    }

    protected override GrenadeBase CloneInternal()
    {
        return new BananaGrenade(Grenade, _game, _extendedEvents, _createPaintDelegate, _createGrenadeDelegate)
        {
            _allowSplitting = _allowSplitting,
            _explosionTimer = _isSplittingScheduled ? (float)SplitDelay.TotalMilliseconds : Grenade.GetExplosionTimer()
        };
    }

    private void DisallowSplitting()
    {
        _allowSplitting = false;
    }

    private void OnExplosion()
    {
        if (!_allowSplitting || _isSplittingScheduled)
            return;

        var parentAngle = Grenade.GetAngle();
        var parentPosition = Grenade.GetWorldPosition();
        var angularVelocity = Grenade.GetAngularVelocity();

        Awaiter.Start(delegate
        {
            for (var i = 0; i < 3; i++)
            {
                var angle = parentAngle + (i - 1) * (Math.PI / 6);
                var position = parentPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 0.5f;
                var velocity = new Vector2(0, SharedRandom.Instance.Next(5, 15)).Rotate((i - 1) * (Math.PI / 4) +
                    ((Math.PI / 6) * (SharedRandom.Instance.Next(0, 2) * SharedRandom.Instance.NextDouble() - 1)));
                var grenadeThrown =
                    (_game.CreateObject("WpnGrenadesThrown", position, (float)angle, velocity, angularVelocity) as
                        IObjectGrenadeThrown)!;
                grenadeThrown.TrackAsMissile(true);
                grenadeThrown.SetExplosionTimer((float)ChildGrenadesExplosionTimer.TotalMilliseconds);

                var grenade = _createGrenadeDelegate(grenadeThrown);
                grenade.Initialize();
                grenade.DisallowSplitting();
            }
        }, SplitDelay);

        _isSplittingScheduled = true;
    }

    protected override void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;

        OnExplosion();

        base.Dispose();
    }
}