using System;
using System.Collections.Generic;
using System.Linq;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups.Bullets;

internal abstract class BulletsPowerupBase : UsablePowerupBase<Firearm>
{
    public abstract override string Name { get; }
    public override int UsesCount => Math.Min(Math.Max(Weapon.MagSize, 3), Weapon.MaxTotalAmmo / 2);
    public sealed override Firearm Weapon { get; protected set; }

    public override int UsesLeft
    {
        get => _usesLeft ??= Math.Min(UsesCount, Weapon.MaxTotalAmmo);
        protected set => _usesLeft = value;
    }

    protected virtual bool CountShotgunBulletsIndependently => true;

    private int? _usesLeft;

    protected BulletsPowerupBase(Firearm firearm, PowerupConstructorArgs args) : base(args)
    {
        Weapon = firearm;
    }

    public abstract override IWeaponPowerup<Firearm> Clone(Weapon weapon);

    protected override void OnFireInternal(IPlayer playerInstance, IEnumerable<IProjectile> projectilesEnumerable)
    {
        if (Weapon.IsBoobyTrapped)
            return;
        
        // _usesLeft = Math.Min(UsesLeft, Weapon.TotalAmmo + projectiles.Count);

        var isShotgun = Weapon is Shotgun;
        var count = isShotgun
            ? (CountShotgunBulletsIndependently ? UsesLeft : UsesLeft * ((Shotgun)Weapon).BulletsPerShot)
            : UsesLeft;

        foreach (var projectile in projectilesEnumerable.Take(count))
        {
            OnFireInternal(playerInstance, projectile);

            if (!isShotgun || isShotgun && CountShotgunBulletsIndependently)
            {
                _usesLeft--;
            }
        }

        if (isShotgun && !CountShotgunBulletsIndependently)
        {
            _usesLeft--;
        }
    }

    protected abstract void OnFireInternal(IPlayer playerInstance, IProjectile projectile);
}