using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Features.ShockedObjects;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Notifications;

internal readonly record struct ShockObjectEndedNotification(IObject Object) : INotification;

internal class ShockObjectEndedNotificationHandler : INotificationHandler<ShockObjectEndedNotification>
{
    private readonly IShockedObjectsService _shockedObjectsService;

    public ShockObjectEndedNotificationHandler(IShockedObjectsService shockedObjectsService)
        => (_shockedObjectsService) = (shockedObjectsService);
    
    public ValueTask Handle(ShockObjectEndedNotification notification, CancellationToken cancellationToken)
    {
        var @object = notification.Object;
        _shockedObjectsService.OnShockEnded(@object);

        return new ValueTask();
    }
}
