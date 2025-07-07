using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed record PowerupItem(WeaponItem WeaponItem, WeaponItemType WeaponItemType)
    : Weapon(WeaponItem, WeaponItemType);