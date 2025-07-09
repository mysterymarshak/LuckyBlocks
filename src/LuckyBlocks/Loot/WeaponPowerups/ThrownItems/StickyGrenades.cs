using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Features.Watchers;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.ThrownItems;

internal class StickyGrenades : GrenadesPowerupBase
{
    public override Color PaintColor => ExtendedColors.Pink;
    public override string Name => "Sticky grenades";
    public override int UsesCount => 3;

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;

    private static readonly List<Type> _incompatiblePowerups = [typeof(BananaGrenades)];

    private readonly PowerupConstructorArgs _args;

    public StickyGrenades(Grenade grenade, PowerupConstructorArgs args) : base(grenade, args)
    {
        _args = args;
    }

    public override IWeaponPowerup<Grenade> Clone(Weapon weapon)
    {
        var grenade = weapon as Grenade;
        ArgumentWasNullException.ThrowIfNull(grenade);
        return new StickyGrenades(grenade, _args) { UsesLeft = UsesLeft };
    }

    protected override GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game,
        IExtendedEvents extendedEvents)
    {
        return new StickyGrenade(grenadeThrown, game, extendedEvents);
    }

    private class StickyGrenade : GrenadeBase
    {
        private const int CollisionVectorLength = 3;

        private static readonly IReadOnlyList<Vector2> CollisionVectors = Enumerable.Range(0, 12)
            .Select(x => x * 30)
            .Select(x => x * Math.PI / 180)
            .Select(x => new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) * CollisionVectorLength)
            .ToList();

        private readonly IObjectGrenadeThrown _grenade;
        private readonly IGame _game;
        private readonly IExtendedEvents _extendedEvents;

        private IObjectWeldJoint? _objectWeldJoint;
        private Events.UpdateCallback? _collisionCheckCallback;
        private Events.PlayerDamageCallback? _playerDamageCallback;
        private PortalsWatcher? _portalsWatcher;
        private bool _isAttached;
        private Vector2 _attachOffset;

        public StickyGrenade(IObjectGrenadeThrown grenade, IGame game, IExtendedEvents extendedEvents) : base(grenade,
            extendedEvents)
        {
            _grenade = grenade;
            _game = game;
            _extendedEvents = extendedEvents;
        }

        public override void Initialize()
        {
            _collisionCheckCallback = Events.UpdateCallback.Start(OnUpdate);
            _playerDamageCallback = Events.PlayerDamageCallback.Start(OnPlayerDamaged);

            base.Initialize();
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
            if (_isAttached)
                return;

            var position = _grenade.GetAABB().Center;
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
                    return result.HitObject?.UniqueId == _grenade.UniqueId ? new RayCastResult() : result;
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

            if (_grenade.UniqueId != args.SourceID)
                return;

            StartFollowing(player);
        }

        private void StartFollowing(IObject @object)
        {
            _objectWeldJoint = (_game.CreateObject("WeldJoint", _grenade.GetWorldPosition()) as IObjectWeldJoint)!;
            _objectWeldJoint.AddTargetObject(@object);
            _objectWeldJoint.AddTargetObject(_grenade);

            _isAttached = true;
            _attachOffset = @object.GetWorldPosition() - _grenade.GetWorldPosition();

            if (@object is IPlayer playerInstance)
            {
                _portalsWatcher = new PortalsWatcher(playerInstance, _game, _extendedEvents,
                    portalExitedCallback: OnPortalExited);
                _portalsWatcher.Initialize();
            }
        }

        private void OnPortalExited(IObjectPortal portal)
        {
            var grenadePosition = _grenade.GetWorldPosition();
            _grenade.SetWorldPosition(grenadePosition + _attachOffset);
        }
    }
}