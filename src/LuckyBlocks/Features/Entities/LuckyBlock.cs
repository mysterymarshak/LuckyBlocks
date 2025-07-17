using LuckyBlocks.Data.Args;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Loot;
using LuckyBlocks.Mediator;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.Entities;

internal class LuckyBlock : IEntity
{
    public int ObjectId => _crate.UniqueId;

    private Vector2 Position => _crate.AsIObject().GetWorldPosition();

    private readonly MappedObject _crate;
    private readonly IMediator _mediator;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Item _predefinedItem;

    private bool _isBroken;
    private IEventSubscription? _damageSubscription;

    public LuckyBlock(IObjectSupplyCrate crate, IMediator mediator, IExtendedEvents extendedEvents,
        Item predefinedItem = Item.None)
    {
        _crate = crate.ToMappedObject();
        _mediator = mediator;
        _extendedEvents = extendedEvents;
        _predefinedItem = predefinedItem;
    }

    public void Initialize()
    {
        var crate = (IObjectSupplyCrate)_crate;
        crate.SetHealth(1);
        crate.SetSupplyCategoryType(SupplyCategoryType.NONE);
        crate.SetWeaponItem(WeaponItem.NONE);
        crate.CustomId = "LuckyBlock";
        _damageSubscription = _extendedEvents.HookOnDamage(_crate, OnDamaged, EventHookMode.Default);
    }

    public IEntity Clone()
    {
        return new LuckyBlock((IObjectSupplyCrate)_crate, _mediator, _extendedEvents);
    }

    public void Dispose()
    {
        _damageSubscription?.Dispose();
    }

    private void OnDamaged(Event<ObjectDamageArgs> @event)
    {
        var crate = _crate.AsIObject();
        if (_isBroken || crate.GetHealth() > 0)
            return;

        var args = @event.Args;
        OnBroken(args.IsPlayer, args.SourceID, crate.CustomId);
    }

    private void OnBroken(bool isPlayer, int playerId, string customId)
    {
        _isBroken = true;
        Dispose();

        var args = new LuckyBlockBrokenArgs(ObjectId, Position, isPlayer, playerId, customId != "RemovedLuckyBlock",
            _predefinedItem);
        var notification = new LuckyBlockBrokenNotification(args);
        _mediator.Publish(notification);
    }
}