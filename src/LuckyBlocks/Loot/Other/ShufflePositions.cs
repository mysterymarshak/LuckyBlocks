using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class ShufflePositions : ILoot
{
    public Item Item => Item.ShufflePositions;
    public string Name => "Shuffle positions";

    private readonly IIdentityService _identityService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly INotificationService _notificationService;

    public ShufflePositions(LootConstructorArgs args)
        => (_identityService, _effectsPlayer, _notificationService) =
            (args.IdentityService, args.EffectsPlayer, args.NotificationService);

    public void Run()
    {
        var slowMoDuration = TimeSpan.FromMilliseconds(1500);

        _effectsPlayer.PlaySloMoEffect(slowMoDuration);
        _notificationService.CreatePopupNotification("SHUFFLE POSITIONS", ExtendedColors.ImperialRed, slowMoDuration);

        Awaiter.Start(OnSlowMoEnded, slowMoDuration);
    }

    private void OnSlowMoEnded()
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