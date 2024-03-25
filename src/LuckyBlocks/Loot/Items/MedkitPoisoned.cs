using System;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Loot.Buffs.Instant;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Items;

internal class MedkitPoisoned : ILoot
{
    public Item Item => Item.MedkitPoisoned;
    public string Name => "Medkit";

    private const int POISON_DELAY_MIN = 1250;
    private const int POISON_DELAY_MAX = 2000;
    private const int DAMAGE_PILLS = 50;
    private const int DAMAGE_MEDKIT = 75;

    private readonly Vector2 _spawnPosition;
    private readonly IBuffsService _buffsService;
    private readonly INotificationService _notificationService;
    private readonly IIdentityService _identityService;
    private readonly IGame _game;

    private WeaponEventsWatcher? _weaponEventsWatcher;
    private int _damage;

    public MedkitPoisoned(Vector2 spawnPosition, LootConstructorArgs args)
        => (_spawnPosition, _buffsService, _notificationService, _identityService, _game) =
            (spawnPosition, args.BuffsService, args.NotificationService, args.IdentityService, args.Game);

    public void Run()
    {
        var className = SharedRandom.Instance.NextDouble() > 0.5 ? "ItemPills" : "ItemMedkit";
        _damage = className == "ItemPills" ? DAMAGE_PILLS : DAMAGE_MEDKIT;

        var medkit = (_game.CreateObject(className, _spawnPosition) as IObjectWeaponItem)!;

        _weaponEventsWatcher = WeaponEventsWatcher.CreateForWeapon(medkit);
        _weaponEventsWatcher.Pickup += OnPickup;
        _weaponEventsWatcher.Start();
    }

    private void OnPickup(IPlayer player)
    {
        var delay = TimeSpan.FromMilliseconds(SharedRandom.Instance.Next(POISON_DELAY_MIN, POISON_DELAY_MAX));
        Awaiter.Start(() => Poison(player), delay);
        Dispose();
    }

    private void Poison(IPlayer playerInstance)
    {
        if (!playerInstance.IsValid() || playerInstance.IsDead)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);
        
        var poison = new Poison(player, _damage, _notificationService);
        var addBuffResult = _buffsService.TryAddBuff(poison, player);

        if (addBuffResult.IsT0)
            return;

        if (addBuffResult.IsT2)
        {
            _notificationService.CreateChatNotification($"'{playerInstance.Name}' wasn't poisoned, because he had an immunity",
                Color.White);
        }
    }

    private void Dispose()
    {
        _weaponEventsWatcher?.Dispose();
    }
}