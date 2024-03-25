using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class LegacyAimBullets : BulletsPowerupBase
{
    public override string Name => "Aim bullets";

    private readonly IPlayersTrajectoryWatcher _playersTrajectoryWatcher;
    private readonly IGame _game;
    private readonly Dictionary<int, BulletTrajectoryWatcher> _bulletsTrajectoryWatchers;

    public LegacyAimBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
        => (_playersTrajectoryWatcher, _game, _bulletsTrajectoryWatchers) =
            (args.PlayersTrajectoryWatcher, args.Game, new());

    protected override void OnFire(IPlayer player, IProjectile projectile)
    {
        projectile.Velocity /= 4;

        var bullet = new Bullet(projectile, ExtendedEvents);
        bullet.Hit += OnBulletHit;

        var bulletsTrajectoryWatcher = new BulletTrajectoryWatcher(projectile, _playersTrajectoryWatcher, _game);
        _bulletsTrajectoryWatchers[projectile.InstanceID] = bulletsTrajectoryWatcher;
        bulletsTrajectoryWatcher.Start();
    }

    private void OnBulletHit(IBullet bullet, ProjectileHitArgs args)
    {
        if (!args.RemoveFlag)
            return;

        bullet.Hit -= OnBulletHit;
        bullet.Dispose();

        var bulletTrajectoryWatcher = _bulletsTrajectoryWatchers[bullet.Projectile.InstanceID];
        bulletTrajectoryWatcher.Stop();
    }

    private class BulletTrajectoryWatcher
    {
        private readonly IProjectile _projectile;
        private readonly IPlayersTrajectoryWatcher _playersTrajectoryWatcher;
        private readonly IGame _game;

        private Events.UpdateCallback? _updateCallback;
        private TrajectoryNode _previousNode;

        public BulletTrajectoryWatcher(IProjectile projectile, IPlayersTrajectoryWatcher playersTrajectoryWatcher,
            IGame game)
            => (_projectile, _playersTrajectoryWatcher, _game) = (projectile, playersTrajectoryWatcher, game);

        public void Start()
        {
            _updateCallback = Events.UpdateCallback.Start(OnUpdate);
        }

        public void Stop()
        {
            _updateCallback?.Stop();
        }

        private void OnUpdate(float elapsed)
        {
            var playerId = _projectile.InitialOwnerPlayerID;
            var position = _projectile.Position;

            var raycastResult = _game.RayCast(position, _projectile.Direction * 100,
                new RayCastInput { Types = new[] { typeof(IPlayer) }, ClosestHitOnly = true });
            if (raycastResult.Any(x => x is { Hit: true, IsPlayer: true }))
                return;

            var getTrajectoryNodeResult = _playersTrajectoryWatcher.GetTrajectoryNodeFromPosition(position, playerId);
            if (!getTrajectoryNodeResult.TryPickT0(out var trajectoryNode, out _))
                return;

            _updateCallback?.Stop();
            _previousNode = trajectoryNode;

            FollowToNextPosition(default);
        }

        private void FollowToNextPosition(float e)
        {
            if (_projectile.IsRemoved)
                return;

            var playerId = _previousNode.PlayerId;
            var previousNodeId = _previousNode.Id;
            var getTrajectoryNodeResult = _playersTrajectoryWatcher.GetNextNodeForPlayer(playerId, previousNodeId);

            if (!getTrajectoryNodeResult.TryPickT0(out var trajectoryNode, out _))
            {
                Stop();
                return;
            }

            var direction = trajectoryNode.Position - _previousNode.Position;
            direction.Normalize();

            _projectile.Direction = direction;
            _projectile.Position = trajectoryNode.Position;

            _previousNode = trajectoryNode;
            Awaiter.Start(FollowToNextPosition, TimeSpan.FromMilliseconds(Math.Max((trajectoryNode.ElapsedTimeFromPrevious - e) / 3, 0)));
        }
    }
}