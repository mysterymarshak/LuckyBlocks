using System.Collections.Generic;
using LuckyBlocks.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Mined;

internal class MinedFirearmPowerup : IFirearmPowerup
{
    public string Name => "Mined weapon";
    public Firearm Weapon { get; }

    private readonly INotificationService _notificationService;
    private readonly IGame _game;

    public MinedFirearmPowerup(Firearm firearm, PowerupConstructorArgs args)
        => (Weapon, _notificationService, _game) = (firearm, args.NotificationService, args.Game);

    public void OnRan(IPlayer player)
    {
    }

    public void OnWeaponPickedUp(IPlayer player)
    {
    }

    public void OnWeaponDropped(IPlayer player, IObjectWeaponItem? objectWeaponItem)
    {
    }

    public void OnFire(IPlayer player, IEnumerable<IProjectile> projectiles)
    {
        player.RemoveWeaponItemType(Weapon.WeaponItemType);
        _game.TriggerExplosion(player.GetWorldPosition());
        _notificationService.CreateChatNotification("HAHAHHAHA, WEAPON WAS MINED, LOOOZER", ExtendedColors.ImperialRed,
            player.UserIdentifier);
    }
}