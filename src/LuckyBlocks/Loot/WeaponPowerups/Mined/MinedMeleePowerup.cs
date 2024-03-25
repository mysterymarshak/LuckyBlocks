using System.Linq;
using LuckyBlocks.Data;
using LuckyBlocks.Utils;
using SFDGameScriptInterface;

namespace LuckyBlocks.Loot.WeaponPowerups.Mined;

internal class MinedMeleePowerup : IWeaponPowerup<Melee>
{
    public string Name => "Mined melee";
    public Melee Weapon { get; }

    private readonly INotificationService _notificationService;
    private readonly IGame _game;

    private IPlayer? _owner;
    private Events.PlayerKeyInputCallback? _keyInputCallback;

    public MinedMeleePowerup(Melee melee, PowerupConstructorArgs args)
        => (Weapon, _notificationService, _game) = (melee, args.NotificationService, args.Game);

    public void OnRan(IPlayer player)
    {
        _owner = player;
        _keyInputCallback = Events.PlayerKeyInputCallback.Start(OnKeyPressed);
    }

    public void OnWeaponPickedUp(IPlayer player)
    {
        _owner = player;
    }

    public void OnWeaponDropped(IPlayer player, IObjectWeaponItem? objectWeaponItem)
    {
    }

    private void OnKeyPressed(IPlayer player, VirtualKeyInfo[] args)
    {
        if (player.UniqueId != _owner!.UniqueId)
            return;

        var meleeDrawn = args.Any(virtualKeyInfo => virtualKeyInfo is
            { Key: VirtualKey.DRAW_MELEE, Event: VirtualKeyEvent.Pressed });

        if (!meleeDrawn)
            return;

        TriggerExplosion();
        _notificationService.CreateChatNotification("HAHAHHAHA, WEAPON WAS MINED, LOOOZER", ExtendedColors.ImperialRed,
            player.UserIdentifier);

        _owner!.RemoveWeaponItemType(WeaponItemType.Melee);
        _keyInputCallback?.Stop();
    }

    private void TriggerExplosion()
    {
        var position = _owner!.GetWorldPosition();
        _game.TriggerExplosion(position);
    }
}