using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.SourceGenerators.ExtendedEvents.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

internal abstract class GrenadesPowerupBase : UsablePowerupBase<Grenade>
{
    public abstract override string Name { get; }
    public abstract Color PaintColor { get; }
    public abstract override int UsesCount { get; }
    public sealed override Grenade Weapon { get; protected set; }

    public override int UsesLeft
    {
        get => _usesLeft ??= Math.Min(UsesCount, Weapon.MaxAmmo);
        protected set => _usesLeft = value;
    }

    protected abstract override IEnumerable<Type> IncompatiblePowerups { get; }

    private readonly IGame _game;

    private int? _usesLeft;

    protected GrenadesPowerupBase(Grenade grenade, PowerupConstructorArgs args) : base(args)
    {
        Weapon = grenade;
        _game = args.Game;
    }

    public abstract override IWeaponPowerup<Grenade> Clone(Weapon weapon);

    protected override void OnRunInternal()
    {
        Weapon.Throw += OnWeaponDropped;
        Weapon.Drop += OnWeaponDropped;

        if (Weapon.IsDropped)
        {
            CreatePaint(Weapon.ObjectId, ExtendedEvents);
        }
    }

    protected override void OnDisposeInternal()
    {
        Weapon.Throw -= OnWeaponDropped;
        Weapon.Drop -= OnWeaponDropped;
    }

    private void OnWeaponDropped(IObjectWeaponItem? objectWeaponItem, Weapon weapon)
    {
        ArgumentWasNullException.ThrowIfNull(objectWeaponItem);
        CreatePaint(objectWeaponItem, ExtendedEvents);
    }

    protected override void OnThrowInternal(IPlayer? player, IObject? thrown, Throwable? throwable)
    {
        var grenadeThrown = thrown as IObjectGrenadeThrown;
        ArgumentWasNullException.ThrowIfNull(grenadeThrown);
        ArgumentWasNullException.ThrowIfNull(player);
        ArgumentWasNullException.ThrowIfNull(throwable);

        var grenade = CreateGrenadeAndPaint(grenadeThrown);
        grenade.Initialize();

        // _usesLeft = Math.Min(Weapon.CurrentAmmo, UsesLeft - 1);

        _usesLeft--;
    }

    protected abstract GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents);

    protected GrenadeBase CreateGrenadeAndPaint(IObjectGrenadeThrown grenadeThrown)
    {
        var grenade = CreateGrenade(grenadeThrown, _game, ExtendedEvents);
        CreatePaint(grenadeThrown, ExtendedEvents);

        return grenade;
    }

    private void CreatePaint(int objectId, IExtendedEvents extendedEvents) =>
        CreatePaint(_game.GetObject(objectId), extendedEvents);

    private void CreatePaint(IObject @object, IExtendedEvents extendedEvents)
    {
        var grenadeIndicator = new GrenadeIndicator(@object, extendedEvents);
        grenadeIndicator.Paint(PaintColor);
    }

    protected abstract class GrenadeBase
    {
        private readonly IObjectGrenadeThrown _grenade;
        private readonly IExtendedEvents _extendedEvents;

        private IEventSubscription? _disposeEventSubscription;

        protected GrenadeBase(IObjectGrenadeThrown grenade, IExtendedEvents extendedEvents)
        {
            _grenade = grenade;
            _extendedEvents = extendedEvents;
        }

        public virtual void Initialize()
        {
            _disposeEventSubscription =
                _extendedEvents.HookOnDestroyed(_grenade, _ => Dispose(), EventHookMode.Default);
            _grenade.SetDudChance(0f);
        }

        protected virtual void Dispose()
        {
            _disposeEventSubscription?.Dispose();
        }
    }
}