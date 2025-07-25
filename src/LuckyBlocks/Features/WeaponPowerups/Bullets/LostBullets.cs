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
    public override string Name => "Lost bullets";

    public override int UsesCount => Math.Max(base.UsesCount,
        (Weapon is Shotgun ? (Weapon.TotalAmmo / 2) : Weapon.TotalAmmo));

    protected override IEnumerable<Type> IncompatiblePowerups => _incompatiblePowerups;
    protected override bool CountShotgunBulletsIndependently => false;

    private static readonly List<Type> _incompatiblePowerups = [];

    private readonly IProjectilesService _projectilesService;
    private readonly IGame _game;
    private readonly IEffectsPlayer _effectsPlayer;
    private readonly PowerupConstructorArgs _args;
    private readonly PeriodicTimer<Weapon> _effectTimer;

    public LostBullets(Firearm firearm, PowerupConstructorArgs args) : base(firearm, args)
    {
        _projectilesService = args.ProjectilesService;
        _game = args.Game;
        _effectsPlayer = args.EffectsPlayer;
        _args = args;
        _effectTimer = new PeriodicTimer<Weapon>(TimeSpan.FromMilliseconds(250), TimeBehavior.TimeModifier,
            PlayLostEffect,
            x => !x.IsDropped, null, Weapon, ExtendedEvents);
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
            _effectTimer.Start();
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

    private void PlayLostEffect(Weapon weapon)
    {
        var weaponObject = _game.GetObject(Weapon.ObjectId);
        if (weaponObject is null)
        {
            _effectTimer.Stop();
            return;
        }

        _effectsPlayer.PlayLostEffect(weaponObject.GetWorldPosition());
    }
}