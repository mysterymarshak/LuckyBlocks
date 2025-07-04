﻿using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Extensions;
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
    
    public StickyGrenades(Grenade grenade, PowerupConstructorArgs args) : base(grenade, args)
    {
    }

    protected override GrenadeBase CreateGrenade(IObjectGrenadeThrown grenadeThrown, IGame game, IExtendedEvents extendedEvents)
    {
        return new StickyGrenade(grenadeThrown, game, extendedEvents);
    }

    private class StickyGrenade : GrenadeBase
    {
        private const int CollisionVectorLength = 2;
        private static readonly IReadOnlyList<Vector2> CollisionVectors = Enumerable.Range(0, 360)
            .Where(x => x % 10 == 0)
            .Select(x => x * Math.PI / 180)
            .Select(x => new Vector2((float)Math.Cos(x), (float)Math.Sin(x)) * CollisionVectorLength)
            .ToList();

        private readonly IObjectGrenadeThrown _grenade;
        private readonly IGame _game;
        
        private IObjectWeldJoint? _objectWeldJoint;
        private Events.UpdateCallback? _collisionCheckCallback;
        private Events.PlayerDamageCallback? _playerDamageCallback;
        
        public StickyGrenade(IObjectGrenadeThrown grenade, IGame game, IExtendedEvents extendedEvents) : base(grenade, extendedEvents)
        {
            _grenade = grenade;
            _game = game;
        }

        public override void Initialize()
        {
            _collisionCheckCallback = Events.UpdateCallback.Start(OnCollisionCheckCallback);
            _playerDamageCallback = Events.PlayerDamageCallback.Start(OnPlayerDamaged);
            
            base.Initialize();
        }

        private void OnCollisionCheckCallback(float e)
        {
            var position = _grenade.GetWorldPosition();
            var raycastInput = new RayCastInput
            {
                ClosestHitOnly = true,
                IncludeOverlap = false,
                FilterOnMaskBits = true,
                MaskBits = ushort.MaxValue ^ 0b100
            };

            var raycastResults =
                CollisionVectors.Select(x => _game.RayCast(position, position + x, raycastInput).First());
            var result = raycastResults
                .Where(x => x.Hit)
                .OrderBy(x => (x.Position - position).Length())
                .FirstOrDefault();

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
            _objectWeldJoint = (_game.CreateObject("WeldJoint") as IObjectWeldJoint)!;
            _objectWeldJoint.AddTargetObject(@object);
            _objectWeldJoint.AddTargetObject(_grenade);
        }

        protected override void Dispose()
        {
            _playerDamageCallback?.Stop();
            _collisionCheckCallback?.Stop();
            _objectWeldJoint?.RemoveDelayed();
            
            base.Dispose();
        }
    }
}