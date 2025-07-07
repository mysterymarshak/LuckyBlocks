using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed record InstantPickupItem(WeaponItem WeaponItem, WeaponItemType WeaponItemType)
    : Weapon(WeaponItem, WeaponItemType);