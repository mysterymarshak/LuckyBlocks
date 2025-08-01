using System;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Extensions;
using LuckyBlocks.Mediator;
using Mediator;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups;

internal class ChangedAmmo : IStackablePowerup<Weapon>
{
    public string Name => "Changed ammo";
    public bool IsHidden => true;
    public Weapon Weapon { get; private set; }

    private readonly IMediator _mediator;
    private readonly IGame _game;

    private float _ammo;
    private float _time;

    public ChangedAmmo(Weapon weapon, IMediator mediator, IGame game)
    {
        Weapon = weapon;
        _mediator = mediator;
        _game = game;
        _ammo = -1;
    }

    public IWeaponPowerup<Weapon> Clone(Weapon copiedWeapon)
    {
        return new ChangedAmmo(Weapon, _mediator, _game);
    }

    public void SetAmmo(float ammo)
    {
        _ammo = ammo;
        _time = _game.TotalElapsedGameTime;
    }

    public void Run()
    {
        if (Weapon is not (Firearm or Throwable or Melee))
        {
            throw new InvalidOperationException($"cannot apply ChangedAmmo powerup to {Weapon.GetType()}");
        }

        if (_ammo < 0)
        {
            throw new InvalidOperationException("powerup isnt initialized");
        }

        if (Weapon.IsDropped)
        {
            Weapon.PickUp += OnPickup;
            return;
        }

        OnPickup(Weapon, Weapon.Owner!);
    }

    public void Stack(IStackablePowerup<Weapon> powerup)
    {
        var changedAmmoPowerup = (ChangedAmmo)powerup;
        _ammo = changedAmmoPowerup._time > _time ? changedAmmoPowerup._ammo : _ammo;
    }

    public bool IsCompatibleWith(Type otherPowerupType) => true;

    public void MoveToWeapon(Weapon otherWeapon)
    {
        Weapon = otherWeapon;
        Run();
    }

    public void Dispose()
    {
        Weapon.PickUp -= OnPickup;
    }

    private void OnPickup(Weapon weapon, IPlayer playerInstance)
    {
        if (Weapon is Firearm firearm)
        {
            playerInstance.SetAmmo(firearm, (int)_ammo);
        }
        else if (Weapon is Throwable throwable)
        {
            playerInstance.SetAmmo(throwable, (int)_ammo);
        }
        else if (Weapon is Melee melee)
        {
            // in luckyblocks i use 0.0-1.0f range instead of 0.0-100f
            melee.SetDurability(_ammo / 100f);
        }

        OnFinish();
    }

    private void OnFinish()
    {
        Dispose();

        var notification = new WeaponPowerupFinishedNotification(this, Weapon);
        _mediator.Publish(notification);
    }
}