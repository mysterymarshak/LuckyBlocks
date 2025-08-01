﻿using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Instant;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal class WindMagic : AreaMagicBase
{
    public override AreaMagicType Type => AreaMagicType.Wind;
    public override TimeSpan PropagationTime => TimeSpan.FromMilliseconds(500);
    public override string Name => "Wind magic";

    private static Vector2 PushSpeed => new(20, 0);
    private const float PlayerPushSpeedModifier = 1f;

    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    private readonly IGame _game;
    private readonly MagicConstructorArgs _args;

    private List<IProjectile>? _reflectedProjectiles;

    public WindMagic(Player wizard, MagicConstructorArgs args, int direction = default) : base(wizard, args, direction)
    {
        _buffsService = args.BuffsService;
        _identityService = args.IdentityService;
        _game = args.Game;
        _args = args;
    }

    public override void PlayEffects(Area area)
    {
        PlayEffects(EffectName.Steam, area, Direction);
    }

    public override MagicBase Copy()
    {
        return new WindMagic(Wizard, _args) { _reflectedProjectiles = _reflectedProjectiles };
    }

    public override void Cast(Area fullArea, Area iterationArea)
    {
        var objects = GetAffectedObjectsByArea(fullArea, iterationArea).ToList();

        ClearFire(objects, fullArea);

        var wizardInstance = Wizard.Instance;

        var objectsToPush = objects
            .Where(x => x.GetBodyType() != BodyType.Static && x.UniqueId != wizardInstance?.UniqueId)
            .ToList();

        foreach (var objectToPush in objectsToPush)
        {
            if (objectToPush is not IPlayer playerInstance || !playerInstance.IsValid())
                continue;

            var hasImmunity = HasPlayerImmunity(playerInstance);
            if (hasImmunity)
                continue;

            var player = _identityService.GetPlayerByInstance(playerInstance);
            var disarm = new WindDisarm(player);
            _buffsService.TryAddBuff(disarm, player);

            playerInstance.LiftUp();
        }

        ScheduleWind(objects, Direction);
        ReflectProjectiles(iterationArea);
        PlayEffects(iterationArea);

        base.Cast(fullArea, iterationArea);
    }

    private void ClearFire(IEnumerable<IObject> objects, Area area)
    {
        var fireNodes = GetFireNodesByArea(area);
        foreach (var fireNode in fireNodes)
        {
            _game.EndFireNode(fireNode.InstanceID);
        }

        foreach (var @object in objects.Where(x => x.IsBurning))
        {
            @object.ClearFire();
        }
    }

    private void ScheduleWind(IReadOnlyList<IObject> objects, int direction)
    {
        Awaiter.Start(delegate
        {
            foreach (var @object in objects)
            {
                if (@object is IPlayer playerInstance && playerInstance.IsValid() && HasPlayerImmunity(playerInstance))
                    continue;

                @object.SetLinearVelocity(new Vector2(0, @object.GetLinearVelocity().Y) +
                                          PushSpeed * direction * (@object is IPlayer ? PlayerPushSpeedModifier : 1f));
            }
        }, TimeSpan.Zero);
    }

    private bool HasPlayerImmunity(IPlayer playerInstance)
    {
        var player = _identityService.GetPlayerByInstance(playerInstance);
        return player.GetImmunityFlags().HasFlag<ImmunityFlag>(ImmunityFlag.ImmunityToWind);
    }

    private void ReflectProjectiles(Area area)
    {
        var projectilesInArea = _game.GetProjectiles().Where(x => area.Contains(x.Position));
        foreach (var projectile in projectilesInArea)
        {
            if (_reflectedProjectiles?.Contains(projectile) == true)
                continue;

            _reflectedProjectiles ??= [];
            _reflectedProjectiles.Add(projectile);

            projectile.Velocity = -projectile.Velocity;
        }
    }
}