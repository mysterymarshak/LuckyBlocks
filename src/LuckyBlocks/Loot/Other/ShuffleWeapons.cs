using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class ShuffleWeapons : ILoot
{
    public Item Item => Item.ShuffleWeapons;
    public string Name => "Shuffle weapons";

    private readonly IIdentityService _identityService;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly INotificationService _notificationService;
    private readonly IWeaponPowerupsService _weaponPowerupsService;

    public ShuffleWeapons(LootConstructorArgs args)
        => (_identityService, _effectsPlayer, _notificationService, _weaponPowerupsService) =
            (args.IdentityService, args.EffectsPlayer, args.NotificationService, args.WeaponsPowerupsService);

    public void Run()
    {
        var slowMoDuration = TimeSpan.FromMilliseconds(1500);

        _effectsPlayer.PlaySloMoEffect(slowMoDuration);
        _notificationService.CreatePopupNotification("SHUFFLE WEAPONS", ExtendedColors.ImperialRed, slowMoDuration);

        Awaiter.Start(OnSlowMoEnded, slowMoDuration);
    }

    private void OnSlowMoEnded()
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
            var playerInstance = player.Instance!;
            var weaponsData = pair.Value;
            
            playerInstance.SetWeapons(weaponsData);
            player.SetWeaponsData(weaponsData);
        }
    }
}