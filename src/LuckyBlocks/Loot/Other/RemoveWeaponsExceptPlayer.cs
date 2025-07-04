using System;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class RemoveWeaponsExceptPlayer : GameEventWithSlowMoBase
{
    public override Item Item => Item.RemoveWeaponsExceptPlayer;
    public override string Name => "Remove weapons";

    protected override TimeSpan SlowMoDuration => TimeSpan.FromMilliseconds(1500);

    private readonly IPlayer _playerInstance;
    private readonly IIdentityService _identityService;

    public RemoveWeaponsExceptPlayer(IPlayer playerInstance, LootConstructorArgs args) : base(args)
    {
        _playerInstance = playerInstance;
        _identityService = args.IdentityService;
    }

    protected override void OnSlowMoEnded()
    {
        var alivePlayers = _identityService.GetAlivePlayers();
        foreach (var player in alivePlayers)
        {
            var playerInstance = player.Instance!;
            if (playerInstance == _playerInstance)
                continue;

            playerInstance.RemoveAllWeapons();
        }
    }
}