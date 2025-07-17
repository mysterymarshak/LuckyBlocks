using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Mathematics;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class AimProjectile : ProjectilePowerupBase
{
    protected override float ProjectileSpeedModifier => 1 / 3f;

    private static Vector2 PlayerPositionOffset => new(0, 5);
    private const int FindTargetRadius = 100;
    private const double FindTargetFov = 2 * Math.PI / 3;

    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    private IEventSubscription? _updateEventSubscription;
    private IPlayer? _target;

    public AimProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args) : base(
        projectile, extendedEvents, args)
    {
        _game = args.Game;
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new AimProjectile(Projectile, ExtendedEvents, _args) { _target = _target };
        // what if respawned
    }

    protected override void OnRunInternal()
    {
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
            FindTarget();
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

        var a = (direction * FindTargetRadius).Rotate(FindTargetFov / 2);
        var b = (direction * FindTargetRadius).Rotate(-(FindTargetFov / 2));

        var triangle = new Triangle(position, position + a, position + b);
        var players = _game.GetPlayers()
            .Where(x => x.IsValid() && !x.IsDead);

        List<IPlayer>? candidates = default;

        foreach (var player in players)
        {
            var playerPosition = player.GetWorldPosition() + PlayerPositionOffset;
            if (!triangle.IsInTriangle(playerPosition))
                continue;

            candidates ??= [];
            candidates.Add(player);
        }

        if (candidates is null)
            return;

        _target = candidates
            .OrderBy(x => Vector2.Distance(x.GetWorldPosition(), position))
            .First();
    }

    protected override void OnDisposedInternal()
    {
        _updateEventSubscription?.Dispose();
    }
}