using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Mathematics;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class AimBullets : BulletsPowerupBase
{
    public override string Name => "Aim bullets";

    private const int FIND_TARGET_RADIUS = 100;
    private const double FIND_TARGET_FOV = 2 * Math.PI / 3; 
    
    private readonly IGame _game;

    public AimBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
        => (_game) = (args.Game);

    protected override void OnFire(IPlayer player, IProjectile projectile)
    {
        var aimBullet = new AimBullet(projectile, _game, ExtendedEvents);
        aimBullet.Remove += OnBulletRemoved;
    }

    private void OnBulletRemoved(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Remove -= OnBulletRemoved;
        bullet.Dispose();
    }

    private class AimBullet : BulletBase
    {
        protected override float ProjectileSpeedDivider => 3;
        
        private static Vector2 PlayerPositionOffset => new(0, 5);

        private readonly IGame _game;
        private readonly IEventSubscription _updateEventSubscription;
        
        private IPlayer? _target;
        
        public AimBullet(IProjectile projectile, IGame game, IExtendedEvents extendedEvents) : base(projectile, extendedEvents)
        {
            _game = game;
            projectile.Velocity = GetNewProjectileVelocity();
            _updateEventSubscription = ExtendedEvents.HookOnUpdate(OnUpdate, EventHookMode.Default);
        }

        private void OnUpdate(Event<float> @event)
        {
            if (_target is null)
            {
                FindTarget();
                return;
            }

            if (!_target.IsValid() || _target.IsDead)
            {
                _target = null;
                return;
            }

            const float minDistanceToPlayer = 14f;
            
            var playerPosition = _target.GetWorldPosition() + PlayerPositionOffset;
            var bulletPosition = Projectile.Position;
            var vectorToBullet = playerPosition - bulletPosition;
            
            if (vectorToBullet.Length() < minDistanceToPlayer)
                return;
            
            vectorToBullet.Normalize();
            Projectile.Direction = vectorToBullet;
        }

        private void FindTarget()
        {
            var position = Projectile.Position;
            var direction = Projectile.Direction;
            
            var a = (direction * FIND_TARGET_RADIUS).Rotate(FIND_TARGET_FOV / 2);
            var b = (direction * FIND_TARGET_RADIUS).Rotate(-(FIND_TARGET_FOV / 2));

            var triangle = new Triangle(position, position + a, position + b);
            var players = _game.GetPlayers()
                .Where(x => x.IsValid() && !x.IsDead);

            List<IPlayer>? candidates = default;
            
            foreach (var player in players)
            {
                var playerPosition = player.GetWorldPosition() + PlayerPositionOffset;
                if (!triangle.IsInTriangle(playerPosition))
                    continue;

                candidates ??= new();
                candidates.Add(player);
            }
            
            if (candidates is null)
                return;

            _target = candidates
                .OrderBy(x => Vector2.Distance(x.GetWorldPosition(), position))
                .First();
        }

        protected override void OnDisposed()
        {
            _updateEventSubscription.Dispose();
        }
    }
}