using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed record Shotgun(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxTotalAmmo,
    int MagSize,
    int CurrentSpareMags,
    int MaxSpareMags,
    bool IsLazerEquipped,
    ProjectilePowerupData ProjectilePowerupData,
    ProjectileItem ProjectileItem,
    int BulletsPerShot) : Firearm(WeaponItem, WeaponItemType, CurrentAmmo, MaxTotalAmmo, MagSize, CurrentSpareMags,
    MaxSpareMags, IsLazerEquipped, ProjectilePowerupData, ProjectileItem);