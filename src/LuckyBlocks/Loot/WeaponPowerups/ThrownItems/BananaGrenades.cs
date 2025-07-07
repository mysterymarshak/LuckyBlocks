using System;
using System.Collections.Generic;
using System.Threading;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

internal class BananaGrenades : GrenadesPowerupBase
{
    public override Color PaintColor => Color.Yellow;
    public override string Name => "Banana grenades";
    public override int UsesCount => 1;

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(StickyGrenades)];

    private readonly Func<IObjectGrenadeThrown, BananaGrenade> _createGrenadeAndPaintDelegate;
    private readonly PowerupConstructorArgs _args;

    public BananaGrenades(Grenade grenade, PowerupConstructorArgs args) : base(grenade, args)
    {
        _createGrenadeAndPaintDelegate = grenadeThrown => (BananaGrenade)CreateGrenadeAndPaint(grenadeThrown);
        _args = args;
    }

    public override IWeaponPowerup<Grenade> Clone(Weapon weapon)
    {
        var grenade = weapon as Grenade;
        ArgumentWasNullException.ThrowIfNull(grenade);
        return new BananaGrenades(grenade, _args) { UsesLeft = UsesLeft };
    }

    protected override GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents)
    {
        return new BananaGrenade(grenadeThrown, game, extendedEvents, _createGrenadeAndPaintDelegate);
    }

    private class BananaGrenade : GrenadeBase
    {
        private static TimeSpan SplitDelay => TimeSpan.FromMilliseconds(150);
        private static TimeSpan ChildGrenadesExplosionTimer => TimeSpan.FromMilliseconds(700);

        private readonly IObjectGrenadeThrown _grenade;
        private readonly IGame _game;
        private readonly Func<IObjectGrenadeThrown, BananaGrenade> _createGrenadeAndPaintDelegate;
        private readonly CancellationTokenSource _cts;

        private bool _isDisposed;
        private bool _isSplittingScheduled;
        private bool _allowSplitting;

        public BananaGrenade(IObjectGrenadeThrown grenade, IGame game, IExtendedEvents extendedEvents,
            Func<IObjectGrenadeThrown, BananaGrenade> createGrenadeAndPaintDelegate) : base(grenade, extendedEvents)
        {
            _grenade = grenade;
            _game = game;
            _createGrenadeAndPaintDelegate = createGrenadeAndPaintDelegate;
            _allowSplitting = true;
            _cts = new CancellationTokenSource();
        }

        public override void Initialize()
        {
            base.Initialize();

            Awaiter.Start(OnExplosion, TimeSpan.FromMilliseconds(_grenade.GetExplosionTimer()), _cts.Token);
        }

        private void DisallowSplitting()
        {
            _allowSplitting = false;
        }

        private void OnExplosion()
        {
            if (!_allowSplitting)
                return;

            var parentAngle = _grenade.GetAngle();
            var parentPosition = _grenade.GetWorldPosition();
            var angularVelocity = _grenade.GetAngularVelocity();

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

                    var grenade = _createGrenadeAndPaintDelegate(grenadeThrown);
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

            _cts.Cancel();
            _cts.Dispose();

            _isDisposed = true;

            if (!_isSplittingScheduled)
            {
                OnExplosion();
            }

            base.Dispose();
        }
    }
}