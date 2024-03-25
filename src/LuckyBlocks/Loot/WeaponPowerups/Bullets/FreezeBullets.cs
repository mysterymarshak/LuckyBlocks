using System;
using LuckyBlocks.Data;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Buffs.Durable;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Bullets;

internal class FreezeBullets : BulletsPowerupBase
{
    public override string Name => "Freeze bullets";

    private static TimeSpan FreezeTime => TimeSpan.FromMilliseconds(3000);

    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public FreezeBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
        => (_buffsService, _identityService, _effectsPlayer, _game, _args) =
            (args.BuffsService, args.IdentityService, args.EffectsPlayer, args.Game, args);

    protected override void OnFire(IPlayer player, IProjectile projectile)
    {
        var freezeBullet = new FreezeBullet(projectile, _effectsPlayer, ExtendedEvents);
        freezeBullet.PlayerHit += OnBulletPlayerHit;
        freezeBullet.Remove += OnBulletRemoved;
    }

    private void OnBulletRemoved(IBullet bullet, ProjectileHitArgs args)
    {
        bullet.Remove -= OnBulletRemoved;
        ((FreezeBullet)bullet).PlayerHit -= OnBulletPlayerHit;
        bullet.Dispose();
    }

    private void OnBulletPlayerHit(FreezeBullet bullet, ProjectileHitArgs args)
    {
        var playerInstance = _game.GetPlayer(args.HitObjectID);
        if (playerInstance.IsDead)
            return;
        
        playerInstance.SetHealth(playerInstance.GetHealth() + args.Damage);
        
        var player = _identityService.GetPlayerByInstance(playerInstance);
        var freeze = new Freeze(player, _args.BuffConstructorArgs, FreezeTime);
        _buffsService.TryAddBuff(freeze, player);
    }

    private class FreezeBullet : BulletBase
    {
        public event Action<FreezeBullet, ProjectileHitArgs>? PlayerHit;

        protected override float ProjectileSpeedDivider => 2;

        private readonly IEffectsPlayer _effectsPlayer;
        private readonly PeriodicTimer<IProjectile> _periodicTimer;

        public FreezeBullet(IProjectile projectile, IEffectsPlayer effectsPlayer, IExtendedEvents extendedEvents) : base(projectile, extendedEvents)
        {
            _effectsPlayer = effectsPlayer;
            projectile.Velocity = GetNewProjectileVelocity();
            _periodicTimer = new PeriodicTimer<IProjectile>(TimeSpan.FromMilliseconds(50), TimeBehavior.TimeModifier, PlayFreezeEffect,
                projectile => projectile.IsRemoved, default, projectile, ExtendedEvents);
            _periodicTimer.Start();
        }

        protected override void OnHit(ProjectileHitArgs args)
        {
            if (!args.IsPlayer)
                return;

            PlayerHit?.Invoke(this, args);
        }

        protected override void OnDisposed()
        {
            _periodicTimer.Stop();
        }

        private void PlayFreezeEffect(IProjectile projectile)
        {
            _effectsPlayer.PlayEffect(EffectName.Electric, projectile.Position);
        }
    }
}