using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class PoisonProjectile : ProjectilePowerupBase
{
    protected override float ProjectileSpeedModifier => 1 / 2f;

    private readonly IGame _game;
    private readonly IIdentityService _identityService;
    private readonly IBuffsService _buffsService;
    private readonly BuffConstructorArgs _buffConstructorArgs;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly PeriodicTimer _periodicTimer;
    private readonly PowerupConstructorArgs _args;

    public PoisonProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args) : base(
        projectile, extendedEvents, args)
    {
        _game = args.Game;
        _identityService = args.IdentityService;
        _buffsService = args.BuffsService;
        _buffConstructorArgs = args.BuffConstructorArgs;
        _effectsPlayer = args.EffectsPlayer;
        _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(50), TimeBehavior.TimeModifier, PlayPoisonEffect,
            null, int.MaxValue, ExtendedEvents);
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new PoisonProjectile(Projectile, ExtendedEvents, _args);
    }

    protected override void OnRunInternal()
    {
        _periodicTimer.Start();
    }

    protected override void OnHitInternal(ProjectileHitArgs args)
    {
        if (!args.IsPlayer)
            return;

        var playerInstance = _game.GetPlayer(args.HitObjectID);
        if (playerInstance.IsDead)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);
        var freeze = new DurablePoison(player, _buffConstructorArgs);
        _buffsService.TryAddBuff(freeze, player);
    }

    protected override void OnDisposedInternal()
    {
        _periodicTimer.Stop();
    }

    private void PlayPoisonEffect()
    {
        _effectsPlayer.PlayEffect(EffectName.AcidSplash, Projectile.Position);
    }
}