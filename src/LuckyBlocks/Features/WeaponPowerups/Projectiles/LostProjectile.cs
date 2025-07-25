using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class LostProjectile : ProjectilePowerupBase
{
    protected override float ProjectileSpeedModifier => 1 / 5f;

    private const int CollisionVectorLength = 50;

    private static readonly IReadOnlyList<Vector2> CollisionVectors = Enumerable.Range(0, 12)
        .Select(x => x * 30)
        .Select(x => x * Math.PI / 180)
        .Select(x => new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) * CollisionVectorLength)
        .ToList();

    private static IObject[]? _spawnPoints;

    private readonly IGame _game;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly PowerupConstructorArgs _args;
    private readonly PeriodicTimer<IProjectile> _periodicTimer;

    public LostProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args) :
        base(projectile, extendedEvents, args)
    {
        _game = args.Game;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
        _periodicTimer = new PeriodicTimer<IProjectile>(TimeSpan.FromMilliseconds(50), TimeBehavior.TimeModifier,
            PlayLostEffect, x => x.IsRemoved, null, projectile, ExtendedEvents);
        _spawnPoints ??= _game.GetObjectsByName("SpawnPlayer");
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new LostProjectile(Projectile, ExtendedEvents, _args);
    }

    protected override void OnRunInternal()
    {
        var spawnPoint = _spawnPoints!.GetRandomElement();

        Projectile.Position = spawnPoint.GetWorldPosition();
        Projectile.Direction = GetProjectileDirection();

        _periodicTimer.Start();
    }

    protected override void OnDisposedInternal()
    {
        _periodicTimer.Stop();
    }

    private Vector2 GetProjectileDirection()
    {
        var results = new List<(Vector2 CollisionVector, RayCastResult Result)>();

        foreach (var collisionVector in CollisionVectors)
        {
            var result = _game.RayCast(Projectile.Position, Projectile.Position + collisionVector,
                new RayCastInput(true))[0];
            results.Add((collisionVector, result));
        }

        if (results.Any(x => !x.Result.Hit))
        {
            return results
                .Where(x => !x.Result.Hit)
                .ToList()
                .GetRandomElement()
                .CollisionVector;
        }

        return results
            .OrderByDescending(x => x.Result.Fraction)
            .First()
            .CollisionVector;
    }

    private void PlayLostEffect(IProjectile projectile)
    {
        for (var i = 0; i < 3; i++)
        {
            _effectsPlayer.PlayLostEffect(projectile.Position);
        }
    }
}