using System;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;

namespace LuckyBlocks.Loot.Events;

internal class RemoveWeapons : GameEventWithSlowMoBase
{
    public override Item Item => Item.RemoveWeapons;
    public override string Name => "Remove weapons";

    protected override TimeSpan SlowMoDuration => TimeSpan.FromMilliseconds(1500);

    private readonly IIdentityService _identityService;

    public RemoveWeapons(LootConstructorArgs args) : base(args)
    {
        _identityService = args.IdentityService;
    }

    protected override void OnSlowMoEnded()
    {
        var alivePlayers = _identityService.GetAlivePlayers();
        foreach (var player in alivePlayers)
        {
            var playerInstance = player.Instance!;
            playerInstance.RemoveAllWeapons();
        }
    }
}