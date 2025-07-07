using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed record Flamethrower(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxTotalAmmo,
    int MagSize,
    int CurrentSpareMags,
    int MaxSpareMags,
    bool IsLazerEquipped,
    ProjectilePowerupData ProjectilePowerupData,
    ProjectileItem ProjectileItem)
    : Firearm(WeaponItem, WeaponItemType, CurrentAmmo, MaxTotalAmmo, MagSize, CurrentSpareMags, MaxSpareMags,
        IsLazerEquipped, ProjectilePowerupData, ProjectileItem);