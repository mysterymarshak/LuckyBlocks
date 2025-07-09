using System;
using LuckyBlocks.Data;
using LuckyBlocks.Data.Weapons;
using LuckyBlocks.Exceptions;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Mined;

internal class MinedMeleePowerup : IStackablePowerup<Melee>
{
    public string Name => "Mined melee";
    public Melee Weapon { get; private set; }

    private IPlayer? Player => Weapon.Owner;

    private readonly INotificationService _notificationService;
    private readonly IGame _game;
    private readonly PowerupConstructorArgs _args;

    public MinedMeleePowerup(Melee melee, PowerupConstructorArgs args)
    {
        Weapon = melee;
        _notificationService = args.NotificationService;
        _game = args.Game;
        _args = args;
    }

    public IWeaponPowerup<Melee> Clone(Weapon weapon)
    {
        var melee = weapon as Melee;
        ArgumentWasNullException.ThrowIfNull(melee);
        return new MinedMeleePowerup(melee, _args);
    }

    public void Run()
    {
        Weapon.Draw += OnDrawn;
    }

    public bool IsCompatibleWith(Type otherPowerupType) => true;

    public void Stack(IStackablePowerup<Weapon> powerup)
    {
    }

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