using System;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.ShockedObjects;
using LuckyBlocks.Loot.Buffs.Durable;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal class ElectricMagic : AreaMagicBase
{
    public override AreaMagicType Type => AreaMagicType.Electric;
    public override string Name => "Electric magic";

    private static TimeSpan ObjectShockTime => TimeSpan.FromSeconds(7);
    private static TimeSpan PlayerShockTime => TimeSpan.FromMilliseconds(3500);

    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    private readonly IShockedObjectsService _shockedObjectsService;
    private readonly BuffConstructorArgs _args;

    public ElectricMagic(Player wizard, BuffConstructorArgs args, int direction = default) :
        base(wizard, args, direction) => (_buffsService, _identityService, _shockedObjectsService, _args) =
        (args.BuffsService, args.IdentityService, args.ShockedObjectsService, args);

    public override void PlayEffects(Area area)
    {
        PlayEffects(EffectName.Electric, area, Direction);
    }

    protected override void CastInternal(Area fullArea, Area iterationArea)
    {
        var objects = GetAffectedObjectsByArea(fullArea, iterationArea);
        foreach (var @object in objects)
        {
            ShockObject(@object);
        }

        PlayEffects(iterationArea);
    }

    private void ShockObject(IObject @object)
    {
        if (_shockedObjectsService.IsShocked(@object))
            return;

        if (@object is IPlayer player)
        {
            ShockPlayer(player);
            return;
        }

        if (@object.GetBodyType() == BodyType.Static || @object.GetPhysicsLayer() != PhysicsLayer.Active)
            return;

        _shockedObjectsService.Shock(@object, ObjectShockTime);
    }

    private void ShockPlayer(IPlayer playerInstance)
    {
        var wizardInstance = Wizard.Instance;

        if (!playerInstance.IsValidUser() || playerInstance.IsDead)
            return;

        if (playerInstance == wizardInstance)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);
        if (player.HasBuff(typeof(Shock)))
            return;

        var shock = new Shock(player, _args, PlayerShockTime);
        _buffsService.TryAddBuff(shock, player);
    }
}