using System;
using System.Collections.Generic;
using LuckyBlocks.Data.Args;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Features.Notifications;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Features.WeaponPowerups;

internal class BoobyTrapped : IStackablePowerup<Weapon>
{
    public string Name => "Booby-trapped";
    public Weapon Weapon { get; private set; }

    private IPlayer? Player => Weapon.Owner;

    private readonly IGame _game;
    private readonly INotificationService _notificationService;
    private readonly PowerupConstructorArgs _args;

    public BoobyTrapped(Weapon weapon, PowerupConstructorArgs args)
    {
        Weapon = weapon;
        _game = args.Game;
        _notificationService = args.NotificationService;
        _args = args;
    }

    public IWeaponPowerup<Weapon> Clone(Weapon copiedWeapon)
    {
        return new BoobyTrapped(copiedWeapon, _args);
    }

    public void Run()
    {
        switch (Weapon)
        {
            case Melee melee:
                melee.Draw += OnDrawn;
                break;
            case Firearm firearm:
                firearm.Fire += OnFired;
                break;
            case Throwable throwable:
                throwable.Activate += OnActivated;
                break;
            default:
                throw new InvalidOperationException($"cannot booby trap weapon {Weapon}");
        }
    }

    private void OnDrawn(Weapon weapon) => Explode();

    private void OnFired(Weapon weapon, IPlayer playerInstance, IEnumerable<IProjectile> projectilesEnumerable)
    {
        foreach (var projectile in projectilesEnumerable)
        {
            projectile.FlagForRemoval();
        }

        Explode();
    }

    private void OnActivated(Weapon weapon) => Explode();


    public bool IsCompatibleWith(Type otherPowerupType) => true;

    public void Stack(IStackablePowerup<Weapon> powerup)
    {
    }

    public void MoveToWeapon(Weapon otherWeapon)
    {
        Weapon = otherWeapon;
        Run();
    }

    public void Dispose()
    {
        switch (Weapon)
        {
            case Melee melee:
                melee.Draw -= OnDrawn;
                break;
            case Firearm firearm:
                firearm.Fire -= OnFired;
                break;
            case Throwable throwable:
                throwable.Activate -= OnActivated;
                break;
        }
    }

    private void Explode()
    {
        ArgumentWasNullException.ThrowIfNull(Player);

        Player.RemoveWeaponItemType(Weapon.WeaponItemType);
        _game.TriggerExplosion(Player.GetWorldPosition());

        _notificationService.CreateChatNotification("HAHAHHAHA, WEAPON WAS BOOBY-TRAPPED, LOOOZER",
            ExtendedColors.ImperialRed, Player.UserIdentifier);
    }
}