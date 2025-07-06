using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;

namespace LuckyBlocks.Loot.Other;

internal class ShuffleWeapons : GameEventWithSlowMoBase
{
    public override Item Item => Item.ShuffleWeapons;
    public override string Name => "Shuffle weapons";

    protected override TimeSpan SlowMoDuration => TimeSpan.FromMilliseconds(1500);

    private readonly IIdentityService _identityService;

    public ShuffleWeapons(LootConstructorArgs args) : base(args)
    {
        _identityService = args.IdentityService;
    }

    protected override void OnSlowMoEnded()
    {
        var players = _identityService.GetAlivePlayers().ToList();
        var shuffledPlayers = players.ShuffleAndGuaranteeIndexChanging();

        var newWeaponsInfo = new Dictionary<Player, WeaponsData>(players.Count);

        for (var i = 0; i < players.Count; i++)
        {
            var player = players[i];
            var shuffledPlayer = shuffledPlayers[i];
            newWeaponsInfo.Add(player, shuffledPlayer.WeaponsData);
        }

        foreach (var pair in newWeaponsInfo)
        {
            var player = pair.Key;
            var weaponsData = pair.Value;

            player.SetWeapons(weaponsData, true);
        }
    }
}