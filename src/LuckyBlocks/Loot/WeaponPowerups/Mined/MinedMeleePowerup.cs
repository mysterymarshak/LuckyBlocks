using System;
using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Mined;

internal class MinedMeleePowerup : IWeaponPowerup<Melee>
{
    public string Name => "Mined melee";
    public Melee Weapon { get; private set; }

    private IPlayer? Player => Weapon.Owner;

    private readonly INotificationService _notificationService;
    private readonly IGame _game;

    public MinedMeleePowerup(Melee melee, PowerupConstructorArgs args)
        => (Weapon, _notificationService, _game) = (melee, args.NotificationService, args.Game);

    public void Run()
    {
        Weapon.Draw += OnDrawn;
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
        Weapon.Draw -= OnDrawn;
    }

    private void OnDrawn(Weapon weapon)
    {
        ArgumentWasNullException.ThrowIfNull(Player);

        Player.RemoveWeaponItemType(Weapon.WeaponItemType);
        _game.TriggerExplosion(Player.GetWorldPosition());

        _notificationService.CreateChatNotification("HAHAHHAHA, WEAPON WAS MINED, LOOOZER", ExtendedColors.ImperialRed,
            Player.UserIdentifier);
    }
}