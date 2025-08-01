using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.WeaponPowerups;
using Mediator;

namespace LuckyBlocks.Mediator;

internal readonly record struct UserRegisteredNotification(Player Player) : INotification;

internal class UserRegisteredNotificationHandler : INotificationHandler<UserRegisteredNotification>
{
    private readonly IBuffsService _buffsService;
    private readonly IWeaponPowerupsService _weaponPowerupsService;

    public UserRegisteredNotificationHandler(IBuffsService buffsService, IWeaponPowerupsService weaponPowerupsService)
    {
        _buffsService = buffsService;
        _weaponPowerupsService = weaponPowerupsService;
    }

    public ValueTask Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;

        _buffsService.InitializePlayer(player);
        _weaponPowerupsService.InitializePlayer(player);

        return new ValueTask();
    }
}