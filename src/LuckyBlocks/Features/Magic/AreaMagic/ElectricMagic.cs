using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Durable;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.ShockedObjects;
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
    private readonly BuffConstructorArgs _buffConstructorArgs;
    private readonly MagicConstructorArgs _args;

    public ElectricMagic(Player wizard, MagicConstructorArgs args, BuffConstructorArgs buffConstructorArgs,
        int direction = default) :
        base(wizard, args, direction)
    {
        _buffsService = args.BuffsService;
        _identityService = args.IdentityService;
        _shockedObjectsService = args.ShockedObjectsService;
        _buffConstructorArgs = buffConstructorArgs;
        _args = args;
    }

    public override void PlayEffects(Area area)
    {
        PlayEffects(EffectName.Electric, area, Direction);
    }

    public override MagicBase Copy()
    {
        return new ElectricMagic(Wizard, _args, _buffConstructorArgs);
    }

    public override void Cast(Area fullArea, Area iterationArea)
    {
        var objects = GetAffectedObjectsByArea(fullArea, iterationArea);
        foreach (var @object in objects)
        {
            ShockObject(@object);
        }

        PlayEffects(iterationArea);

        base.Cast(fullArea, iterationArea);
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

        var shock = new Shock(player, _buffConstructorArgs, PlayerShockTime);
        _buffsService.TryAddBuff(shock, player);
    }
}