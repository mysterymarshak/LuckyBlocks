using System;
using LuckyBlocks.Data;
using LuckyBlocks.Exceptions;
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

    private const int PoisonDelayMin = 1250;
    private const int PoisonDelayMax = 2000;
    private const int DamagePills = 50;
    private const int DamageMedkit = 75;

    private readonly Vector2 _spawnPosition;
    private readonly IBuffsService _buffsService;
    private readonly INotificationService _notificationService;
    private readonly IIdentityService _identityService;
    private readonly IWeaponsDataWatcher _weaponsDataWatcher;
    private readonly IGame _game;

    private int _damage;

    public MedkitPoisoned(Vector2 spawnPosition, LootConstructorArgs args)
    {
        _spawnPosition = spawnPosition;
        _buffsService = args.BuffsService;
        _notificationService = args.NotificationService;
        _identityService = args.IdentityService;
        _weaponsDataWatcher = args.WeaponsDataWatcher;
        _game = args.Game;
    }

    public void Run()
    {
        var className = SharedRandom.Instance.NextDouble() > 0.5 ? "ItemPills" : "ItemMedkit";
        _damage = className == "ItemPills" ? DamagePills : DamageMedkit;

        var medkit = (_game.CreateObject(className, _spawnPosition) as IObjectWeaponItem)!;
        var weapon = _weaponsDataWatcher.RegisterWeapon(medkit);
        weapon.PickUp += OnPickedUp;
    }

    private void OnPickedUp(Weapon weapon)
    {
        var playerInstance = weapon.Owner;
        ArgumentWasNullException.ThrowIfNull(playerInstance);

        weapon.PickUp -= OnPickedUp;

        var delay = TimeSpan.FromMilliseconds(SharedRandom.Instance.Next(PoisonDelayMin, PoisonDelayMax));
        Awaiter.Start(() => Poison(playerInstance), delay);
    }

    private void Poison(IPlayer playerInstance)
    {
        if (!playerInstance.IsValid() || playerInstance.IsDead)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);

        var poison = new Poison(player, _damage, _notificationService);
        var addBuffResult = _buffsService.TryAddBuff(poison, player);

        if (addBuffResult.IsT2)
        {
            _notificationService.CreateChatNotification(
                $"'{playerInstance.Name}' wasn't poisoned, because he had an immunity", Color.White);
        }
    }
}