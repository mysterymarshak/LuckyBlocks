using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Buffs.Durable;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class PoisonBullets : BulletsPowerupBase
{
    public override string Name => "Poison bullets";

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups =
    [
        typeof(ExplosiveBullets), typeof(PushBullets),
        typeof(InfiniteRicochetBullets), typeof(TripleRicochetBullets)
    ];

    private readonly IIdentityService _identityService;
    private readonly IBuffsService _buffsService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly BuffConstructorArgs _buffArgs;
    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public PoisonBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _identityService = args.IdentityService;
        _buffsService = args.BuffsService;
        _effectsPlayer = args.EffectsPlayer;
        _buffArgs = args.BuffConstructorArgs;
        _game = args.Game;
        _args = args;
    }

    public override IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new PoisonBullets(firearm, _args) { UsesLeft = UsesLeft };
    }

    protected override void OnFireInternal(IPlayer playerInstance, IProjectile projectile)
    {
        var bullet = new PoisonBullet(projectile, _effectsPlayer, ExtendedEvents);
        bullet.Hit += OnBulletHit;
        bullet.Remove += OnBulletRemove;
    }

    private void OnBulletRemove(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Remove -= OnBulletRemove;
        bullet.Hit -= OnBulletHit;
        bullet.Dispose();
    }

    private void OnBulletHit(IBullet bullet, ProjectileHitArgs args)
    {
        if (!args.IsPlayer)
            return;

        var playerInstance = _game.GetPlayer(args.HitObjectID);
        if (playerInstance.IsDead)
            return;

        // playerInstance.SetHealth(playerInstance.GetHealth() + args.Damage);

        var player = _identityService.GetPlayerByInstance(playerInstance);
        var freeze = new DurablePoison(player, _buffArgs);
        _buffsService.TryAddBuff(freeze, player);
    }

    private class PoisonBullet : BulletBase
    {
        protected override float ProjectileSpeedDivider => 2;

        private readonly IEffectsPlayer _effectsPlayer;
        private readonly PeriodicTimer<IProjectile> _periodicTimer;

        public PoisonBullet(IProjectile projectile, IEffectsPlayer effectsPlayer, IExtendedEvents extendedEvents) :
            base(projectile, extendedEvents)
        {
            _effectsPlayer = effectsPlayer;
            projectile.Velocity = GetNewProjectileVelocity();
            _periodicTimer = new PeriodicTimer<IProjectile>(TimeSpan.FromMilliseconds(50), TimeBehavior.TimeModifier,
                PlayEffect, projectile => projectile.IsRemoved, default, projectile, ExtendedEvents);
            _periodicTimer.Start();
        }

        protected override void OnDisposed()
        {
            _periodicTimer.Stop();
        }

        private void PlayEffect(IProjectile projectile)
        {
            _effectsPlayer.PlayEffect(EffectName.AcidSplash, projectile.Position);
        }
    }
}