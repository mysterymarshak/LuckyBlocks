using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Buffs.Instant;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class IgniteRandomPlayer : ILoot
{
    public Item Item => Item.IgniteRandomPlayer;
    public string Name => "Ignite random player";

    private readonly IIdentityService _identityService;
    private readonly IBuffsService _buffsService;
    private readonly INotificationService _notificationService;

    public IgniteRandomPlayer(LootConstructorArgs args)
        => (_identityService, _buffsService, _notificationService) =
            (args.IdentityService, args.BuffsService, args.NotificationService);

    public void Run()
    {
        var alivePlayers = _identityService.GetAlivePlayers().ToList();
        var player = alivePlayers.GetRandomElement();

        var igniteBuff = new Ignite(player);
        var addBuffResult = _buffsService.TryAddBuff(igniteBuff, player);

        if (addBuffResult.IsT0)
        {
            _notificationService.CreateChatNotification($"{player.Name}, BURN, SUKA, BURN!1!!!1",
                ExtendedColors.Orange);
            return;
        }

        if (addBuffResult.IsT2)
        {
            _notificationService.CreateChatNotification($"{player.Name} wasn't ignited, because he had an immunity",
                Color.White);
        }
    }
}