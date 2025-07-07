using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Mined;

internal class MinedFirearmPowerup : IWeaponPowerup<Firearm>
{
    public string Name => "Mined weapon";
    public Firearm Weapon { get; private set; }

    private readonly INotificationService _notificationService;
    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public MinedFirearmPowerup(Firearm firearm, PowerupConstructorArgs args)
    {
        Weapon = firearm;
        _notificationService = args.NotificationService;
        _game = args.Game;
        _args = args;
    }

    public IWeaponPowerup<Firearm> Clone(Weapon weapon)
    {
        var firearm = weapon as Firearm;
        ArgumentWasNullException.ThrowIfNull(firearm);
        return new MinedFirearmPowerup(firearm, _args);
    }

    public void Run()
    {
        Weapon.Fire += OnFired;
    }

    public bool IsCompatibleWith(Type otherPowerupType) => true;

    public void MoveToWeapon(Weapon otherWeapon)
    {
        if (otherWeapon is not Firearm firearm)
        {
            throw new InvalidCastException("cannot cast otherWeapon to firearm");
        }

        Weapon = firearm;
        Run();
    }

    public void Dispose()
    {
        Weapon.Fire -= OnFired;
    }

    private void OnFired(IPlayer? player, IEnumerable<IProjectile>? projectiles)
    {
        ArgumentWasNullException.ThrowIfNull(player);
        ArgumentWasNullException.ThrowIfNull(projectiles);

        foreach (var projectile in projectiles)
        {
            projectile.FlagForRemoval();
        }

        player.RemoveWeaponItemType(Weapon.WeaponItemType);
        _game.TriggerExplosion(player.GetWorldPosition());

        _notificationService.CreateChatNotification("HAHAHHAHA, WEAPON WAS MINED, LOOOZER", ExtendedColors.ImperialRed,
            player.UserIdentifier);
    }
}