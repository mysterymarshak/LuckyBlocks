using LuckyBlocks.Data.Weapons.Unsafe;
using SFDGameScriptInterface;

namespace LuckyBlocks.Data.Weapons;

internal sealed record MeleeTemp(WeaponItem WeaponItem, WeaponItemType WeaponItemType, float CurrentDurability)
    : Melee(WeaponItem, WeaponItemType, CurrentDurability)
{
    public override bool IsDrawn => !IsDropped && Owner!.CurrentWeaponDrawn == WeaponItemType;

    public void Update(in UnsafeMeleeTemp newData)
    {
        CurrentDurability = newData.CurrentDurability;
    }
}