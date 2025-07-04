using System;
using Autofac;
using LuckyBlocks.Data;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Extensions;
using LuckyBlocks.Mathematics;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using Serilog;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Melees;

internal class FlamyKatana : IWeaponPowerup<Melee>
{
    public string Name => "Flamy katana";
    public Melee Weapon { get; private set; }

    private static TimeSpan Lifetime => TimeSpan.FromSeconds(15);

    private IPlayer? Player => Weapon.Owner;

    private readonly IEffectsPlayer _effectsPlayer;
    private readonly IGame _game;
    private readonly ILogger _logger;
    private readonly IExtendedEvents _extendedEvents;

    private TimerBase _drawnTimer = null!;
    private TimerBase _durabilityTimer = null!;
    private TimerBase _objectWeaponTimer = null!;

    public FlamyKatana(Melee melee, PowerupConstructorArgs args)
    {
        Weapon = melee;
        _effectsPlayer = args.EffectsPlayer;
        _logger = args.Logger;
        _game = args.Game;
        var thisScope = args.LifetimeScope.BeginLifetimeScope();
        _extendedEvents = thisScope.Resolve<IExtendedEvents>();
    }

    public void Run()
    {
        Weapon.Draw += OnDraw;
        Weapon.MeleeHit += OnMeleeHit;

        _durabilityTimer = new PeriodicTimer<Weapon>(TimeSpan.Zero, TimeBehavior.TimeModifier, UpdateDurability,
            x => !x.IsDrawn, default, Weapon, _extendedEvents);

        _drawnTimer = new PeriodicTimer<Weapon>(TimeSpan.FromMilliseconds(300), TimeBehavior.TimeModifier, weapon =>
        {
            var owner = weapon.Owner!;
            var offset = owner.IsMeleeAttacking || owner.IsJumpAttacking
                ? new Vector2(15 * owner.GetFaceDirection(), 7)
                : new Vector2(-5 * owner.GetFaceDirection(), 15);

            _effectsPlayer.PlayEffect(EffectName.FireTrail, owner.GetWorldPosition() + offset);
        }, x => !x.IsDrawn, null, Weapon, _extendedEvents);

        _objectWeaponTimer = new PeriodicTimer<Weapon>(TimeSpan.FromMilliseconds(100), TimeBehavior.TimeModifier,
            weapon =>
            {
                var objectWeaponItem = _game.GetObject(weapon.ObjectId);
                _effectsPlayer.PlayEffect(EffectName.FireTrail, objectWeaponItem.GetWorldPosition());
            }, weapon => !weapon.IsDropped, null, Weapon, _extendedEvents);

        if (Weapon.IsDropped)
        {
            AddFlameToObject();
        }
    }

    public bool IsCompatibleWith(Type otherPowerupType) => true;

    public void MoveToWeapon(Weapon otherWeapon)
    {
        if (otherWeapon is not Melee melee)
        {
            throw new InvalidCastException("cannot cast otherWeapon to melee");
        }

        Weapon = melee;
        Run();
    }

    public void Dispose()
    {
        Weapon.Draw -= OnDraw;
        Weapon.MeleeHit -= OnMeleeHit;

        _drawnTimer.Stop();
        _objectWeaponTimer.Stop();
        _durabilityTimer.Stop();
    }

    private void OnDraw(Weapon weapon)
    {
        AddFlameToDrawn();

        _durabilityTimer.Reset();
        _durabilityTimer.Start();
    }

    private void OnMeleeHit(PlayerMeleeHitArg args)
    {
        var hitObject = args.HitObject;
        if (hitObject.GetBodyType() == BodyType.Static)
            return;

        hitObject.SetMaxFire();
    }

    private void UpdateDurability(Weapon weapon)
    {
        Weapon.SetDurability(Weapon.CurrentDurability -
                             1 / ((float)Lifetime.TotalMilliseconds / _durabilityTimer.ElapsedFromPreviousTick));

        if (Weapon.CurrentDurability == 0)
        {
            Player!.SetMaxFire();
        }
    }

    private void AddFlameToDrawn()
    {
        _drawnTimer.Reset();
        _drawnTimer.Start();
    }

    private void AddFlameToObject()
    {
        _objectWeaponTimer.Reset();
        _objectWeaponTimer.Start();
    }
}