﻿using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Features.WeaponPowerups;
using Mediator;

namespace LuckyBlocks.Mediator;

internal readonly record struct WeaponPowerupFinishedNotification(IWeaponPowerup<Weapon> Powerup, Weapon Weapon)
    : INotification;

internal class WeaponPowerupFinishedNotificationHandler : INotificationHandler<WeaponPowerupFinishedNotification>
{
    private readonly IWeaponPowerupsService _weaponPowerupsService;

    public WeaponPowerupFinishedNotificationHandler(IWeaponPowerupsService weaponPowerupsService)
        => (_weaponPowerupsService) = (weaponPowerupsService);

    public ValueTask Handle(WeaponPowerupFinishedNotification notification, CancellationToken cancellationToken)
    {
        var powerup = notification.Powerup;
        var weapon = notification.Weapon;

        _weaponPowerupsService.RemovePowerup(powerup, weapon);

        return new ValueTask();
    }
}