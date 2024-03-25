using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LuckyBlocks.Data;
using LuckyBlocks.Entities;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Buffs;
using LuckyBlocks.Features.Identity;
using LuckyBlocks.Features.ShockedObjects;
using LuckyBlocks.Loot.Buffs.Durable;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Notifications;

internal readonly record struct ObjectsTouchedShockObjectNotification(ShockedObject ShockedObject,
    IEnumerable<IObject> Objects) : INotification;

internal class
    ObjectsTouchedShockObjectNotificationHandler : INotificationHandler<ObjectsTouchedShockObjectNotification>
{
    private readonly IShockedObjectsService _shockedObjectsService;
    private readonly BuffConstructorArgs _buffConstructorArgs;
    private readonly IBuffsService _buffsService;
    private readonly IIdentityService _identityService;
    
    public ObjectsTouchedShockObjectNotificationHandler(IShockedObjectsService shockedObjectsService,
        BuffConstructorArgs buffConstructorArgs, IBuffsService buffsService, IIdentityService identityService) =>
        (_shockedObjectsService, _buffConstructorArgs, _buffsService, _identityService) =
        (shockedObjectsService, buffConstructorArgs, buffsService, identityService);

    public ValueTask Handle(ObjectsTouchedShockObjectNotification notification, CancellationToken cancellationToken)
    {
        var shockedObject = notification.ShockedObject;
        var touchedObjects = notification.Objects;

        foreach (var @object in touchedObjects)
        {
            if (_shockedObjectsService.IsShocked(@object))
                continue;
            
            if (shockedObject.Charge <= ShockedObject.MinCharge)
                break;
            
            var shockTime = shockedObject.TimeLeft.Divide(2);
            shockedObject.TimeLeft = shockTime;
            
            if (@object is IPlayer { IsUser: true } playerInstance)
            {
                var player = _identityService.GetPlayerByInstance(playerInstance);
                if (player.HasBuff(typeof(Shock)))
                    continue;
                
                var shock = new Shock(player, _buffConstructorArgs, shockTime);
                _buffsService.TryAddBuff(shock, player);
            }
            else
            {
                _shockedObjectsService.Shock(@object, shockTime);
            }
        }

        return new ValueTask();
    }
}