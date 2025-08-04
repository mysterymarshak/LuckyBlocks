using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.WeaponPowerups.Projectiles;
using LuckyBlocks.Utils;
using LuckyBlocks.Utils.Timers;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Bullets;

// name and effect are a reference to orange juice card
// https://100orangejuice.fandom.com/wiki/Lost_Child
// in the game when u hold this card
// your character moves backwards
internal class LostBullets : BulletsPowerupBase
{
    public static readonly IReadOnlyCollection<Type> IncompatiblePowerups = [];
    
    public override string Name => "Lost bullets";

    public override int UsesCount => Math.Max(base.UsesCount,
        (Weapon is Shotgun ? (Weapon.TotalAmmo / 2) : Weapon.TotalAmmo));

    protected override IReadOnlyCollection<Type> IncompatiblePowerupsInternal => IncompatiblePowerups;
    protected override bool CountShotgunBulletsIndependently => false;

    private readonly IProjectilesService _projectilesService;
    private readonly IGame _game;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly PowerupConstructorArgs _args;
    private readonly PeriodicTimer _effectTimer;

    public LostBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _projectilesService = args.ProjectilesService;
        _game = args.Game;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
        _effectTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(250), TimeBehavior.TimeModifier, PlayLostEffect,
            null, int.MaxValue, ExtendedEvents);
    }

    public override IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new LostBullets(firearm, _args) { UsesLeft = UsesLeft };
    }

    protected override void OnRunInternal()
    {
        if (Weapon.IsDropped)
        {
            _effectTimer.Restart();
        }

        Weapon.Drop += OnWeaponDropped;
        Weapon.Throw += OnWeaponDropped;
    }

    protected override void OnDisposeInternal()
    {
        Weapon.Drop -= OnWeaponDropped;
        Weapon.Throw -= OnWeaponDropped;
    }

    protected override void OnFireInternal(IPlayer playerInstance, IProjectile projectile)
    {
        _projectilesService.AddPowerup<LostProjectile>(projectile, _args);
    }

    private void OnWeaponDropped(IObjectWeaponItem? objectWeaponItem, Weapon weapon)
    {
        _effectTimer.Restart();
    }

    private void PlayLostEffect()
    {
        if (!Weapon.IsDropped)
        {
            _effectTimer.Stop();
            return;
        }

        var weaponObject = _game.GetObject(Weapon.ObjectId);
        if (weaponObject is null)
        {
            _effectTimer.Stop();
            return;
        }

        _effectsPlayer.PlayLostEffect(weaponObject.GetWorldPosition());
    }
}