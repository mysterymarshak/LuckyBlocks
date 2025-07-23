using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Buffs.Instant;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Magic.AreaMagic;

internal class FireMagic : AreaMagicBase
{
    public override AreaMagicType Type => AreaMagicType.Fire;
    public override string Name => "Fire magic";

    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    private readonly IGame _game;
    private readonly MagicConstructorArgs _args;

    private List<Player>? _ignitedPlayers;

    public FireMagic(Player wizard, MagicConstructorArgs args, int direction = default) : base(wizard, args, direction)
    {
        _buffsService = args.BuffsService;
        _identityService = args.IdentityService;
        _game = args.Game;
        _args = args;
    }

    public override void PlayEffects(Area area)
    {
        var fireNodes = _game.SpawnFireNodes(area.Center, 25, 5f, FireNodeType.Flamethrower);
        ScheduleEndFireNodes(fireNodes);
    }

    public override MagicBase Copy()
    {
        return new FireMagic(Wizard, _args) { _ignitedPlayers = _ignitedPlayers };
    }

    public override void Cast(Area fullArea, Area iterationArea)
    {
        var objects = GetAffectedObjectsByArea(fullArea, iterationArea);

        foreach (var @object in objects)
        {
            BurnObject(@object);
        }

        PlayEffects(iterationArea);

        base.Cast(fullArea, iterationArea);
    }

    private void BurnObject(IObject @object)
    {
        if (@object is not IPlayer player)
        {
            if (!@object.IsBurning)
            {
                @object.SetMaxFire();
            }

            return;
        }

        BurnPlayer(player);
    }

    private void BurnPlayer(IPlayer playerInstance)
    {
        var wizardInstance = Wizard.Instance;

        if (!playerInstance.IsValid() || playerInstance.IsDead)
            return;

        if (playerInstance == wizardInstance)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);

        if (_ignitedPlayers?.Contains(player) == true)
            return;

        var igniteBuff = new Ignite(player);
        var result = _buffsService.TryAddBuff(igniteBuff, player);

        if (!result.IsT0)
            return;

        _ignitedPlayers ??= [];
        _ignitedPlayers.Add(player);
    }

    private void ScheduleEndFireNodes(FireNode[] fireNodes)
    {
        Awaiter.Start(() =>
        {
            foreach (var fireNode in fireNodes)
            {
                _game.EndFireNode(fireNode.InstanceID);
            }
        }, TimeSpan.FromMilliseconds(PropagationTime.TotalMilliseconds / IterationsCount));
    }
}