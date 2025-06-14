using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Immunity;
using LuckyBlocks.Loot.Buffs.Instant;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal class WindMagic : AreaMagicBase
{
    public override AreaMagicType Type => AreaMagicType.Wind;
    public override TimeSpan PropagationTime => TimeSpan.FromMilliseconds(500);
    public override string Name => "Wind magic";

    private static Vector2 PushSpeed => new(20, 0);

    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    private readonly IGame _game;

    private List<IObject>? _pushedObjects;
    private List<IProjectile>? _reflectedProjectiles;

    public WindMagic(Player wizard, BuffConstructorArgs args, int direction = default) : base(wizard, args, direction)
        => (_buffsService, _identityService, _game) = (args.BuffsService, args.IdentityService, args.Game);

    public override void PlayEffects(Area area)
    {
        PlayEffects(EffectName.Steam, area, Direction);
    }

    protected override void CastInternal(Area area)
    {
        var objects = GetObjectsByArea(area)
            .Where(x => _pushedObjects?.Contains(x) == false)
            .ToList();

        ClearFire(objects, area);

        var wizardInstance = Wizard.Instance;

        var objectsToPush = objects
            .Where(x => x.GetBodyType() != BodyType.Static)
            .Where(x => x.UniqueId != wizardInstance?.UniqueId)
            .ToList();

        _pushedObjects ??= new();
        _pushedObjects.AddRange(objectsToPush);

        foreach (var objectToPush in objectsToPush)
        {
            if (objectToPush is not IPlayer playerInstance || !playerInstance.IsValidUser())
                continue;

            // decoys cant be disarmed

            var hasImmunity = HasPlayerImmunity(playerInstance);
            if (hasImmunity)
                continue;

            var player = _identityService.GetPlayerByInstance(playerInstance);
            var disarm = new Disarm(player);
            _buffsService.TryAddBuff(disarm, player);

            objectToPush.SetLinearVelocity(new Vector2(0, 3));
        }

        ScheduleWind(objects, Direction);
        ReflectProjectiles(area);
        PlayEffects(area);
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
        Awaiter.Start(delegate()
        {
            foreach (var obj in objects)
            {
                if (obj is IPlayer playerInstance && playerInstance.IsValidUser() && HasPlayerImmunity(playerInstance))
                    continue;

                obj.SetLinearVelocity(new Vector2(0, obj.GetLinearVelocity().Y) + PushSpeed * direction);
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

            _reflectedProjectiles ??= new();
            _reflectedProjectiles.Add(projectile);

            projectile.Velocity = -projectile.Velocity;
        }
    }
}