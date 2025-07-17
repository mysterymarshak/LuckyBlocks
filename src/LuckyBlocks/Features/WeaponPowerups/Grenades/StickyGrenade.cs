using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Objects;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Watchers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Grenades;

internal class StickyGrenade : GrenadeBase
{
    private const int CollisionVectorLength = 3;

    private static readonly IReadOnlyList<Vector2> CollisionVectors = Enumerable.Range(0, 12)
        .Select(x => x * 30)
        .Select(x => x * Math.PI / 180)
        .Select(x => new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) * CollisionVectorLength)
        .ToList();

    private readonly IGame _game;
    private readonly IExtendedEvents _extendedEvents;
    private readonly Action<IObject, IExtendedEvents> _createPaintDelegate;

    private IObjectWeldJoint? _objectWeldJoint;
    private Events.UpdateCallback? _collisionCheckCallback;
    private Events.PlayerDamageCallback? _playerDamageCallback;
    private PortalsWatcher? _portalsWatcher;
    private Vector2 _attachOffset;
    private MappedObject? _attachObject;
    private float _explosionTimer;

    public StickyGrenade(IObjectGrenadeThrown grenade, IGame game, IExtendedEvents extendedEvents,
        Action<IObject, IExtendedEvents> createPaintDelegate) : base(grenade, extendedEvents, createPaintDelegate)
    {
        _game = game;
        _extendedEvents = extendedEvents;
        _createPaintDelegate = createPaintDelegate;
    }

    public override void Initialize()
    {
        if (IsCloned)
        {
            Grenade.SetExplosionTimer(_explosionTimer);

            if (_attachObject is not null)
            {
                StartFollowing(_attachObject);
                base.Initialize();
                return;
            }
        }

        _collisionCheckCallback = Events.UpdateCallback.Start(OnUpdate);
        _playerDamageCallback = Events.PlayerDamageCallback.Start(OnPlayerDamaged);

        base.Initialize();
    }

    protected override GrenadeBase CloneInternal()
    {
        return new StickyGrenade(Grenade, _game, _extendedEvents, _createPaintDelegate)
        {
            _attachObject = _attachObject, _attachOffset = _attachOffset, _explosionTimer = Grenade.GetExplosionTimer()
        };
    }

    protected override void Dispose()
    {
        _playerDamageCallback?.Stop();
        _collisionCheckCallback?.Stop();
        _objectWeldJoint?.RemoveDelayed();
        _portalsWatcher?.Dispose();

        base.Dispose();
    }

    private void OnUpdate(float e)
    {
        var position = Grenade.GetAABB().Center;
        var raycastInput = new RayCastInput
        {
            ClosestHitOnly = true,
            IncludeOverlap = false,
            FilterOnMaskBits = true,
            MaskBits = ushort.MaxValue ^ 0b100
        };

        var result = CollisionVectors
            .Select(x =>
            {
                var result = _game.RayCast(position, position + x, raycastInput)[0];
                return result.HitObject?.UniqueId == Grenade.UniqueId ? new RayCastResult() : result;
            })
            .FirstOrDefault(x => x.Hit);

        // var result = CollisionVectors
        //     .Select(x => _game.RayCast(position, position + x, raycastInput).First())
        //     .Where(x => x.Hit)
        //     .OrderBy(x => (x.Position - position).Length())
        //     .FirstOrDefault();

#if DEBUG
        if (_game.IsEditorTest)
        {
            foreach (var collisionVector in CollisionVectors)
            {
                _game.DrawLine(position, position + collisionVector, Color.Red);
            }
        }
#endif

        if (!result.Hit)
            return;

        var @object = _game.GetObject(result.ObjectID);
        StartFollowing(@object);
    }

    private void OnPlayerDamaged(IPlayer player, PlayerDamageArgs args)
    {
        if (args.DamageType != PlayerDamageEventType.Missile)
            return;

        if (Grenade.UniqueId != args.SourceID)
            return;

        StartFollowing(player);
    }

    private void StartFollowing(IObject @object)
    {
        _playerDamageCallback?.Stop();
        _collisionCheckCallback?.Stop();

        _objectWeldJoint = (_game.CreateObject("WeldJoint", Grenade.GetWorldPosition()) as IObjectWeldJoint)!;
        _objectWeldJoint.AddTargetObject(@object);
        _objectWeldJoint.AddTargetObject(Grenade);

        if (_attachObject is null)
        {
            _attachOffset = @object.GetWorldPosition() - Grenade.GetWorldPosition();
            _attachObject = @object.ToMappedObject();
        }

        if (@object is IPlayer playerInstance)
        {
            _portalsWatcher = new PortalsWatcher(playerInstance, _game, _extendedEvents,
                portalExitedCallback: OnPortalExited);
            _portalsWatcher.Initialize();
        }
    }

    private void OnPortalExited(IObjectPortal portal)
    {
        var grenadePosition = Grenade.GetWorldPosition();
        Grenade.SetWorldPosition(grenadePosition + _attachOffset);
    }
}