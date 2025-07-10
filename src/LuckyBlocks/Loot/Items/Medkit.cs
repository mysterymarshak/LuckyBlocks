using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Loot.Buffs.Instant;
using LuckyBlocks.Loot.WeaponPowerups;
using LuckyBlocks.Loot.Weapons;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.Items;

internal class Medkit : PowerUppedWeaponBase
{
    public override Item Item => Item.Medkit;
    public override string Name => "Medkit";

    protected override WeaponItem WeaponItem { get; }

    private int PoisonDamage => WeaponItem == WeaponItem.PILLS ? DamagePills : DamageMedkit;

    private const double PoisonedChance = 0.5;
    private const int PoisonDelayMin = 1250;
    private const int PoisonDelayMax = 2000;
    private const int DamagePills = 50;
    private const int DamageMedkit = 75;

    private readonly IIdentityService _identityService;
    private readonly INotificationService _notificationService;
    private readonly IBuffsService _buffsService;
    private readonly bool _isPoisoned;

    public Medkit(Vector2 spawnPosition, LootConstructorArgs args) : base(spawnPosition, args)
    {
        _identityService = args.IdentityService;
        _notificationService = args.NotificationService;
        _buffsService = args.BuffsService;
        WeaponItem = SharedRandom.Instance.NextDouble() > 0.5 ? WeaponItem.PILLS : WeaponItem.MEDKIT;
        _isPoisoned = SharedRandom.Instance.NextDouble() <= PoisonedChance;
    }

    protected override void OnWeaponRegistered(Weapon weapon)
    {
        if (!_isPoisoned)
            return;

        weapon.PickUp += OnPickedUp;
    }

    protected override IEnumerable<IWeaponPowerup<Weapon>> GetPowerups(Weapon weapon)
    {
        yield break;
    }

    private void OnPickedUp(Weapon weapon, IPlayer playerInstance)
    {
        weapon.PickUp -= OnPickedUp;

        var delay = TimeSpan.FromMilliseconds(SharedRandom.Instance.Next(PoisonDelayMin, PoisonDelayMax));
        Awaiter.Start(() => Poison(playerInstance), delay);
    }

    private void Poison(IPlayer playerInstance)
    {
        if (!playerInstance.IsValid() || playerInstance.IsDead)
            return;

        var player = _identityService.GetPlayerByInstance(playerInstance);

        var poison = new Poison(player, PoisonDamage, _notificationService);
        var addBuffResult = _buffsService.TryAddBuff(poison, player);

        if (addBuffResult.IsT2)
        {
            _notificationService.CreateChatNotification(
                $"'{playerInstance.Name}' wasn't poisoned, because he had an immunity", Color.White);
        }
    }
}