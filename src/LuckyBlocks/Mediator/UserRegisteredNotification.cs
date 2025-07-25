using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using Mediator;

namespace LuckyBlocks.Mediator;

internal readonly record struct UserRegisteredNotification(Player Player) : INotification;

internal class UserRegisteredNotificationHandler : INotificationHandler<UserRegisteredNotification>
{
    private readonly IBuffsService _buffsService;

    public UserRegisteredNotificationHandler(IBuffsService buffsService)
    {
        _buffsService = buffsService;
    }
    
    public ValueTask Handle(UserRegisteredNotification notification, CancellationToken cancellationToken)
    {
        var player = notification.Player;
        _buffsService.InitializePlayer(player);
        
        return new ValueTask();
    }
}