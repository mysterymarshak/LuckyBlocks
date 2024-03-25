﻿using LuckyBlocks.Data;
using LuckyBlocks.Features.LuckyBlocks;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Other;

internal class IncreaseSpawnChance : ILoot
{
    public Item Item => Item.IncreaseSpawnChance;
    public string Name => "Lucky blocks drop chance increase";

    private readonly ISpawnChanceService _spawnChanceService;
    private readonly INotificationService _notificationService;

    public IncreaseSpawnChance(LootConstructorArgs args)
        => (_spawnChanceService, _notificationService) = (args.SpawnChanceService, args.NotificationService);

    public void Run()
    {
        _spawnChanceService.Increase();

        _notificationService.CreateChatNotification(
            $"LUCKY BLOCKS DROP CHANCE INCREASED TO {_spawnChanceService.Chance * 100}%", Color.White);
    }
}