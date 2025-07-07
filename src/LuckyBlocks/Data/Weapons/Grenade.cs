using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed record Grenade(
    WeaponItem WeaponItem,
    WeaponItemType WeaponItemType,
    int CurrentAmmo,
    int MaxAmmo,
    bool IsActive,
    float TimeToExplosion)
    : Throwable(WeaponItem, WeaponItemType, CurrentAmmo, MaxAmmo, IsActive)
{
    public float TimeToExplosion { get; private set; } = TimeToExplosion;

    public void Update(bool isActive, float timeToExplosion)
    {
        IsActive = isActive;
        TimeToExplosion = timeToExplosion;
    }
}