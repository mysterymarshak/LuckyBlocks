using LuckyBlocks.Data;
using LuckyBlocks.Notifications;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Entities;

internal class LuckyBlock
{
    public int Id => _crate.UniqueID;

    private Vector2 Position => _crate.GetWorldPosition();

    private readonly IObjectSupplyCrate _crate;
    private readonly IMediator _mediator;
    private readonly IExtendedEvents _events;

    private bool _broken;

    public LuckyBlock(IObjectSupplyCrate crate, IMediator mediator, IExtendedEvents extendedEvents)
        => (_crate, _mediator, _events) = (crate, mediator, extendedEvents);

    public void Init()
    {
        _crate.SetHealth(1);
        _crate.SetSupplyCategoryType(SupplyCategoryType.NONE);
        _crate.SetWeaponItem(WeaponItem.NONE);
        _events.HookOnDamage(_crate, OnDamaged, EventHookMode.Default);
    }

    private void OnDamaged(Event<ObjectDamageArgs> @event)
    {
        if (_broken || _crate.GetHealth() > 0)
            return;

        var args = @event.Args;
        OnBroken(args.IsPlayer, args.SourceID);
    }

    private void OnBroken(bool isPlayer, int playerId)
    {
        _broken = true;
        Dispose();

        var args = new LuckyBlockBrokenArgs(Id, Position, isPlayer, playerId);
        var notification = new LuckyBlockBrokenNotification(args);
        _mediator.Publish(notification);
    }

    private void Dispose()
    {
        _events.Clear();
    }
}