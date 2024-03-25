using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Buffs.Instant;
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

    private List<int>? _ignitedPlayers;

    public FireMagic(Player wizard, BuffConstructorArgs args, int direction = default) : base(wizard, args, direction)
        => (_buffsService, _identityService, _game) = (args.BuffsService, args.IdentityService, args.Game);

    public override void PlayEffects(Area area)
    {
        var fireNodes = _game.SpawnFireNodes(area.Center, 25, 5f, FireNodeType.Flamethrower);
        ScheduleEndFireNodes(fireNodes);
    }

    protected override void CastInternal(Area area)
    {
        var objects = GetObjectsByArea(area);

        foreach (var obj in objects)
        {
            BurnObject(obj);
        }
        
        PlayEffects(area);
    }
    
    private void BurnObject(IObject obj)
    {
        if (obj is not IPlayer player)
        {
            obj.SetMaxFire();
            return;
        }

        BurnPlayer(player);
    }

    private void BurnPlayer(IPlayer playerInstance)
    {
        var wizardInstance = Wizard.Instance;
        
        if (playerInstance.IsDead)
            return;
        
        if (playerInstance.UniqueId == wizardInstance?.UniqueId)
            return;

        if (_ignitedPlayers?.Contains(playerInstance.UniqueId) == true)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);
        var igniteBuff = new Ignite(player);
        var result = _buffsService.TryAddBuff(igniteBuff, player);

        if (!result.IsT0)
            return;

        _ignitedPlayers ??= new();
        _ignitedPlayers.Add(playerInstance.UniqueID);
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