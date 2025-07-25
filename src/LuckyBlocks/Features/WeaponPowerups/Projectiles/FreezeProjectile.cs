using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Projectiles;

internal class FreezeProjectile : ProjectilePowerupBase
{
    protected override float ProjectileSpeedModifier => 1 / 2f;

    private static TimeSpan FreezeTime => TimeSpan.FromMilliseconds(3000);

    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly IIdentityService _identityService;
    private readonly IBuffsService _buffsService;
    private readonly BuffConstructorArgs _buffConstructorArgs;
    private readonly PeriodicTimer _periodicTimer;
    private readonly PowerupConstructorArgs _args;

    public FreezeProjectile(IProjectile projectile, IExtendedEvents extendedEvents, PowerupConstructorArgs args) : base(
        projectile, extendedEvents, args)
    {
        _effectsPlayer = args.EffectsPlayer;
        _game = args.Game;
        _identityService = args.IdentityService;
        _buffsService = args.BuffsService;
        _buffConstructorArgs = args.BuffConstructorArgs;
        _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(50), TimeBehavior.TimeModifier, PlayFreezeEffect,
            null, int.MaxValue, ExtendedEvents);
        _args = args;
    }

    protected override ProjectilePowerupBase CloneInternal()
    {
        return new FreezeProjectile(Projectile, ExtendedEvents, _args);
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
        var freeze = new Freeze(player, _buffConstructorArgs, FreezeTime);
        _buffsService.TryAddBuff(freeze, player);
    }

    protected override void OnDisposedInternal()
    {
        _periodicTimer.Stop();
    }

    private void PlayFreezeEffect()
    {
        _effectsPlayer.PlayEffect(EffectName.Electric, Projectile.Position);
    }
}