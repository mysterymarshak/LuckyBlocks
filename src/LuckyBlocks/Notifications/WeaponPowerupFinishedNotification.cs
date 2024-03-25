using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Data;
using LuckyBlocks.Features.WeaponPowerups;
using LuckyBlocks.Loot.WeaponPowerups;
using Mediator;

namespace LuckyBlocks.Notifications;

internal readonly record struct WeaponPowerupFinishedNotification(IWeaponPowerup<Weapon> Powerup, Weapon Weapon) : INotification;

internal class WeaponPowerupFinishedNotificationHandler : INotificationHandler<WeaponPowerupFinishedNotification>
{
    private readonly IWeaponsPowerupsService _weaponsPowerupsService;

    public WeaponPowerupFinishedNotificationHandler(IWeaponsPowerupsService weaponsPowerupsService)
        => (_weaponsPowerupsService) = (weaponsPowerupsService);

    public ValueTask Handle(WeaponPowerupFinishedNotification notification, CancellationToken cancellationToken)
    {
        var powerup = notification.Powerup;
        var weapon = notification.Weapon;

        _weaponsPowerupsService.RemovePowerup(powerup, weapon);

        return new ValueTask();
    }
}