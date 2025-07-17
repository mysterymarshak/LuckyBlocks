using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Events;

internal class ShufflePositions : GameEventWithSlowMoBase
{
    public override Item Item => Item.ShufflePositions;
    public override string Name => "Shuffle positions";

    protected override TimeSpan SlowMoDuration => TimeSpan.FromMilliseconds(1500);

    private readonly IIdentityService _identityService;

    public ShufflePositions(LootConstructorArgs args) : base(args)
    {
        _identityService = args.IdentityService;
    }

    protected override void OnSlowMoEnded()
    {
        var players = _identityService.GetAlivePlayers()
            .Select(x => x.Instance!)
            .ToList();
        var shuffledPlayers = players.ShuffleAndGuaranteeIndexChanging();

        var newMovementData = new Dictionary<IPlayer, MovementData>(players.Count);

        for (var i = 0; i < players.Count; i++)
        {
            var position = shuffledPlayers[i].GetWorldPosition();
            var velocity = shuffledPlayers[i].GetLinearVelocity();

            var movementData = new MovementData(position, velocity);
            newMovementData.Add(players[i], movementData);
        }

        foreach (var pair in newMovementData)
        {
            var player = pair.Key;
            var movementData = pair.Value;

            player.SetWorldPosition(movementData.Position);
            player.SetLinearVelocity(movementData.Velocity);
        }
    }

    private record MovementData(Vector2 Position, Vector2 Velocity);
}