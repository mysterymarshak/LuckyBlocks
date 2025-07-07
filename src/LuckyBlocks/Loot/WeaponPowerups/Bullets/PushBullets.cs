using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class PushBullets : BulletsPowerupBase
{
    public override string Name => "Push bullets";

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(TripleRicochetBullets), typeof(FreezeBullets)];

    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public PushBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _game = args.Game;
        _args = args;
    }

    public override IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new PushBullets(firearm, _args) { UsesLeft = UsesLeft };
    }

    protected override void OnFired(IPlayer player, IProjectile projectile)
    {
        var pushBullet = new PushBullet(projectile, _game, ExtendedEvents);
        pushBullet.Remove += OnBulletRemoved;
    }

    private void OnBulletRemoved(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Remove -= OnBulletRemoved;
        bullet.Dispose();
    }

    private class PushBullet : BulletBase
    {
        protected override float ProjectileSpeedDivider => 4.5f;

        private readonly IGame _game;
        private readonly IEventSubscription _updateEventSubscription;

        public PushBullet(IProjectile projectile, IGame game, IExtendedEvents extendedEvents) : base(projectile,
            extendedEvents)
        {
            _game = game;
            projectile.Velocity = GetNewProjectileVelocity();
            _updateEventSubscription = ExtendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
        }

        protected override void OnDisposed()
        {
            _updateEventSubscription.Dispose();
        }

        private void OnUpdate(Event<float> @event)
        {
            const int diagonalHalf = 10;

            var position = Projectile.Position;
            var min = new Vector2(position.X - diagonalHalf, position.Y - diagonalHalf);
            var max = new Vector2(position.X + diagonalHalf, position.Y + diagonalHalf);
            var area = new Area(min, max);

            var objects = _game
                .GetObjectsByArea(area)
                .Where(x => x.GetBodyType() != BodyType.Static)
                .Where(x => x.GetPhysicsLayer() == PhysicsLayer.Active);

            foreach (var @object in objects)
            {
                if (@object.UniqueId == Projectile.InitialOwnerPlayerID)
                    continue;

                @object.SetLinearVelocity(Projectile.Velocity / 20);
            }
        }
    }
}